//using CitizenFX.Core;
//using CitizenFX.Core.Native;
//using FamilyRP.Roleplay.Client.Helpers;
//using FamilyRP.Roleplay.Enums.Controls;
//using FamilyRP.Roleplay.Enums.Police;
//using FamilyRP.Roleplay.SharedClasses;
//using FamilyRP.Roleplay.SharedModels;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FamilyRP.Roleplay.Enums.Character;
//using static FamilyRP.Roleplay.Client.Helpers.WorldProbe;

//namespace FamilyRP.Roleplay.Client.Classes.Actions.Jobs.Police
//{
//	static class Arrest
//	{
//		static PlayerList playerList = new PlayerList();

//		static float cuffRaycastDistance = 20f;
//		static float cuffRange = 3f;
//		static int ragdollTripTime = 2000;

//		static CitizenFX.Core.Player targetPlayer;
//		static public CitizenFX.Core.Player attachedPlayer = null;
//		public static CuffState playerCuffState = CuffState.None;
//		static List<CuffState> cuffEnumValues = Enum.GetValues( typeof( CuffState ) ).OfType<CuffState>().ToList();
//		static Dictionary<CuffState, string> cuffStateStrings = new Dictionary<CuffState, string> {
//			[CuffState.None] = "unrestrained",
//			[CuffState.Cuffed] = "cuffed",
//			[CuffState.Hobbled] = "cuffed and hobbled"
//		};
//		static readonly List<VehicleSeat> ExtraSeats = new List<VehicleSeat>()
//		{
//			VehicleSeat.ExtraSeat1, VehicleSeat.ExtraSeat2, VehicleSeat.ExtraSeat3, VehicleSeat.ExtraSeat4,
//			VehicleSeat.ExtraSeat5, VehicleSeat.ExtraSeat6, VehicleSeat.ExtraSeat7, VehicleSeat.ExtraSeat8,
//			VehicleSeat.ExtraSeat9, VehicleSeat.ExtraSeat10, VehicleSeat.ExtraSeat11, VehicleSeat.ExtraSeat12
//		};
//		private static bool stayInVehicle = false;

//		public static void Init() {
//			// Looks like some of these things have to be done by the local player
//			// => Events
//			Client.GetInstance().RegisterEventHandler( "Arrest.ToggleCuffs", new Action<int>( async ( newCuffState ) => {
//				Game.PlayerPed.Task.ClearAllImmediately();
//				playerCuffState = (CuffState)newCuffState;
//				//EmotesManager.isPlayingEmote = false;
//				if( Game.PlayerPed.Weapons.Current != WeaponHash.Unarmed ) {
//					Function.Call( Hash.SET_CURRENT_PED_WEAPON, Game.PlayerPed.Handle, WeaponHash.Unarmed, true );
//					await BaseScript.Delay( 50 );
//				}


//				if( playerCuffState == CuffState.None ) {
//					Function.Call( Hash.SET_PED_CAN_SWITCH_WEAPON, Game.PlayerPed.Handle, true );
//					Function.Call( Hash.SET_ENABLE_HANDCUFFS, Game.PlayerPed.Handle, false );
//					Function.Call( Hash.SET_PED_PATH_CAN_USE_CLIMBOVERS, Game.PlayerPed.Handle, false );
//					Function.Call( Hash.SET_PED_PATH_CAN_USE_LADDERS, Game.PlayerPed.Handle, false );
//					Game.PlayerPed.Detach();
//				}
//				else if( playerCuffState == CuffState.Cuffed ) {
//					await Game.PlayerPed.Task.PlayAnimation( "mp_arresting", "idle", 8f, -8f, -1, (AnimationFlags)49, 0 );
//				}
//				Function.Call( Hash.DECOR_SET_INT, Game.PlayerPed.Handle, "Arrest.CuffState", newCuffState );
//			} ) );

//			Client.GetInstance().RegisterEventHandler( "Player.CarEvent", new Action<string>( HandleCarEvent ) );
//			Client.GetInstance().RegisterEventHandler( "Player.AttachmentEvent", new Action<string>( HandleAttachmentEvent ) );

//			Function.Call( Hash.DECOR_REGISTER, "Arrest.CuffState", 3 );
//			Function.Call( Hash.DECOR_REGISTER, "IsVehicleEnterTarget", 2 );

