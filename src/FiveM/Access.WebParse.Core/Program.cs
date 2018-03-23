using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using MySql.Data.MySqlClient;
using Steam.Models.SteamCommunity;
using SteamWebAPI2.Interfaces;
using TwitchLib;
using TwitchLib.Models.API.v5.Users;
using YamlDotNet.Serialization;

namespace Access.WebParse.Core
{
	internal static class Program
    {
	    private static ConfigModel _config;

	    private static List<string> _steamIds;
	    private static List<string> _discordIds;
	    private static List<string> _twitchIds;
	    private static List<string> _ipAddresses;

	    private static void Main()
        {
            DeserializerBuilder deserializerBuilder = new DeserializerBuilder();
            Deserializer deserializer = deserializerBuilder.Build();
            _config = deserializer.Deserialize<ConfigModel>(new StringReader(File.ReadAllText("config.yaml")));
            Console.WriteLine($"Web Server MySql Connection String: {_config.ConnString}");

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            _steamIds = FetchSteamIds();
            _discordIds = FetchDiscordIds();
            _twitchIds = FetchTwitchIds();
            _ipAddresses = FetchIpAddresses();

            Console.WriteLine($"SteamIds: {string.Join(", ", _steamIds)}");
            Console.WriteLine($"DiscordIds: {string.Join(", ", _discordIds)}");
            Console.WriteLine($"TwitchIds: {string.Join(", ", _twitchIds)}");
            Console.WriteLine($"IpAddresses: {string.Join(", ", _ipAddresses)}");

			RunEvery(PropagateNewUsers);
			RunEvery(PropagateChangedUsers);
			RunEvery(ParseNewSteamIds);
            RunEvery(ParseNewDiscordIds);
            RunEvery(ParseNewTwitchIds);
            RunEvery(ParseNewSteamIds);
            RunEvery(UpdateWhitelistTables);
			RunEvery(async () => {await Task.Run(() => GC.Collect());});

            bool quitFlag = false;
            while (!quitFlag)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                quitFlag = keyInfo.Key == ConsoleKey.C
                         && keyInfo.Modifiers == ConsoleModifiers.Control;
            }
        }

