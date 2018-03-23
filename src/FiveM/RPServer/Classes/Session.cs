using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Helpers;
using FamilyRP.Common;
using System;
using FamilyRP.Roleplay.SharedClasses;
using Newtonsoft.Json;
using System.Threading;
using System.Data.Common;
using System.Threading.Tasks;
using FamilyRP.Roleplay.Enums.Session;
using FamilyRP.Roleplay.Enums.City;
using MySql.Data.MySqlClient;
using FamilyRP.Roleplay.Server.Classes.Vehicle;
using FamilyRP.Roleplay.SharedModels;
using FamilyRP.Roleplay.Server.Shops;
using FamilyRP.Roleplay.Server.Classes.Services;
using FamilyRP.Roleplay.Enums.Character;

namespace FamilyRP.Roleplay.Server
{
	public class Session
	{
		// Session data
		public string NetID { get; private set; }
		public string Name { get; private set; }
		public string[] Identities { get; private set; }
		public Models.SteamID SteamID { get; private set; } = null;

		// Player data
		public int UserID { get; set; }
        public Privilege Privilege { get; set; }

		// Player states
		public bool HasSpawned { get; private set; }
		public bool IsLoggedIn { get { return UserID > 0; } private set { } }
		public bool IsPlaying { get { return Character != null; } private set { } }

		// Game objects
		public SharedModels.CharacterData Character { get; private set; }

		// Aliases
		public int Ping => Function.Call<int>( Hash.GET_PLAYER_PING, NetID );
		public int LastMsg => Function.Call<int>( Hash.GET_PLAYER_LAST_MSG, NetID );
		public string EndPoint => Function.Call<string>( Hash.GET_PLAYER_ENDPOINT, NetID );
        public bool IsDeveloper => Privilege.HasFlag(Privilege.DEV);
        public bool IsAdmin => Privilege.HasFlag(Privilege.ADMIN);
		public void Drop( string reason ) => Function.Call( Hash.DROP_PLAYER, NetID, reason );

		public Player Player { get; private set; }
		public SemaphoreSlim Mutex { get; private set; }

		#region Session class logic
		/// <summary>
		/// Creates a new session based on the given CitizenFX.Core.Player object
		/// </summary>
		/// <param name="player">Valid CitizenFX.Core.Player object</param>
		public Session( Player player ) {
			Mutex = new SemaphoreSlim( 1, 1 );
			Player = player;
			NetID = player.Handle;

			// Fetch identities
			int numIdents = Function.Call<int>( Hash.GET_NUM_PLAYER_IDENTIFIERS, NetID );
			List<string> idents = new List<string>();

			for( int i = 0; i < numIdents; i++ ) {
				idents.Add( Function.Call<string>( Hash.GET_PLAYER_IDENTIFIER, NetID, i ) );
			}

			Identities = idents.ToArray();

			// Fetch steamID
			if( Identities.Length > 0 ) {
				foreach( string identity in Identities ) {
					Models.SteamID steamID = new Models.SteamID( identity );

					if( steamID.IsValid() ) {
						SteamID = steamID;
						break;
					}
				}
			}

			// Set unknowns
			Character = null;
			UserID = 0;
			Privilege = Enums.Session.Privilege.NONE;
			HasSpawned = false;
		}

		public async void SetPrivilege( Privilege Privilege, bool targetState ) {
			try {
				if( Privilege == Privilege.ADMIN ) {
					using( var conn = DBManager.GetConnection() ) {
						using( var cmd = conn.GetCommand() ) {
							cmd.CommandText = @"UPDATE Users SET IsAdmin = @IsAdmin WHERE UserId = @UserId;";
							cmd.Parameters.AddWithValue( "IsAdmin", targetState );
							cmd.Parameters.AddWithValue( "UserId", this.UserID );
							await cmd.ExecuteNonQueryAsync();
						}
					}
					if( targetState )
						this.Privilege |= Privilege;
					else
						this.Privilege &= ~Privilege;

					BaseScript.TriggerClientEvent( "Session.PrivilegesUpdated", NetID, (int)this.Privilege );
				}
				else if( Privilege == Privilege.DEV ) {
					using( var conn = DBManager.GetConnection() ) {
						using( var cmd = conn.GetCommand() ) {
							cmd.CommandText = @"UPDATE Users SET IsDeveloper = @IsDeveloper WHERE UserId = @UserId;";
							cmd.Parameters.AddWithValue( "IsDeveloper", targetState );
							cmd.Parameters.AddWithValue( "UserId", this.UserID );
							await cmd.ExecuteNonQueryAsync();
						}
					}
					if( targetState )
						this.Privilege |= Privilege;
					else
						this.Privilege &= ~Privilege;

					BaseScript.TriggerClientEvent( "Session.PrivilegesUpdated", NetID, (int)this.Privilege );
					Log.Debug( $"Session.PrivilegesUpdated Trigger sent: {NetID} {this.Privilege}" );
				}
			}
			catch( Exception ex ) {
				Log.Error( $"Session SetPrivilege: {ex}" );
			}
		}