//			Client.GetInstance().RegisterTickHandler( OnTick );

//			Task.Factory.StartNew( async () => {
//				while( true ) {
//					if( Game.PlayerPed.IsRunning && playerCuffState == CuffState.Cuffed && new Random().Next( 1, 100 ) <= 25 ) {
//						// Trip
//						Function.Call( Hash.SET_PED_TO_RAGDOLL, CitizenFX.Core.Game.PlayerPed.Handle, ragdollTripTime, 2 * ragdollTripTime, 0, true, true, true );
//						await BaseScript.Delay( 2 * ragdollTripTime );
//						await Game.PlayerPed.Task.PlayAnimation( "mp_arresting", "idle", 10f, 10f, -1, (AnimationFlags)49, 0 );
//					}
//					await BaseScript.Delay( 100 );
//				}
//			} );
//		}

//		private static void HandleAttachmentEvent( string serializedEvent ) {
//			try {
//				TriggerEventForPlayersModel eventData = Helpers.MsgPack.Deserialize<TriggerEventForPlayersModel>( serializedEvent );
//				if( eventData.Payload == "ATTACH" ) {
//					if( !Game.PlayerPed.IsInVehicle() ) {
//						var sourcePlayer = playerList[eventData.sourceServerId];
//						if( !sourcePlayer.Character.IsInVehicle() && sourcePlayer.Character.Position.DistanceToSquared( Game.PlayerPed.Position ) < Math.Pow( cuffRange, 2 ) ) {
//							Function.Call( Hash.ATTACH_ENTITY_TO_ENTITY, Game.PlayerPed.Handle, sourcePlayer.Character.Handle, 1, 0f, 1f, 0f, 0f, -90f, 0f, false, false, false, true, 1, true );
//							BaseScript.TriggerServerEvent( "TriggerEventForPlayers", Helpers.MsgPack.Serialize( new TriggerEventForPlayersModel( eventData.sourceServerId, "Player.AttachmentEvent", "SUCCESSFUL_ATTACH", true ) ) );
//						}
//					}
//				}
//				else if( eventData.Payload == "DETACH" ) {
//					if( !Game.PlayerPed.IsInVehicle() ) {
//						Game.PlayerPed.Detach();
//					}
//					BaseScript.TriggerServerEvent( "TriggerEventForPlayers", Helpers.MsgPack.Serialize( new TriggerEventForPlayersModel( eventData.sourceServerId, "Player.AttachmentEvent", "SUCCESSFUL_DETACH", true ) ) );
//				}
//				else if( eventData.Payload == "SUCCESSFUL_DETACH" ) {
//					attachedPlayer = null;
//				}
//				else if( eventData.Payload == "SUCCESSFUL_ATTACH" ) {
//					attachedPlayer = playerList[eventData.sourceServerId];
//				}
//			}
//			catch( Exception ex ) {
//				Log.Error( $"HandleAttachmentEvent error: {ex.Message}" );
//			}
//		}

//		class CarEventModel
//		{
//			public string EventName = null;
//			public float[] vehiclePosition { get; set; } = null;
//			public VehicleSeat EnterSeat { get; set; } = VehicleSeat.Any;

//			public CarEventModel() { }
//		}