        private static async Task UpdateWhitelistTables()
        {
	        try
	        {
		        using (MySqlConnection conn = new MySqlConnection(_config.ConnString))
		        {
			        conn.Open();
			        MySqlCommand command = new MySqlCommand(@" CALL FamilyRpServerAccess.WhitelistProcedure;
                                                            CALL FamilyRpServerAccess.PublicProcedure;
                                                            CALL FamilyRpServerAccess.TesterProcedure;
                                                            CALL FamilyRpServerAccess.DeveloperProcedure;
															CALL FamilyRpServerAccess.ProcessPrivateSteamIds;", conn);
			        await command.ExecuteNonQueryAsync();
		        }
	        }
	        catch (Exception ex)
	        {
		        Console.WriteLine($"UpdateWhitelistTables: {ex}");
	        }

		}

        public static async void RunEvery(Func<Task> action, int periodicity = 60000)
        {
            await Task.Run(async () =>
            {
	            try
	            {
					Stopwatch stopwatch = Stopwatch.StartNew();
					bool isFirstRun = true;
		            while (true)
		            {
			            if (stopwatch.ElapsedMilliseconds > periodicity || isFirstRun)
			            {
				            isFirstRun = false;
				            await action.Invoke();
				            stopwatch = Stopwatch.StartNew();
			            }
			            else
			            {
				            await Task.Delay(1000);
			            }
		            }
	            }
				catch( Exception e ) {
		            Console.WriteLine( e );
	            }
				// ReSharper disable once FunctionNeverReturns
			} );
        }

        private static void AuditLog(string forumId, string accountType = "", string changeType = "", string oldValue = "", string newValue = "", BatchQueryBuilder batchQueryBuilder = null)
        {
	        batchQueryBuilder?.AppendSqlCommand(new MySqlCommand($@"INSERT INTO FamilyRpServerAccess.Log (Date, AccountType, ChangeType, OldValue, NewValue, ForumId) VALUES
                                (NOW(),
                                {(accountType == null ? "null" : $"'{MySqlHelper.EscapeString(accountType)}'")},
                                {(changeType == null ? "null" : $"'{MySqlHelper.EscapeString(changeType)}'")},
                                {(oldValue == null ? "null" : $"'{MySqlHelper.EscapeString(oldValue)}'")},
                                {(newValue == null ? "null" : $"'{MySqlHelper.EscapeString(newValue)}'")},
                                {forumId ?? "null"});"));
        }

        private static List<string> FetchTwitchIds(bool fetchNonParsedOnly = false)
        {
	        try
	        {
				using (MySqlConnection conn = new MySqlConnection(_config.ConnString))
				{
					conn.Open();
					MySqlCommand command = new MySqlCommand($"SELECT TwitchId FROM FamilyRpServerAccess.ParsedTwitchAccounts{(fetchNonParsedOnly ? " WHERE IsParsed = 0 AND ShouldParserIgnore <> 1" : "")};", conn);
					MySqlDataReader reader = command.ExecuteReader();
					List<string> result = new List<string>();
					while (reader.Read()) result.Add(Convert.ToString(reader["TwitchId"]));
					reader.Dispose();
					return result;
				}
			}
	        catch( Exception e ) {
		        Console.WriteLine( e );
			}
			return null;
		}

        private static void UpdateGroups(string forumId, List<int> groups, BatchQueryBuilder batchQueryBuilder)
        {
            batchQueryBuilder.AppendSqlCommand($"DELETE FROM FamilyRpServerAccess.GroupMembership WHERE ForumId = {forumId};");
            batchQueryBuilder.AppendSqlCommand("INSERT INTO FamilyRpServerAccess.GroupMembership (ForumId, GroupMembership.Group) " +
                                               $"VALUES {string.Join(", ", groups.Select(g => $"({forumId}, {g})"))};");
        }

        private static List<string> FetchDiscordIds(bool fetchNonParsedOnly = false)
        {
	        try
	        {
				using (MySqlConnection conn = new MySqlConnection(_config.ConnString))
				{
					conn.Open();
					MySqlCommand command = new MySqlCommand($"SELECT DiscordId FROM FamilyRpServerAccess.ParsedDiscordAccounts{(fetchNonParsedOnly ? " WHERE IsParsed = 0 AND ShouldParserIgnore <> 1" : "")};", conn);
					MySqlDataReader reader = command.ExecuteReader();
					List<string> result = new List<string>();
					while (reader.Read()) result.Add(Convert.ToString(reader["DiscordId"]));
					reader.Dispose();
					return result;
				}
			}
	        catch( Exception e ) {
		        Console.WriteLine( e );
			}
			return null;
		}

        private static List<string> FetchSteamIds(bool fetchNonParsedOnly = false)
        {
	        try
	        {
				using (MySqlConnection conn = new MySqlConnection(_config.ConnString))
				{
					conn.Open();
					MySqlCommand command = new MySqlCommand($"SELECT SteamId FROM FamilyRpServerAccess.ParsedSteamAccounts{(fetchNonParsedOnly ? " WHERE IsParsed = 0 AND ShouldParserIgnore <> 1" : "")};", conn);
					MySqlDataReader reader = command.ExecuteReader();
					List<string> result = new List<string>();
					while (reader.Read()) result.Add(Convert.ToString(reader["SteamId"]));
					reader.Dispose();
					return result;
				}
			}
	        catch( Exception e ) {
		        Console.WriteLine( e );
	        }
	        return null;
        }

        private static List<string> FetchIpAddresses(bool fetchIpv4Only = true)
        {
	        try
	        {
				using (MySqlConnection conn = new MySqlConnection(_config.ConnString))
				{
					conn.Open();
					MySqlCommand command = new MySqlCommand($"SELECT IP FROM FamilyRpServerAccess.IpAddresses{(fetchIpv4Only ? " WHERE CHAR_LENGTH(Ip) <= 16" : "")};", conn);
					MySqlDataReader reader = command.ExecuteReader();
					List<string> result = new List<string>();
					while (reader.Read()) result.Add(Convert.ToString(reader["IP"]));
					reader.Dispose();
					return result;
				}
			}
	        catch( Exception e ) {
		        Console.WriteLine( e );
	        }
	        return null;
        }

        private static async Task ParseNewTwitchIds()
        {
	        try
	        {
		        List<string> twitchIdsToParse = FetchTwitchIds(true);

		        TwitchAPI.Settings.ClientId = _config.TwitchApiClientId;
		        TwitchAPI.Settings.AccessToken = _config.TwitchApiAccessToken;

		        using (BatchQueryBuilder batchQuerybuilder = new BatchQueryBuilder(_config.ConnString))
		        {
			        foreach (string i in twitchIdsToParse)
			        {
				        if (i == "0") continue;
				        try
				        {
					        Console.WriteLine($"Parsing Twitch ID {i}");
					        User twitchUser = await TwitchAPI.Users.v5.GetUserByIDAsync(i);
					        DateTime twitchUserCreated = twitchUser.CreatedAt;
					        int twitchUserFollowerCount = (await TwitchAPI.Channels.v5.GetChannelFollowersAsync(twitchUser.Id)).Total;

					        batchQuerybuilder.AppendSqlCommand(
						        $"UPDATE FamilyRpServerAccess.ParsedTwitchAccounts SET TwitchCreated = '{twitchUserCreated:s}', " +
						        $"TwitchName = {(twitchUser.Name == null ? "null" : $"'{MySqlHelper.EscapeString(Utf8ToAscii(twitchUser.Name))}'")}, " +
						        $"TwitchFollowerCount = {twitchUserFollowerCount}, IsParsed = 1  WHERE TwitchId = {i};");
					        AuditLog(null, "Twitch", "Parsed", "", i, batchQuerybuilder);
				        }
				        catch (Exception ex)
				        {
					        if (!ex.ToString().Contains("TwitchLib.Exceptions.API.NotPartneredException"))
						        Console.WriteLine($"Failed to parse Twitch ID {i}; {ex}");
				        }
			        }
		        }
			}
	        catch (Exception e)
	        {
		        Console.WriteLine(e);
	        }
		}

        private static async Task ParseNewDiscordIds()
        {
			try {
				DiscordRestClient discordClient = new DiscordRestClient();
				//discordClient.Log += new Func<LogMessage, Task>(l => { Console.WriteLine(l); return Task.CompletedTask; });
				await discordClient.LoginAsync( TokenType.Bot, _config.DiscordApiToken );

				List<string> discordIdsToParse = FetchDiscordIds( true );

				using( BatchQueryBuilder batchQuerybuilder = new BatchQueryBuilder( _config.ConnString, 4000 ) ) {
					foreach( string i in discordIdsToParse ) {
						if( i == "0" ) continue;
						try {
							Console.WriteLine( $"Parsing Discord ID {i}" );
							RestUser discordUser = await discordClient.GetUserAsync( ulong.Parse( i ) );
							batchQuerybuilder.AppendSqlCommand( $"UPDATE FamilyRpServerAccess.ParsedDiscordAccounts SET DiscordCreated = '{discordUser.CreatedAt.UtcDateTime:s}', " +
															   $"DiscordUsername = '{MySqlHelper.EscapeString( Utf8ToAscii( discordUser.Username ) )}', " +
															   $"DiscordDiscriminator = {MySqlHelper.EscapeString( discordUser.Discriminator )}, IsParsed = 1 WHERE DiscordId = {i};" );
							AuditLog( null, "Discord", "Parsed", "", i, batchQuerybuilder );
						}
						catch( Exception ex ) {
							Console.WriteLine( $"Could not parse Discord ID {i}; {ex}" );
						}
					}
				}
			}
			catch( Exception e ) {
				Console.WriteLine($"ParseNewDiscordIds: {e}");
			}
        }

        private static async Task ParseNewSteamIds()
        {
			try {
				List<string> steamIdsToParse = FetchSteamIds( true );

				using( BatchQueryBuilder batchQuerybuilder = new BatchQueryBuilder( _config.ConnString ) ) {
					SteamUser steamInterface = new SteamUser( _config.SteamWebApiKey );

					foreach( string i in steamIdsToParse ) {
						if( i == "0" ) continue;
						try {
							Console.WriteLine( $"Parsing Steam ID {i}" );
							PlayerSummaryModel playerSummaryResponseData = (await steamInterface.GetPlayerSummaryAsync( ulong.Parse( i ) )).Data;
							IReadOnlyCollection<PlayerBansModel> playerBansResponseData = (await steamInterface.GetPlayerBansAsync( ulong.Parse( i ) )).Data;
							IReadOnlyCollection<FriendModel> playerFriendsResponseData = null;
							if( playerSummaryResponseData.ProfileVisibility == ProfileVisibility.Public )
								playerFriendsResponseData = (await steamInterface.GetFriendsListAsync( ulong.Parse( i ) )).Data;
							batchQuerybuilder.AppendSqlCommand( new MySqlCommand( $"UPDATE FamilyRpServerAccess.ParsedSteamAccounts SET SteamCreated = '{playerSummaryResponseData.AccountCreatedDate:s}', RowUpdated = NOW()," +
								$"SteamName = '{MySqlHelper.EscapeString( Utf8ToAscii( playerSummaryResponseData.Nickname ) )}'," +
								$"SteamVisibility = '{playerSummaryResponseData.ProfileVisibility}'," +
								$"NumSteamFriends = {playerFriendsResponseData?.Count ?? 0}," +
								$"SteamBans = {playerBansResponseData.First().NumberOfGameBans + (playerBansResponseData.First().CommunityBanned ? 1 : 0) + playerBansResponseData.First().NumberOfVACBans}," +
								"isParsed = 1 " +
								$"WHERE SteamId = {i};" ) );
							AuditLog( null, "Steam", "Parsed", "", i, batchQuerybuilder );
						}
						catch( Exception ex ) {
							if( !(ex.ToString().Contains( "System.Net.Http.HttpRequestException" ) && ex.ToString().Contains( "Unauthorized" )) )
								Console.WriteLine( $"Could not fetch Steam ID {i}: {ex}" );
						}
					}
				}
			}
			catch( Exception ex ) {
				Console.WriteLine($"{ex}");
			}
        }

        public static string Utf8ToAscii(string text)
        {
            Encoding utf8 = Encoding.UTF8;
            byte[] encodedBytes = utf8.GetBytes(text);
            byte[] convertedBytes =
                    Encoding.Convert(Encoding.UTF8, Encoding.ASCII, encodedBytes);
            Encoding ascii = Encoding.ASCII;

            return ascii.GetString(convertedBytes);
        }

        private static Task PropagateNewUsers()
        {
			try {
				using( MySqlConnection conn = new MySqlConnection( _config.ConnString ) ) {
					conn.Open();
					MySqlCommand command = new MySqlCommand {
						Connection = conn,
						CommandText = @"
                                    SELECT member_id, 
                                            name, 
                                            fm.email,
                                            COALESCE((SELECT ip_address FROM familyrp.frpcore_members_known_ip_addresses AS fmkip WHERE fmkip.member_id = fm.member_id AND CHAR_LENGTH(ip_address) <= 16 ORDER BY last_seen DESC LIMIT 1), ip_address) AS ip_address_fetched, 
                                            steamid, 
                                            From_unixtime(joined)        AS joined, 
                                            From_unixtime(last_visit)    AS last_visit, 
                                            From_unixtime(last_activity) AS last_activity, 
                                            discord_id, 
                                            discord_token, 
                                            tw_id, 
                                            tw_name, 
                                            tw_dname, 
                                            tw_token, 
                                            temp_ban, 
                                            member_posts, 
                                            msg_count_total, 
                                            member_group_id, 
                                            mgroup_others, 
                                            Md5(CONCAT(COALESCE(steamid, ''), COALESCE(last_visit, ''), 
                                                    COALESCE(last_activity, ''), COALESCE(discord_id, ''), 
                                                COALESCE(tw_id, ''), 
                                                    COALESCE(temp_ban, ''), COALESCE(member_posts, ''), 
                                                    COALESCE(msg_count_total, ''), COALESCE(member_group_id, ''), 
                                                COALESCE(mgroup_others, ''), COALESCE((SELECT ip_address FROM familyrp.frpcore_members_known_ip_addresses AS fmkip WHERE fmkip.member_id = fm.member_id AND CHAR_LENGTH(ip_address) <= 16 ORDER BY last_seen DESC LIMIT 1), fm.ip_address, ''))) AS checksum 
                                    FROM   familyrp.frpcore_members AS fm
                                    WHERE member_id IN (SELECT member_id FROM FamilyRpServerAccess.Users AS fsa
                                    RIGHT JOIN familyrp.frpcore_members AS fm ON fm.member_id = fsa.ForumId
                                    WHERE fsa.ForumId IS NULL
                                    ORDER BY fm.member_id DESC);"
                    };


					DbDataReader reader = command.ExecuteReader();
					int i = 1;

					using( BatchQueryBuilder bqBuilder = new BatchQueryBuilder( _config.ConnString ) ) {
						while( reader.Read() ) {
							UserDbModel user = UserDbModel.Create( reader );

							if( Convert.ToString( reader["steamid"] ) != "" )
								AuditLog( Convert.ToString( reader["member_id"] ), "Steam", "Added", null, Convert.ToString( reader["steamid"] ), bqBuilder );
							if( reader["steamid"] != DBNull.Value && !_steamIds.Contains( Convert.ToString( reader["steamid"] ) ) )
								CreateNonParsedSteamId( Convert.ToString( reader["steamid"] ), bqBuilder );

							if( Convert.ToString( reader["discord_id"] ) != "" )
								AuditLog( Convert.ToString( reader["member_id"] ), "Discord", "Added", null, Convert.ToString( reader["discord_id"] ), bqBuilder );
							if( reader["discord_id"] != DBNull.Value && Convert.ToString( reader["discord_id"] ) != null && !_discordIds.Contains( Convert.ToString( reader["discord_id"] ) ) )
								CreateNonParsedDiscordId( Convert.ToString( reader["discord_id"] ), bqBuilder );

							if( Convert.ToString( reader["tw_id"] ) != "" )
								AuditLog( Convert.ToString( reader["member_id"] ), "Twitch", "Added", null, Convert.ToString( reader["tw_id"] ), bqBuilder );
							if( reader["tw_id"] != DBNull.Value && Convert.ToString( reader["tw_id"] ) != null && !_twitchIds.Contains( Convert.ToString( reader["tw_id"] ) ) )
								CreateNonParsedTwitchId( Convert.ToString( reader["tw_id"] ), bqBuilder );

							if( Convert.ToString( reader["ip_address_fetched"] ) != "" )
								AuditLog( Convert.ToString( reader["member_id"] ), "IP", "Added", null, Convert.ToString( reader["ip_address_fetched"] ), bqBuilder );
						    if (reader["ip_address_fetched"] != DBNull.Value && Convert.ToString(reader["ip_address_fetched"]) != null)
						    {
						        if (!_ipAddresses.Contains(Convert.ToString(reader["ip_address_fetched"])))
						            CreateIpAddress(Convert.ToInt32(reader["member_id"]), Convert.ToString(reader["ip_address_fetched"]),
						                bqBuilder);
						        else
						            RefreshIpAddress(Convert.ToInt32(reader["member_id"]), Convert.ToString(reader["ip_address_fetched"]),
						                bqBuilder);
						    }

						    command = new MySqlCommand {
								CommandText =
									"INSERT INTO FamilyRpServerAccess.Users (ForumId, Email, SteamId, DiscordId, TwitchId, ForumBanned, ForumPostCount, " +
									"ServerBanned, ForumPmCount, ForumGroups, IsAdmin, IsDev, IsPolice, IsEMS, IsFireDept, LastLoggedInForum, ForumDbRowChecksum, CurrentIP) " +
									"VALUES (@ForumId, @Email, @SteamId, @DiscordId, @TwitchId, @ForumBanned, @ForumPostCount, " +
									"@ServerBanned, @ForumPmCount, @ForumGroups, @IsAdmin, @IsDev, @IsPolice, @IsEMS, @IsFireDept, @LastLoggedInForum, @ForumDbRowChecksum, @CurrentIP);"
							};


							command.Parameters.AddWithValue( "ForumId", user.Forum.Id );
							command.Parameters.AddWithValue( "SteamId", user.Steam.Id );
							command.Parameters.AddWithValue( "DiscordId", user.Discord.Id );
							command.Parameters.AddWithValue( "TwitchId", user.Twitch.Id );
							command.Parameters.AddWithValue( "ForumBanned", user.Forum.BanTime != 0 ? 1 : 0 );
							command.Parameters.AddWithValue( "Email", user.Forum.Email );
							command.Parameters.AddWithValue( "ForumPostCount", user.Forum.PostCount );
							command.Parameters.AddWithValue( "ServerBanned", 0 );
							command.Parameters.AddWithValue( "ForumPmCount", user.Forum.MessageCount );
							command.Parameters.AddWithValue( "ForumGroups", String.Join( ",", user.Forum.Groups ) );
							command.Parameters.AddWithValue( "IsAdmin", user.Forum.Groups.Contains( 10 ) ? 1 : 0 );
							command.Parameters.AddWithValue( "IsDev", user.Forum.Groups.Contains( 11 ) ? 1 : 0 );
							command.Parameters.AddWithValue( "IsPolice", user.Forum.Groups.Contains( 9 ) ? 1 : 0 );
							command.Parameters.AddWithValue( "IsEMS", user.Forum.Groups.Contains( 20 ) ? 1 : 0 );
							command.Parameters.AddWithValue( "IsFireDept", 0 );
							command.Parameters.AddWithValue( "LastLoggedInForum", user.Forum.LastVisit.ToString( "s" ) );
							command.Parameters.AddWithValue( "ForumDbRowChecksum", user.Forum.RowChecksum );
							command.Parameters.AddWithValue( "CurrentIP", user.Forum.CurrentIP );
							bqBuilder.AppendSqlCommand( command );
							Console.WriteLine( $"Populating row #{i} with forum id #{user.Forum.Id}" );
							AuditLog( user.Forum.Id.ToString(), "Forum", "Create", "", "", bqBuilder );
							UpdateGroups( user.Forum.Id.ToString(), user.Forum.Groups, bqBuilder );
							i++;
						}
					}
					reader.Dispose();
				}
			}
			catch( Exception ex ) {
				Console.WriteLine($"{ex}");
			}
            return Task.CompletedTask;
        }

        private static void CreateNonParsedTwitchId(string twitchId, BatchQueryBuilder batchQueryBuilder)
        {
            _twitchIds.Add(twitchId);
            batchQueryBuilder.AppendSqlCommand(new MySqlCommand("INSERT INTO FamilyRpServerAccess.ParsedTwitchAccounts (TwitchId, IsParsed, RowCreated, RowUpdated) " +
                                                                $"VALUES ({MySqlHelper.EscapeString(twitchId)}, 0, NOW(), NOW());"));
        }

        private static void CreateIpAddress(int forumId, string ip, BatchQueryBuilder batchQueryBuilder)
        {
            _ipAddresses.Add(ip);
            batchQueryBuilder.AppendSqlCommand(new MySqlCommand("INSERT INTO FamilyRpServerAccess.IpAddresses (ForumId, IP, RowCreated, RowUpdated) " +
                                                                $"VALUES ({forumId}, '{MySqlHelper.EscapeString(ip)}', NOW(), NOW());"));
        }
        private static void RefreshIpAddress(int forumId, string ip, BatchQueryBuilder batchQueryBuilder)
        {
            batchQueryBuilder.AppendSqlCommand(new MySqlCommand($"UPDATE FamilyRpServerAccess.IpAddresses SET ForumId = {forumId}, RowUpdated = NOW() WHERE IP = '{MySqlHelper.EscapeString(ip)}';"));
        }

        // ReSharper disable once UnusedMember.Local
        private static void CreateGtaId(string gtaId, BatchQueryBuilder batchQueryBuilder)
        {
            batchQueryBuilder.AppendSqlCommand(new MySqlCommand("INSERT INTO FamilyRpServerAccess.GtaAccounts (GtaLicense, RowCreated, RowUpdated) " +
                                                                $"VALUES ('{MySqlHelper.EscapeString(gtaId)}', NOW(), NOW());"));
        }

        private static void CreateNonParsedDiscordId(string discordId, BatchQueryBuilder batchQueryBuilder)
        {
            _discordIds.Add(discordId);
            batchQueryBuilder.AppendSqlCommand(new MySqlCommand("INSERT INTO FamilyRpServerAccess.ParsedDiscordAccounts " +
                                                                $"(DiscordId, IsParsed, RowCreated, RowUpdated) VALUES ({Convert.ToUInt64(discordId)}, 0, NOW(), NOW());"));
        }

        private static void CreateNonParsedSteamId(string steamId, BatchQueryBuilder batchQueryBuilder)
        {
            _steamIds.Add(steamId);
            batchQueryBuilder.AppendSqlCommand(new MySqlCommand("INSERT INTO FamilyRpServerAccess.ParsedSteamAccounts (SteamId, IsParsed, RowCreated, RowUpdated) " +
                                                                $"VALUES ({MySqlHelper.EscapeString(steamId)}, 0, NOW(), NOW());"));
        }

        private static Task PropagateChangedUsers()
        {
			try {
				using( MySqlConnection conn = new MySqlConnection( _config.ConnString ) ) {
					conn.Open();
					MySqlCommand command = new MySqlCommand {
						Connection = conn,
						CommandText = @"
                                    SELECT member_id, 
                                            name, 
                                            fm.email,
                                            COALESCE((SELECT ip_address FROM familyrp.frpcore_members_known_ip_addresses AS fmkip WHERE fmkip.member_id = fm.member_id AND CHAR_LENGTH(ip_address) <= 16 ORDER BY last_seen DESC LIMIT 1), ip_address) AS ip_address_fetched, 
                                            fm.steamid, 
                                            From_unixtime(joined)        AS joined, 
                                            From_unixtime(last_visit)    AS last_visit, 
                                            From_unixtime(last_activity) AS last_activity, 
                                            discord_id, 
                                            discord_token, 
                                            tw_id, 
                                            tw_name, 
                                            tw_dname, 
                                            tw_token, 
                                            fsa.DiscordId, 
                                            fsa.TwitchId, 
                                            fsa.SteamId AS OldSteamId, 
                                            fsa.CurrentIp AS OldIp,
                                            temp_ban, 
                                            member_posts, 
                                            msg_count_total, 
                                            member_group_id, 
                                            mgroup_others, 
                                            Md5(CONCAT(COALESCE(fm.steamid, ''), COALESCE(last_visit, ''), 
                                                    COALESCE(last_activity, ''), COALESCE(discord_id, ''), 
                                                COALESCE(tw_id, ''), 
                                                    COALESCE(temp_ban, ''), COALESCE(member_posts, ''), 
                                                    COALESCE(msg_count_total, ''), COALESCE(member_group_id, ''), 
                                                COALESCE(mgroup_others, ''), 
                                                COALESCE((SELECT ip_address FROM familyrp.frpcore_members_known_ip_addresses AS fmkip WHERE fmkip.member_id = fm.member_id AND CHAR_LENGTH(ip_address) <= 16 ORDER BY last_seen DESC LIMIT 1), fm.ip_address, ''))) AS checksum 
                                    FROM   familyrp.frpcore_members AS fm
                                    LEFT JOIN FamilyRpServerAccess.Users AS fsa ON fm.member_id = fsa.ForumId
                                    WHERE member_id IN (SELECT member_id FROM FamilyRpServerAccess.Users AS fsa
                                    INNER JOIN familyrp.frpcore_members AS fm ON fm.member_id = fsa.ForumId
                                    AND fsa.ForumDbRowChecksum <> MD5(CONCAT(COALESCE(fm.steamid, ''), COALESCE(fm.last_visit, ''), 
                                                    COALESCE(fm.last_activity, ''), COALESCE(fm.discord_id, ''), 
                                                COALESCE(fm.tw_id, ''), 
                                                    COALESCE(fm.temp_ban, ''), COALESCE(fm.member_posts, ''), 
                                                    COALESCE(fm.msg_count_total, ''), COALESCE(fm.member_group_id, ''), 
                                                COALESCE(fm.mgroup_others, ''), 
                                                COALESCE((SELECT ip_address FROM familyrp.frpcore_members_known_ip_addresses AS fmkip WHERE fmkip.member_id = fm.member_id AND CHAR_LENGTH(ip_address) <= 16 ORDER BY last_seen DESC LIMIT 1), fm.ip_address, ''))) 
                                    ORDER BY member_id DESC);"
                    };


					DbDataReader reader = command.ExecuteReader();
					int i = 0;

					using( BatchQueryBuilder bqBuilder = new BatchQueryBuilder( _config.ConnString ) ) {
						while( reader.Read() ) {
							UserDbModel user = UserDbModel.Create( reader );

							if( Convert.ToString( reader["steamid"] ) != "" && Convert.ToString( reader["steamid"] ) != Convert.ToString( reader["OldSteamId"] ) )
								AuditLog( Convert.ToString( reader["member_id"] ), "Steam", "Changed", Convert.ToString( reader["OldSteamId"] ), Convert.ToString( reader["steamid"] ), bqBuilder );
							if( reader["steamid"] != DBNull.Value && !_steamIds.Contains( Convert.ToString( reader["steamid"] ) ) )
								CreateNonParsedSteamId( Convert.ToString( reader["steamid"] ), bqBuilder );

							if( Convert.ToString( reader["discord_id"] ) != "" && Convert.ToString( reader["discord_id"] ) != Convert.ToString( reader["DiscordId"] ) )
								AuditLog( Convert.ToString( reader["member_id"] ), "Discord", "Changed", Convert.ToString( reader["DiscordId"] ), Convert.ToString( reader["discord_id"] ), bqBuilder );
							if( reader["discord_id"] != DBNull.Value && Convert.ToString( reader["discord_id"] ) != null && !_discordIds.Contains( Convert.ToString( reader["discord_id"] ) ) )
								CreateNonParsedDiscordId( Convert.ToString( reader["discord_id"] ), bqBuilder );

							if( Convert.ToString( reader["tw_id"] ) != "" && Convert.ToString( reader["tw_id"] ) != Convert.ToString( reader["TwitchId"] ) )
								AuditLog( Convert.ToString( reader["member_id"] ), "Twitch", "Changed", Convert.ToString( reader["TwitchId"] ), Convert.ToString( reader["tw_id"] ), bqBuilder );
							if( reader["tw_id"] != DBNull.Value && Convert.ToString( reader["tw_id"] ) != null && !_twitchIds.Contains( Convert.ToString( reader["tw_id"] ) ) )
								CreateNonParsedTwitchId( Convert.ToString( reader["tw_id"] ), bqBuilder );

							if( Convert.ToString( reader["ip_address_fetched"] ) != "" && Convert.ToString( reader["ip_address_fetched"] ) != Convert.ToString( reader["OldIp"] ) )
								AuditLog( Convert.ToString( reader["member_id"] ), "IP", "Changed", Convert.ToString( reader["OldIp"] ), Convert.ToString( reader["ip_address_fetched"] ), bqBuilder );
						    if (reader["ip_address_fetched"] != DBNull.Value && Convert.ToString(reader["ip_address_fetched"]) != null)
						    {
                                if(!_ipAddresses.Contains(Convert.ToString(reader["ip_address_fetched"])))
						            CreateIpAddress(Convert.ToInt32(reader["member_id"]), Convert.ToString(reader["ip_address_fetched"]), bqBuilder);
                                else
                                    RefreshIpAddress(Convert.ToInt32(reader["member_id"]), Convert.ToString(reader["ip_address_fetched"]), bqBuilder);
                            }

							command = new MySqlCommand {
								CommandText =
									"UPDATE FamilyRpServerAccess.Users SET SteamId = @SteamId, DiscordId = @DiscordId, TwitchId = @TwitchId, GtaLicense = @GtaLicense, " +
									"ForumBanned = @ForumBanned, ForumPostCount = @ForumPostCount, ForumPmCount = @ForumPmCount, ForumGroups = @ForumGroups, IsAdmin = @IsAdmin, " +
									"IsDev = @IsDev, IsPolice = @IsPolice, IsEMS = @IsEMS, IsFireDept = @IsFireDept, LastLoggedInForum = @LastLoggedInForum, " +
									"ForumDbRowChecksum = @ForumDbRowChecksum, CurrentIP = @CurrentIP, RowUpdated = NOW() WHERE ForumId = @ForumId;"
							};


							command.Parameters.AddWithValue( "ForumId", user.Forum.Id );
							command.Parameters.AddWithValue( "SteamId", user.Steam.Id );
							command.Parameters.AddWithValue( "DiscordId", user.Discord.Id );
							command.Parameters.AddWithValue( "TwitchId", user.Twitch.Id );
							command.Parameters.AddWithValue( "GtaLicense", DBNull.Value );
							command.Parameters.AddWithValue( "ForumBanned", user.Forum.BanTime != 0 ? 1 : 0 );
							command.Parameters.AddWithValue( "ForumPostCount", user.Forum.PostCount );
							command.Parameters.AddWithValue( "ForumPmCount", user.Forum.MessageCount );
							command.Parameters.AddWithValue( "ForumGroups", String.Join( ",", user.Forum.Groups ) );
							command.Parameters.AddWithValue( "IsAdmin", user.Forum.Groups.Contains( 10 ) ? 1 : 0 );
							command.Parameters.AddWithValue( "IsDev", user.Forum.Groups.Contains( 11 ) ? 1 : 0 );
							command.Parameters.AddWithValue( "IsPolice", user.Forum.Groups.Contains( 9 ) ? 1 : 0 );
							command.Parameters.AddWithValue( "IsEMS", user.Forum.Groups.Contains( 20 ) ? 1 : 0 );
							command.Parameters.AddWithValue( "IsFireDept", 0 );
							command.Parameters.AddWithValue( "LastLoggedInForum", user.Forum.LastVisit.ToString( "s" ) );
							command.Parameters.AddWithValue( "LastLoggedInGame", new DateTime( 1900, 1, 1 ).ToString( "s" ) );
							command.Parameters.AddWithValue( "ForumDbRowChecksum", user.Forum.RowChecksum );
							command.Parameters.AddWithValue( "CurrentIP", user.Forum.CurrentIP );

							bqBuilder.AppendSqlCommand( command );
							Console.WriteLine( $"Updating row #{i} with forum id #{user.Forum.Id}" );
							UpdateGroups( user.Forum.Id.ToString(), user.Forum.Groups, bqBuilder );
							i++;
						}
					}
					reader.Dispose();
				}
			}
			catch( Exception ex ) {
				Console.WriteLine($"{ex}");
			}
			return Task.CompletedTask;
        }
    }

    internal class ConfigModel
    {
        public string ConnString { get; set; }
        public string SteamWebApiKey { get; set; }
        public string TwitchApiClientId { get; set; }
        public string TwitchApiAccessToken { get; set; }
        public string DiscordApiToken { get; set; }
    }
}