		/// <summary>
		/// Trigger an event for this session only.
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="args"></param>
		public void TriggerEvent( string eventName, params object[] args ) {
			var argsSerialized = Helpers.MsgPack.BinarySerialize( args );

			unsafe
			{
				fixed ( byte* serialized = &argsSerialized[0] ) {
					Function.Call( Hash.TRIGGER_CLIENT_EVENT_INTERNAL, eventName, NetID, serialized, argsSerialized.Length );
				}
			}
		}

		/// <summary>
		/// Activates this session, adding it to the player and public character list.
		/// </summary>
		/// <param name="session"></param>
		/// <returns></returns>
		private bool Activate() {
			if( Character == null || Character.CharID == 0 || SessionManager.PlayerList.ContainsKey( Character.CharID ) ) {
				// Character ID is already loaded in or characterdata is not loaded yet
				return false;
			}

			SessionManager.PlayerList[Character.CharID] = this;
			SessionManager.PublicCharacterList[Convert.ToInt32( NetID )] = Character;

			return true;
		}

		/// <summary>
		/// Deactivates this session. Should not be happening under normal circumstances.
		/// </summary>
		private void Deactivate() {
			SessionManager.PublicCharacterList.Remove( Convert.ToInt32( NetID ) );

			if( Character != null && Character.CharID > 0 ) {
				SessionManager.PlayerList.Remove( Character.CharID );
			}
		}

		/// <summary>
		/// Generates a JSON list of available spawns for this character
		/// </summary>
		/// <returns></returns>
		private string GenerateSpawnJson() {
			if( Character == null )
				return "{}";

			SpawnLocationList spawnLocations = new SpawnLocationList();

			// Add player owned houses

			if( Character.Duty.HasFlag( Duty.Police ) ) {
				// Add police station spawns
				spawnLocations.List.Add( GameData.SpecialSpawnLocations[SpawnLocations.LSPD_MISSION_ROW] );
				spawnLocations.List.Add( GameData.SpecialSpawnLocations[SpawnLocations.LSPD_PALETO_BAY] );
				spawnLocations.List.Add( GameData.SpecialSpawnLocations[SpawnLocations.LSPD_SANDY_SHORES] );
			}

			if( Character.Duty.HasFlag( Duty.EMS ) ) {
				// Add hospital spawns
				spawnLocations.List.Add( GameData.SpecialSpawnLocations[SpawnLocations.LSMC_DAVIS] );
				spawnLocations.List.Add( GameData.SpecialSpawnLocations[SpawnLocations.LSMC_PALETO_BAY] );
				spawnLocations.List.Add( GameData.SpecialSpawnLocations[SpawnLocations.LSMC_ROCKFORD_HILLS] );
				spawnLocations.List.Add( GameData.SpecialSpawnLocations[SpawnLocations.LSMC_SANDY_SHORES] );
				spawnLocations.List.Add( GameData.SpecialSpawnLocations[SpawnLocations.LSMC_STRAWBERRY_AVE] );
			}

			if( Character.Duty.HasFlag( Duty.FireDept ) ) {
				// Add fire department spawns
			}

			if( Character.Duty.HasFlag( Duty.Military ) ) {
				// Add military spawns
			}

			if( Character.Duty.HasFlag( Duty.Tow ) ) {
				// Add tow depot spawns
			}

			// Add locations that can always be picked
			spawnLocations.List.AddRange( GameData.GuestSpawnLocations );

			// Serialize data for NUI
			spawnLocations.Serialize();

			return Helpers.MsgPack.Serialize( spawnLocations );
		}
		#endregion