//		private static async void HandleCarEvent( string serializedEvent ) {
//			try {
//				TriggerEventForPlayersModel eventData = Helpers.MsgPack.Deserialize<TriggerEventForPlayersModel>( serializedEvent );
//				CarEventModel carEvent = Helpers.MsgPack.Deserialize<CarEventModel>( eventData.Payload );
//				Log.Info( "Received " + carEvent.EventName + " from #" + eventData.sourceServerId );
//				if( carEvent.EventName == "ENTER" ) {
//					var targetVehicles = new VehicleList().Select( v => (CitizenFX.Core.Vehicle)Entity.FromHandle( v ) ).Where( v => Function.Call<bool>( Hash.DECOR_EXIST_ON, v.Handle, "IsVehicleEnterTarget" ) && Function.Call<bool>( Hash.DECOR_GET_BOOL, v.Handle, "IsVehicleEnterTarget" ) && v.Position.DistanceToSquared( Game.PlayerPed.Position ) < Math.Pow( 10, 2 ) );
//					if( targetVehicles.Count() > 0 ) {
//						// Send success message
//						CitizenFX.Core.Vehicle targetVehicle = targetVehicles.First();
//						Game.PlayerPed.Detach();
//						if( playerCuffState == CuffState.Hobbled ) {
//							Game.PlayerPed.Task.WarpIntoVehicle( targetVehicle, carEvent.EnterSeat );
//						}
//						else {
//							Game.PlayerPed.Task.EnterVehicle( targetVehicle, carEvent.EnterSeat );
//						}
//						await BaseScript.Delay( 3500 );
//						if( !Game.PlayerPed.IsInVehicle() ) {
//							Game.PlayerPed.Task.ClearAllImmediately();
//							Game.PlayerPed.Task.WarpIntoVehicle( targetVehicle, carEvent.EnterSeat );
//						}
//						Function.Call( Hash.SET_PLAYER_CAN_DO_DRIVE_BY, Game.Player.Handle, false );
//						Function.Call( Hash.SET_ENABLE_HANDCUFFS, Game.PlayerPed.Handle, true );
//						Function.Call( Hash.SET_PED_CAN_SWITCH_WEAPON, Game.PlayerPed.Handle, false );
//						Function.Call( Hash.DECOR_SET_BOOL, targetVehicle.Handle, "IsVehicleEnterTarget", false );
//						var serial = Helpers.MsgPack.Serialize( new CarEventModel() { EventName = "SUCCESSFUL_ENTER" } );
//						BaseScript.TriggerServerEvent( "TriggerEventForPlayers", Helpers.MsgPack.Serialize( new TriggerEventForPlayersModel( eventData.sourceServerId, "Player.CarEvent", serial, true ) ) );
//						stayInVehicle = true;
//						while( stayInVehicle ) {
//							if( ControlHelper.IsControlJustPressed( Control.Enter, false, ControlModifier.Any ) || !(Game.PlayerPed.CurrentVehicle == targetVehicle) ) {
//								if( !targetVehicle.Exists() ) {
//									stayInVehicle = false;
//									ClearVehicleBind();
//									await BaseScript.Delay( 0 );
//									break;
//								}
//								Game.PlayerPed.Task.ClearAllImmediately();
//								Game.PlayerPed.Task.ClearAll();
//								Game.PlayerPed.Task.EnterVehicle( targetVehicle, carEvent.EnterSeat, 1, 16, 0 );
//								Game.PlayerPed.Task.WarpIntoVehicle( targetVehicle, carEvent.EnterSeat );
//							}
//							await BaseScript.Delay( 0 );
//						}
//					}
//				}
//				else if( carEvent.EventName == "EXIT" ) {
//					stayInVehicle = false;
//					if( Game.PlayerPed.IsInVehicle() ) {
//						Game.PlayerPed.Task.LeaveVehicle();
//						await BaseScript.Delay( 1500 );
//						ClearVehicleBind();
//						Function.Call( Hash.ATTACH_ENTITY_TO_ENTITY, Game.PlayerPed.Handle, new PlayerList()[eventData.sourceServerId].Character.Handle, 1, 0f, 1f, 0f, 0f, -90f, 0f, false, false, false, true, 1, true );
//						if( playerCuffState == CuffState.Cuffed ) // soft cuffs reapplied; hobble will be applied automatically
//						{
//							await Game.PlayerPed.Task.PlayAnimation( "mp_arresting", "idle", 8f, -8f, -1, (AnimationFlags)49, 0 );
//						}
//						var serial = Helpers.MsgPack.Serialize( new CarEventModel() { EventName = "SUCCESSFUL_EXIT" } );
//						BaseScript.TriggerServerEvent( "TriggerEventForPlayers", Helpers.MsgPack.Serialize( new TriggerEventForPlayersModel( eventData.sourceServerId, "Player.CarEvent", serial, true ) ) );
//					}
//				}
//				else if( carEvent.EventName == "SUCCESSFUL_ENTER" ) {
//					attachedPlayer = null;
//				}
//				else if( carEvent.EventName == "SUCCESSFUL_EXIT" ) {
//					attachedPlayer = new PlayerList()[eventData.sourceServerId];
//				}
//			}
//			catch( Exception ex ) {
//				Log.Error( $"HandleCarEvent error: {ex.Message}:\r\n{ex.StackTrace}" );
//			}
//		}

//		private static void ClearVehicleBind() {
//			Game.PlayerPed.Task.ClearAllImmediately();
//			if( Game.PlayerPed.IsInVehicle() ) {
//				Game.PlayerPed.Task.WarpOutOfVehicle( Game.PlayerPed.CurrentVehicle );
//			}
//			Function.Call( Hash.SET_PLAYER_CAN_DO_DRIVE_BY, Game.Player.Handle, true );
//			Function.Call( Hash.SET_ENABLE_HANDCUFFS, Game.PlayerPed.Handle, false );
//			Function.Call( Hash.SET_PED_CAN_SWITCH_WEAPON, Game.PlayerPed.Handle, true );
//		}

//		static public async Task OnTick() {
//			try {
//				if( playerCuffState != CuffState.None ) {
//					Game.DisableControlThisFrame( 0, Control.Attack );
//					Game.DisableControlThisFrame( 0, Control.MeleeAttackAlternate );
//					Game.DisableControlThisFrame( 0, Control.MeleeAttack1 );
//					Game.DisableControlThisFrame( 0, Control.MeleeAttack2 );
//					Game.DisableControlThisFrame( 0, Control.VehicleExit );
//					Function.Call( Hash.SET_PED_PATH_CAN_USE_CLIMBOVERS, Game.PlayerPed.Handle, false );
//					Function.Call( Hash.SET_PED_PATH_CAN_USE_LADDERS, Game.PlayerPed.Handle, false );
//					Function.Call( Hash.SET_ENABLE_HANDCUFFS, Game.PlayerPed.Handle, true );
//					bool isUnarmed = Game.PlayerPed.Weapons.Current.Hash != WeaponHash.Unarmed;
//					if( !isUnarmed ) {
//						Game.PlayerPed.Weapons.Select( WeaponHash.Unarmed, true );
//						Game.PlayerPed.CanSwitchWeapons = false;
//					}
//					if( playerCuffState == CuffState.Hobbled ) {
//						await Game.PlayerPed.Task.PlayAnimation( "mp_arresting", "idle", 8f, -8, -1, 0, 0f );
//					}
//				}

//				if( !Game.PlayerPed.IsInVehicle() ) {
//					if( ControlHelper.IsControlJustPressed( Control.Context ) ) {
//						OutgoingToggleCuffs();
//					}
//					else if( ControlHelper.IsControlJustPressed( Control.Context, true, ControlModifier.Alt ) ) {
//						OutgoingToggleAttachment();
//					}
//					else if( ControlHelper.IsControlJustPressed( Control.Context, true, ControlModifier.Shift ) ) {
//						OutgoingToggleVehicle();
//					}
//				}

//				//// Should fix world not loading around attached player
//				if( Function.Call<bool>( Hash.IS_ENTITY_ATTACHED_TO_ANY_OBJECT, Game.PlayerPed ) ) {
//					Function.Call( Hash.REQUEST_COLLISION_AT_COORD, Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z );
//				}
//			}
//			catch( Exception ex ) {
//				Log.Error( $"Arrest OnTick error: {ex.Message}:\r\n{ex.StackTrace}" );
//			}
//		}

//		private static void OutgoingToggleVehicle() {
//			if( CurrentPlayer.CharacterData.Duty != Duty.Police && CurrentPlayer.CharacterData.Duty != Duty.EMS ) {
//				return;
//			}
//			try {
//				var vehicle = GetVehicleInFront( 10f );
//				if( vehicle != null ) {
//					var extraSeat = GetNextAvailableExtraSeat( vehicle );
//					var bones = new Dictionary<EntityBone, VehicleSeat>() {
//						[vehicle.Bones["door_pside_r"]] = VehicleSeat.RightRear,
//						[vehicle.Bones["door_dside_r"]] = VehicleSeat.LeftRear,
//						[vehicle.Bones["door_hatch_l"]] = extraSeat,
//						[vehicle.Bones["door_hatch_r"]] = extraSeat,
//						[vehicle.Bones["gear_door_rl1"]] = extraSeat,
//						[vehicle.Bones["gear_door_rr1"]] = extraSeat,
//						[vehicle.Bones["gear_door_rl2"]] = extraSeat,
//						[vehicle.Bones["gear_door_rr2"]] = extraSeat,
//						[vehicle.Bones["gear_door_rml"]] = extraSeat,
//						[vehicle.Bones["gear_door_rmr"]] = extraSeat
//					};
//					var targetDoor = bones.Keys.OrderBy( b => b.Position
//							.DistanceToSquared( attachedPlayer == null ? Game.PlayerPed.Position : attachedPlayer.Character.Position ) ).FirstOrDefault();
//					var targetSeat = targetDoor == null ? VehicleSeat.None : bones[targetDoor];