		#region Events
		/// <summary>
		/// Called on server event: playerConnecting
		/// </summary>
		//public void Connected( string playerName ) {
		//	Debug.WriteLine( "Player connected: {0} (NetID: {1}, SteamID: {2})", playerName, NetID, SteamID );
		//}

		/// <summary>
		/// Called on client event: Session.Loaded
		/// </summary>
		//public void Loaded() {
		//	// Immediately drop invalid steam IDs. This should be caught by the access module, so this check is redundant.
		//	if( !Models.SteamID.IsValid( SteamID ) ) {
		//		TriggerEvent( "UI.Notification", "~r~INVALID STEAM TICKET~s~ You will now be disconnected." );
		//		Function.Call( Hash.DROP_PLAYER, NetID, "Invalid steam ticket" );
		//		return;
		//	}

		//	// Reset the session if it's being reloaded
		//	if( HasSpawned ) {
		//		//TODO: To be implemented
		//		return;
		//	}

		//	// Client will be accepted - load additional data (client is finished loading the map at this point)
		//	HasSpawned = true;

		//	TriggerEvent( "Session.LoadData", Helpers.MsgPack.Serialize( SessionManager.PublicCharacterList ) );
		//	TriggerEvent( "Items.LoadClientItems", Items.SerializedItems );

		//	// Find out if this player's SteamID exists. If not, automatically create a new account for it
		//	try {
		//		using( MySqlConnection conn = DBManager.GetConnection() ) {
		//			using( MySqlCommand cmd = conn.GetCommand() ) {
		//				cmd.CommandText = "SELECT `UserID`, `IsAdmin`, `IsDeveloper`, (`BannedUntil` IS NOT NULL AND `BannedUntil` > NOW()) AS `IsBanned` FROM `Users` WHERE `SteamID`=@SteamID LIMIT 1";
		//				cmd.Parameters.AddWithValue( "@SteamID", SteamID.Hex64 );

		//				using( DbDataReader res = cmd.ExecuteReader() ) {
		//					if( res.HasRows && res.Read() ) {
		//						UserID = Parse.Int32( res["UserID"] );

		//						if( Parse.Boolean( res["IsAdmin"] ) ) { Privilege |= Privilege.ADMIN; }
		//						if( Parse.Boolean( res["IsDeveloper"] ) ) { Privilege |= Privilege.DEV; }

		//						// Drop banned players. This should be caught by the access module, so this check is redundant.
		//						if( Parse.Boolean( res["IsBanned"] ) ) {
		//							TriggerEvent( "UI.Notification", "~r~YOU ARE BANNED~s~ You will now be disconnected." );
		//							Function.Call( Hash.DROP_PLAYER, NetID, "Banned" );
		//							return;
		//						}
		//					}
		//				}
		//			}

		//			// If the user isn't logged in at this point, it means the account doesn't exist
		//			if( !IsLoggedIn ) {
		//				using( var cmd = conn.GetCommand() ) {
		//					cmd.CommandText = "INSERT INTO `Users` SET `SteamID`=@SteamID";
		//					cmd.Parameters.AddWithValue( "@SteamID", SteamID.Hex64 );

		//					if( cmd.ExecuteNonQuery() > 0 ) {
		//						UserID = (int)cmd.LastInsertedId;
		//					}
		//				}
		//			}

		//			// Drop the connection if the user is not logged in at this point
		//			if( !IsLoggedIn ) {
		//				TriggerEvent( "UI.Notification", "~r~Authentication error~s~ You will now be disconnected. Please try again." );
		//				Function.Call( Hash.DROP_PLAYER, NetID, "Authentication error" );
		//				return;
		//			}

		//			// Inform the client is has authenticated with the server
		//			TriggerEvent( "Session.LoggedIn", UserID, (int)Privilege );
		//			ShowCharList();
		//		}
		//	}
		//	catch( Exception ex ) {
		//		Log.Error( ex, "Player authentication error" );
		//		Function.Call( Hash.DROP_PLAYER, NetID, "Unknown player authentication error" );
		//	}
		//}

		/// <summary>
		/// Called on client event: playerDropped
		/// </summary>
		/// <param name="reason"></param>
		public void Dropped( string reason ) {
			BaseScript.TriggerClientEvent( "Session.PlayerLeft", Convert.ToInt32( NetID ), reason );

			try {
				// Remove any references to this session from the tracking collections
				SessionManager.PublicCharacterList.Remove( Convert.ToInt32( NetID ) );
				SessionManager.PlayerList.Remove( Character.CharID );
				SessionManager.SessionList.Remove( NetID );
			}
			catch( Exception ex ) {
				Log.Warn( ex, string.Format( "Disconnecting player couldn't clean after itself (NetID: {0}, CharID: {1})", NetID, Character.CharID ) );
			}
		}