//					if( attachedPlayer != null ) {
//						Log.Debug( "ENTER VEHICLE PROCEDURE" );
//						if( !vehicle.IsSeatFree( targetSeat ) ) {
//							BaseScript.TriggerEvent( "Chat.Message", "", "#55EEAA", "Someone else is occupying that seat already." );
//							return;
//						}
//						Function.Call( Hash.DECOR_SET_BOOL, vehicle.Handle, "IsVehicleEnterTarget", true );
//						// as NetIds are unreliable
//						var serializedCarEvent = Helpers.MsgPack.Serialize( new CarEventModel {
//							EventName = "ENTER",
//							EnterSeat = targetSeat,
//							vehiclePosition = vehicle.Position.ToArray()
//						} );
//						BaseScript.TriggerServerEvent( "TriggerEventForPlayers",
//							Helpers.MsgPack.Serialize( new TriggerEventForPlayersModel( attachedPlayer.ServerId, "Player.CarEvent",
//								serializedCarEvent, true ) ) );
//					}
//					else {
//						var playerInSeat = playerList.FirstOrDefault( p => p.Character == vehicle.GetPedOnSeat( targetSeat ) );
//						if( playerInSeat != null ) {
//							var serializedCarEvent = Helpers.MsgPack.Serialize( new CarEventModel { EventName = "EXIT" } );
//							BaseScript.TriggerServerEvent( "TriggerEventForPlayers",
//								Helpers.MsgPack.Serialize( new TriggerEventForPlayersModel( playerInSeat.ServerId, "Player.CarEvent",
//									serializedCarEvent, true ) ) );
//						}
//						else {
//							BaseScript.TriggerEvent( "Chat.Message", "", "#55EEAA", "There's nobody in the seat in front of you." );
//						}
//					}
//				}
//				else if( attachedPlayer != null ) {
//					Log.ToChat( "There is no car in front of you." );
//				}
//			}
//			catch( Exception ex ) {
//				Log.Error( $"OutgoingToggleVehicle error: {ex.Message}:\r\n{ex.StackTrace}" );
//			}
//		}

//		private static VehicleSeat GetNextAvailableExtraSeat( CitizenFX.Core.Vehicle vehicle ) {
//			foreach( var extra in ExtraSeats ) {
//				if( vehicle.IsSeatFree( extra ) )
//					return extra;
//			}
//			return VehicleSeat.None;
//		}

//		private static void OutgoingToggleAttachment() {
//			if( CurrentPlayer.CharacterData.Duty != Duty.Police && CurrentPlayer.CharacterData.Duty != Duty.EMS ) {
//				return;
//			}
//			try {
//				if( attachedPlayer != null ) {
//					BaseScript.TriggerServerEvent( "TriggerEventForPlayers", Helpers.MsgPack.Serialize( new TriggerEventForPlayersModel( attachedPlayer.ServerId, "Player.AttachmentEvent", "DETACH", true ) ) );
//				}
//				else {
//					var targetPlayer = FindValidArrestTarget();
//					BaseScript.TriggerServerEvent( "TriggerEventForPlayers", Helpers.MsgPack.Serialize( new TriggerEventForPlayersModel( targetPlayer.ServerId, "Player.AttachmentEvent", "ATTACH", true ) ) );
//				}
//			}
//			catch( Exception ex ) {
//				Log.Error( $"OutgoingToggleAttachment error: {ex.Message}:\r\n{ex.StackTrace}" );
//			}
//		}

//		private static void OutgoingToggleCuffs() {
//			if( CurrentPlayer.CharacterData.Duty != Duty.Police ) {
//				return;
//			}
//			try {
//				var target = FindValidArrestTarget();
//				if( target != null ) {
//					CuffState targetCuffState;
//					Log.Debug( Function.Call<bool>( Hash.DECOR_EXIST_ON, target.Character.Handle, "Arrest.CuffState" ).ToString() );
//					var doesExist = Function.Call<bool>( Hash.DECOR_EXIST_ON, target.Character.Handle, "Arrest.CuffState" );
//					if( !doesExist ) {
//						targetCuffState = Game.Player.IsAiming ? CuffState.Cuffed : CuffState.None;
//					}
//					else {
//						var cuffState = (CuffState)Function.Call<int>( Hash.DECOR_GET_INT, target.Character.Handle, "Arrest.CuffState" );
//						targetCuffState = cuffEnumValues[(cuffEnumValues.IndexOf( cuffState ) + 1) % cuffEnumValues.Count];
//					}
//					if( !Game.PlayerPed.IsAiming && !doesExist )
//						return;
//					BaseScript.TriggerEvent( "Chat.Message", "", "#55EEAA",
//						$"The target has been {cuffStateStrings[targetCuffState]}." );
//					BaseScript.TriggerServerEvent( "Arrest.ToggleCuffs", target.ServerId, (int)targetCuffState );
//				}
//				else {
//					var raycast = CrosshairRaycast();
//					if( raycast.DitHitEntity ) {
//						if( Function.Call<int>( Hash.GET_ENTITY_TYPE, raycast.HitEntity.Handle ) == 1 && !Function.Call<bool>( Hash.IS_ENTITY_PLAYING_ANIM, raycast.HitEntity.Handle, "mp_arresting", "idle", 3 ) ) {
//							Function.Call( Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, raycast.HitEntity.Handle, true );
//							((Ped)(raycast.HitEntity)).Task.PlayAnimation( "mp_arresting", "idle", 8f, 0, 180000, 0, 0f );
//							Function.Call( Hash.SET_PED_COMBAT_ATTRIBUTES, raycast.HitEntity.Handle, 46, true );
//						}
//						else {
//							((Ped)(raycast.HitEntity)).Task.ClearAll();
//						}
//					}
//					else
//						Log.Warn( "No valid arrest target" );
//				}
//			}
//			catch( Exception ex ) {
//				Log.Error( $"OutgoingToggleCuffs error: {ex.Message}:\r\n{ex.StackTrace}" );
//			}
//		}

//		static internal CitizenFX.Core.Player FindValidArrestTarget() {
//			if( Game.PlayerPed.IsAiming ) {
//				var raycast = CrosshairRaycast( cuffRaycastDistance );
//				if( raycast.DitHitEntity && !raycast.HitEntity.Model.IsVehicle ) {
//					IEnumerable<CitizenFX.Core.Player> targetEnumerable = playerList.Where( p => p.Character.Handle == raycast.HitEntity.Handle );
//					if( targetEnumerable.Any() && !targetEnumerable.First().Character.IsInVehicle() ) {
//						return targetEnumerable.FirstOrDefault();
//					}
//				}
//			}
//			return playerList.Where( p => p.Handle != Game.Player.Handle && ProduceDot( p.Character.Position, Game.PlayerPed.Position, Game.PlayerPed.ForwardVector ) > 0 &&
//					 p.Character.Position.DistanceToSquared( Game.PlayerPed.Position ) < Math.Pow( cuffRange, 2 ) )
//					.OrderBy( p => p.Character.Position.DistanceToSquared( Game.PlayerPed.Position ) ).FirstOrDefault();
//		}

//		private static CitizenFX.Core.Vehicle GetVehicleInFront( float meters ) {
//			return new VehicleList()
//				.Select( v => (CitizenFX.Core.Vehicle)Entity.FromHandle( v ) ).Where( v => v.Position.DistanceToSquared( Game.PlayerPed.Position ) < Math.Pow( meters, 2 ) &&
//						ProduceDot( v.Position, Game.PlayerPed.Position, Game.PlayerPed.ForwardVector ) > 0 ).OrderBy( v => v.Position.DistanceToSquared( Game.PlayerPed.Position ) )
//				.FirstOrDefault();
//		}

//		private static float ProduceDot( Vector3 target, Vector3 source, Vector3 forward ) {
//			var vec = (target - source);
//			vec.Normalize();
//			return Vector3.Dot( vec, forward );
//		}
//	}
//}