		/// <summary>
		/// Called on client event: Session.Deinit
		/// </summary>
		public void Deinit() {
			if( Character != null && SessionManager.PlayerList.ContainsKey( Character.CharID ) )
				SessionManager.PlayerList.Remove( Character.CharID );

			ShowCharList();
		}

		/// <summary>
		/// Fetches user's characters from the database to be selected from in the character select screen
		/// </summary>
		public void ShowCharList() {
			var stopwatch = new System.Diagnostics.Stopwatch();
			Log.Verbose( "Showing Character List" );
			if( !IsLoggedIn ) {
				TriggerEvent( "UI.Notification", "~r~Error~s~ Not logged in." );
				return;
			}

			try {
				NUIModels.PlayerCharacters characterList = new NUIModels.PlayerCharacters();
				stopwatch.Start();
				using( var conn = DBManager.GetConnection() ) {
					using( var cmd = conn.GetCommand() ) {
						// Fetch all characters belonging on the logged in user
						cmd.CommandText = "SELECT `CharID`, `FirstName`, `LastName`, DATE_FORMAT( `DOB`, '%m/%d/%Y' ) AS `DOB`, `Gender`, `IsPolice`, `IsEMS`, `IsFireman`, `IsFirstSpawn`, `CharacterCompensation` FROM `Characters` WHERE `UserID`=@UserID AND `IsActive`=1";
						cmd.Prepare();
						cmd.Parameters.AddWithValue( "@UserID", UserID );

						using( var res = cmd.ExecuteReader() ) {
							while( res.Read() ) {
								characterList.Characters.Add( new NUIModels.PlayerCharacter( res ) );
							}
						}
					}
				}
				stopwatch.Stop();
				Log.Verbose( $"Database fetch complete - Elapsed ms: {stopwatch.ElapsedMilliseconds}" );
				Log.Verbose( "Returning character list" );
				stopwatch.Restart();
				TriggerEvent( "Session.CharList", SerializeHelpers.SerializeObject( characterList ) );
				stopwatch.Stop();
				Log.Verbose( $"Triggered Char List, elapsed ms: {stopwatch.ElapsedMilliseconds}" );
			}
			catch( Exception ex ) {
				Log.Error( ex.Message );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="firstName"></param>
		/// <param name="lastName"></param>
		/// <param name="dob"></param>
		/// <param name="gender"></param>
		/// <returns></returns>
		public void CreateCharacter( string firstName, string lastName, string dob, int gender ) {
			Log.Verbose( "Creating Character" );
			// Simple redudant check to see if everything is still alright (checks already done client-side).
			if( string.IsNullOrWhiteSpace( firstName ) || string.IsNullOrWhiteSpace( lastName ) || string.IsNullOrWhiteSpace( dob ) || (gender < 1 || gender > 2) ) {
				TriggerEvent( "UI.Notification", "~r~Error~s~ Please fill in the form correctly." );
				return;
			}

			// Prevent slight unobvious variations of character names
			firstName.Trim();
			lastName.Trim();

			try {
				using( var conn = DBManager.GetConnection() ) {
					using( var cmd = conn.GetCommand() ) {
						// Find out if the character name is already in use
						cmd.CommandText = "SELECT COUNT(*) FROM `Characters` WHERE `FirstName`=@FirstName AND `LastName`=@LastName";
						cmd.Parameters.AddWithValue( "@FirstName", firstName );
						cmd.Parameters.AddWithValue( "@LastName", lastName );

						if( Convert.ToInt32( cmd.ExecuteScalar() ) > 0 ) {
							TriggerEvent( "UI.Notification", "~r~Error~s~ This name is already in use." );
							return;
						}
					}

					using( var cmd = conn.GetCommand() ) {
						// Create the character. FirstName+LastName needs to be unique, otherwise this query fails
						cmd.CommandText = "INSERT INTO `Characters` SET `UserID`=@UserID, `FirstName`=@FirstName, `LastName`=@LastName, `DOB`=@DOB, `Gender`=@Gender, `CreateDate`=@CreateDate;";
						cmd.Parameters.AddWithValue( "@UserID", UserID );
						cmd.Parameters.AddWithValue( "@FirstName", firstName );
						cmd.Parameters.AddWithValue( "@LastName", lastName );
						cmd.Parameters.AddWithValue( "@DOB", dob );
						cmd.Parameters.AddWithValue( "@Gender", gender );
                        cmd.Parameters.AddWithValue( "@CreateDate", DateTime.UtcNow );

						cmd.ExecuteNonQuery();
					}

					// Let user know character has been created and show the character lsit
					TriggerEvent( "UI.Notification", "~g~Success~s~ Your character has been created." );
					ShowCharList();

					return;
				}
			}
			catch( Exception ex ) {
				Log.Error( ex );
				TriggerEvent( "UI.Notification", "~r~Error~s~ Something went wrong. Please check your form and try again." );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="charID"></param>
		/// <returns></returns>
		public void SelectCharacter( int charID ) {
			Log.Info( $@"Selected Character, Source: {NetID} - SteamID: {SteamID} - CharID: {charID}" );

			// Require user to be logged in
			if( !IsLoggedIn ) {
				TriggerEvent( "UI.Notification", "~r~Error~s~ You are not logged in." );
				return;
			}

			try {
				using( var conn = DBManager.GetConnection() ) {
					using( var cmd = conn.GetCommand() ) {
						cmd.CommandText =
						@"
                            SELECT
	                            CharID,
                                UserID,
                                FirstName,
                                LastName,
                                DOB,
                                Gender,
								PedData,
                                Cash,
                                Debit,
                                DirtyMoneyTracked,
                                DirtyMoneyUnTracked,
                                StateDebt,
                                PrisonTime,
                                IsPolice,
                                IsEMS,
                                IsFireman,
                                IsActive,
                                IsFirstSpawn,
                                CharacterCompensation
                            FROM `Characters` 
                            WHERE CharID=@CharID
							LIMIT 1
                        ";
						cmd.Parameters.AddWithValue( "@CharID", charID );

						using( var res = cmd.ExecuteReader() ) {
							if( res.HasRows && res.Read() ) {
								Log.Verbose( "Before new CharacterData" );
								Character = new CharacterData( res );
								Log.Verbose( "After new CharacterData" );

								// Set this session as an active player in the session manager
								if( Activate() ) {
									Log.Verbose( "Character activated & going to Player.Init" );
									// Initialize the player by sending various initializing data
									TriggerEvent( "Player.Init", Helpers.MsgPack.Serialize( Character ), GenerateSpawnJson(), Items.SerializedItems );
									Log.Verbose( "After Player.Init & going to Session.PlayerJoined" );

									// Announce to everyone (including the player itself) a character has joined
									BaseScript.TriggerClientEvent( "Session.PlayerJoined", Convert.ToInt32( NetID ), Helpers.MsgPack.Serialize( (CharacterDataPublic)Character ) );
									Log.Verbose( "After Session.PlayerJoined" );

									// Update last login time for this character
									using( var updateTimestamp = conn.GetCommand() ) {
										updateTimestamp.CommandText = @"UPDATE `Characters` SET `LastLogin`=NOW() WHERE `CharID`=@CharID";
										updateTimestamp.Parameters.AddWithValue( "@CharID", Character.CharID );

										DBManager.BackgroundDbTask.TryAdd( updateTimestamp );
									}

                                    //Ammunation.LoadCharacterWeaponsHandle( Player, string.Empty);
                                    Vehicles.LoadCharacterVehicles( Player, string.Empty);
                                }
                                else {
									TriggerEvent( "UI.Notification", "~r~Error~s~ Character is already logged in." );
								}
							}
						}
					}
				}
            }
            catch ( Exception ex ) {
				Log.Error($"Session SelectCharacter Error: {ex.Message}");
				Deactivate();
			}
		}

		public void SavePedData( string pedData ) {
			try {
				using( var conn = DBManager.GetConnection() ) {
					using( var cmd = conn.GetCommand() ) {
						cmd.CommandText = "UPDATE `Characters` SET `PedData`=@PedData WHERE `CharID`=@CharID;";
						cmd.Parameters.AddWithValue( "@CharID", Character.CharID );
						cmd.Parameters.AddWithValue( "@PedData", StringExtensions.StringToBytes( pedData ) );

						cmd.ExecuteNonQuery();
					}

					return;
				}
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}
		#endregion
	}
}