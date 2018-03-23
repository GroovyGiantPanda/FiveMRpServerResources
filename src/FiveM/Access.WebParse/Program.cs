using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using SteamWebAPI2;
using SteamWebAPI2.Interfaces;
using System.Globalization;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;
using Discord.WebSocket;
using Discord;
using Discord.Rest;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Steam.Models.SteamCommunity;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Diagnostics;

namespace Access.WebParse
{
    class BatchQueryBuilder : IDisposable
    {
        private bool debugPrintEnabled = true;

        public StringBuilder sbBatchSql { get; set; }
        public string ConnString { get; set; }
        public string Query { get; set; }
        public int CharacterThreshold { get; set; }

        public BatchQueryBuilder(string connString, int characterThreshold = 64000)
        {
            sbBatchSql = new StringBuilder();
            this.ConnString = connString;
            this.CharacterThreshold = characterThreshold;
        }

        public void AppendSqlCommand(MySqlCommand sqlCommand)
        {
            string query = sqlCommand.CommandText;

            foreach (MySqlParameter sqlParameter in sqlCommand.Parameters)
            {
                query = query.Replace($"@{sqlParameter.ParameterName}", ParameterValueForSql(sqlParameter));
            }

            if (debugPrintEnabled) Console.WriteLine($"Adding query: {query}");

            sbBatchSql.Append(query);
            if (sbBatchSql.Length > CharacterThreshold)
                this.Flush();
        }

        public void AppendSqlCommand(string sqlCommand)
        {
            AppendSqlCommand(new MySqlCommand(sqlCommand));
        }

        internal static string ParameterValueForSql(MySqlParameter sqlParameter)
        {
            string s;

            if (sqlParameter.Value == null) return "null";

            switch (sqlParameter.DbType)
            {
                default:
                    s = sqlParameter.Value.ToString().Replace("'", "''");
                    break;
            }

            return $"'{s}'";
        }

        public void Flush(bool isDisposal = false)
        {
            if (sbBatchSql.Length == 0) return;
            using (var mysqlConnection = new MySqlConnection(ConnString))
            {
                mysqlConnection.Open();
                MySqlCommand mySqlCommand = new MySqlCommand(sbBatchSql.ToString(), mysqlConnection);
                MySqlTransaction mySqlTransaction = mysqlConnection.BeginTransaction();
                mySqlCommand.Transaction = mySqlTransaction;
                try
                {
                    if (debugPrintEnabled) Console.WriteLine($"Executing batch query (Length: {sbBatchSql.Length})");
                    mySqlCommand.ExecuteNonQuery();
                    mySqlTransaction.Commit();
                    if (debugPrintEnabled) Console.WriteLine($"Query committed");
                }
                catch(Exception ex)
                {
                    mySqlTransaction.Rollback();
                    if (debugPrintEnabled) Console.WriteLine($"Query rolled back; error {ex}");
                }
            }

            sbBatchSql.Clear();
        }

        public void Dispose()
        {
            this.Flush(true);
        }
    }

    static class Program
    {
        static ConfigModel config;

        static List<string> SteamIds;
        static List<string> DiscordIds;
        static List<string> TwitchIds;
        static List<string> IpAddresses;

        static void Main(string[] args)
        {
            DeserializerBuilder deserializerBuilder = new DeserializerBuilder();
            Deserializer deserializer = deserializerBuilder.Build();
            config = deserializer.Deserialize<ConfigModel>(new StringReader(File.ReadAllText("config.yaml")));
            Console.WriteLine($"Web Server MySql Connection String: {config.ConnString}");

            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            SteamIds = FetchSteamIds();
            DiscordIds = FetchDiscordIds();
            TwitchIds = FetchTwitchIds();
            IpAddresses = FetchIpAddresses();

            Console.WriteLine($"SteamIds: {String.Join(", ", SteamIds)}");
            Console.WriteLine($"DiscordIds: {String.Join(", ", DiscordIds)}");
            Console.WriteLine($"TwitchIds: {String.Join(", ", TwitchIds)}");
            Console.WriteLine($"IpAddresses: {String.Join(", ", IpAddresses)}");

            RunEvery(PropagateNewUsers);
            RunEvery(ParseNewSteamIds);
            RunEvery(ParseNewDiscordIds);
            RunEvery(ParseNewTwitchIds);
            RunEvery(ParseNewSteamIds);

            bool _quitFlag = false;
            while (!_quitFlag)
            {
                var keyInfo = Console.ReadKey();
                _quitFlag = keyInfo.Key == ConsoleKey.C
                         && keyInfo.Modifiers == ConsoleModifiers.Control;
            }
        }

        public static void RunEvery(Func<Task> action, int periodicity = 60000)
        {
            Task.Run(async () =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                bool isFirstRun = true;
                while (true)
                {
                    if(stopwatch.ElapsedMilliseconds > periodicity || isFirstRun)
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
            });
        }

        private static void AuditLog(string ForumId, string AccountType = "", string ChangeType = "", string OldValue = "", string NewValue = "", BatchQueryBuilder batchQueryBuilder = null)
        {
            if(batchQueryBuilder != null)
                batchQueryBuilder.AppendSqlCommand(new MySqlCommand($@"INSERT INTO FamilyRpServerAccess.Log (Date, AccountType, ChangeType, OldValue, NewValue, ForumId) VALUES
                                (NOW(),
                                {(AccountType == null ? "null" : $"'{MySqlHelper.EscapeString(AccountType)}'")},
                                {(ChangeType == null ? "null" : $"'{MySqlHelper.EscapeString(ChangeType)}'")},
                                {(OldValue == null ? "null" : $"'{MySqlHelper.EscapeString(OldValue)}'")},
                                {(NewValue == null ? "null" : $"'{MySqlHelper.EscapeString(NewValue)}'")},
                                {(ForumId == null ? "null" : ForumId)});"));
        }

        private static List<string> FetchTwitchIds(bool FetchNonParsedOnly = false)
        {
            using (var conn = new MySqlConnection(config.ConnString))
            {
                conn.Open();
                MySqlCommand command = new MySqlCommand($"SELECT TwitchId FROM FamilyRpServerAccess.ParsedTwitchAccounts{(FetchNonParsedOnly ? " WHERE IsParsed = 0" : "")};", conn);
                var reader = command.ExecuteReader();
                List<string> result = new List<string>();
                while (reader.Read()) result.Add(Convert.ToString(reader["TwitchId"]));
                reader.Dispose();
                return result;
            }
        }

        private static void UpdateGroups(string forumId, List<int> groups, BatchQueryBuilder batchQueryBuilder)
        {
            batchQueryBuilder.AppendSqlCommand($"DELETE FROM FamilyRpServerAccess.GroupMembership WHERE ForumId = {forumId};");
            batchQueryBuilder.AppendSqlCommand($"INSERT INTO FamilyRpServerAccess.GroupMembership (ForumId, GroupMembership.Group) VALUES {String.Join(", ", groups.Select(g => $"({forumId}, {g})"))};");
        }

        private static List<string> FetchDiscordIds(bool FetchNonParsedOnly = false)
        {
            using (var conn = new MySqlConnection(config.ConnString))
            {
                conn.Open();
                MySqlCommand command = new MySqlCommand($"SELECT DiscordId FROM FamilyRpServerAccess.ParsedDiscordAccounts{(FetchNonParsedOnly ? " WHERE IsParsed = 0" : "")};", conn);
                var reader = command.ExecuteReader();
                List<string> result = new List<string>();
                while (reader.Read()) result.Add(Convert.ToString(reader["DiscordId"]));
                reader.Dispose();
                return result;
            }
        }

        private static List<string> FetchSteamIds(bool FetchNonParsedOnly = false)
        {
            using (var conn = new MySqlConnection(config.ConnString))
            {
                conn.Open();
                MySqlCommand command = new MySqlCommand($"SELECT SteamId FROM FamilyRpServerAccess.ParsedSteamAccounts{(FetchNonParsedOnly ? " WHERE IsParsed = 0" : "")};", conn);
                var reader = command.ExecuteReader();
                List<string> result = new List<string>();
                while (reader.Read()) result.Add(Convert.ToString(reader["SteamId"]));
                reader.Dispose();
                return result;
            }
        }

        private static List<string> FetchIpAddresses(bool FetchIpv4Only = true)
        {
            using (var conn = new MySqlConnection(config.ConnString))
            {
                conn.Open();
                MySqlCommand command = new MySqlCommand($"SELECT IP FROM FamilyRpServerAccess.IpAddresses{(FetchIpv4Only ? " WHERE CHAR_LENGTH(Ip) <= 16" : "")};", conn);
                var reader = command.ExecuteReader();
                List<string> result = new List<string>();
                while (reader.Read()) result.Add(Convert.ToString(reader["IP"]));
                reader.Dispose();
                return result;
            }
        }

        private static async Task ParseNewTwitchIds()
        {
            List<string> TwitchIdsToParse = FetchTwitchIds(true);

            TwitchAPI.Settings.ClientId = config.TwitchApiClientId;
            TwitchAPI.Settings.AccessToken = config.TwitchApiAccessToken;

            using (BatchQueryBuilder batchQuerybuilder = new BatchQueryBuilder(config.ConnString))
            {
                foreach (string i in TwitchIdsToParse)
                {
                    if (i == "0") continue;
                    try
                    {
                        Console.WriteLine($"Parsing Twitch ID {i}");
                        var twitchUser = await TwitchAPI.Users.v5.GetUserByIDAsync(i);
                        var twitchUserCreated = twitchUser.CreatedAt;
                        var twitchUserFollowerCount = (await TwitchAPI.Channels.v5.GetChannelFollowersAsync(twitchUser.Id)).Total;

                        batchQuerybuilder.AppendSqlCommand($"UPDATE FamilyRpServerAccess.ParsedTwitchAccounts SET TwitchCreated = '{twitchUserCreated}', TwitchName = {(twitchUser.Name == null ? "null" : $"'{MySqlHelper.EscapeString(UTF8toASCII(twitchUser.Name))}'")}, TwitchFollowerCount = {twitchUserFollowerCount}, IsParsed = 1  WHERE TwitchId = {i};");
                        AuditLog(null, "Twitch", "Parsed", "", i, batchQuerybuilder);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Failed to parse Twitch ID {i}; {ex}");
                    }
                }
            }
        }

        private static async Task ParseNewDiscordIds()
        {
            DiscordRestClient discordClient = new DiscordRestClient();
            discordClient.Log += new Func<LogMessage, Task>(l => { Console.WriteLine(l); return Task.CompletedTask; });
            await discordClient.LoginAsync(TokenType.Bot, config.DiscordApiToken);

            List<string> DiscordIdsToParse = FetchDiscordIds(true);

            using (BatchQueryBuilder batchQuerybuilder = new BatchQueryBuilder(config.ConnString, 4000))
            {
                foreach (string i in DiscordIdsToParse)
                {
                    if (i == "0") continue;
                    try
                    {
                        Console.WriteLine($"Parsing Discord ID {i}");
                        var discordUser = await discordClient.GetUserAsync(UInt64.Parse(i));
                        batchQuerybuilder.AppendSqlCommand($"UPDATE FamilyRpServerAccess.ParsedDiscordAccounts SET DiscordCreated = '{discordUser.CreatedAt.UtcDateTime}', DiscordUsername = '{MySqlHelper.EscapeString(UTF8toASCII(discordUser.Username))}', DiscordDiscriminator = {MySqlHelper.EscapeString(discordUser.Discriminator)}, IsParsed = 1 WHERE DiscordId = {i};");
                        AuditLog(null, "Discord", "Parsed", "", i, batchQuerybuilder);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Could not parse Discord ID {i}; {ex}");
                    }
                }
            }
        }

        private static async Task ParseNewSteamIds()
        {
            List<string> SteamIdsToParse = FetchSteamIds(true);

            using (BatchQueryBuilder batchQuerybuilder = new BatchQueryBuilder(config.ConnString))
            {
                var steamInterface = new SteamUser(config.SteamWebApiKey, null);

                foreach (string i in SteamIdsToParse)
                {
                    if (i == "0") continue;
                    try
                    {
                        Console.WriteLine($"Parsing Steam ID {i}");
                        var playerSummaryResponseData = (await steamInterface.GetPlayerSummaryAsync(UInt64.Parse(i))).Data;
                        var playerBansResponseData = (await steamInterface.GetPlayerBansAsync(UInt64.Parse(i))).Data;
                        IReadOnlyCollection<FriendModel> playerFriendsResponseData = null;
                        if (playerSummaryResponseData.ProfileVisibility == Steam.Models.SteamCommunity.ProfileVisibility.Public)
                            playerFriendsResponseData = (await steamInterface.GetFriendsListAsync(UInt64.Parse(i))).Data;
                        batchQuerybuilder.AppendSqlCommand(new MySqlCommand($"UPDATE FamilyRpServerAccess.ParsedSteamAccounts SET SteamCreated = '{playerSummaryResponseData.AccountCreatedDate}',RowUpdated = NOW()," +
                            $"SteamName = '{MySqlHelper.EscapeString(UTF8toASCII(playerSummaryResponseData.Nickname))}'," +
                            $"SteamVisibility = '{playerSummaryResponseData.ProfileVisibility}'," +
                            $"NumSteamFriends = {(playerFriendsResponseData != null ? playerFriendsResponseData.Count : 0)}," +
                            $"SteamBans = {playerBansResponseData.First().NumberOfGameBans + (playerBansResponseData.First().CommunityBanned ? 1 : 0) + playerBansResponseData.First().NumberOfVACBans}," +
                            $"isParsed = 1 " +
                            $"WHERE SteamId = {i};"));
                        AuditLog(null, "Steam", "Parsed", "", i, batchQuerybuilder);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not fetch Steam ID {i}: {ex}");
                    }
                }
            }
        }

        public static string UTF8toASCII(string text)
        {
            System.Text.Encoding utf8 = System.Text.Encoding.UTF8;
            Byte[] encodedBytes = utf8.GetBytes(text);
            Byte[] convertedBytes =
                    Encoding.Convert(Encoding.UTF8, Encoding.ASCII, encodedBytes);
            System.Text.Encoding ascii = System.Text.Encoding.ASCII;

            return ascii.GetString(convertedBytes);
        }

        private static Task PropagateNewUsers()
        {
            DbDataReader reader;
            using (var conn = new MySqlConnection(config.ConnString))
            {
                conn.Open();
                var command = new MySqlCommand() { Connection = conn };

                command.CommandText = $@"
                                    SELECT member_id, 
                                            name, 
                                            fm.email,
                                            ip_address, 
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
                                                COALESCE( 
                                                    mgroup_others, ''))) AS checksum 
                                    FROM   familyrp.frpcore_members AS fm
                                    WHERE member_id IN (SELECT member_id FROM FamilyRpServerAccess.Users AS fsa
                                    RIGHT JOIN familyrp.frpcore_members AS fm ON fm.member_id = fsa.ForumId
                                    WHERE fsa.ForumId IS NULL
                                    ORDER BY fm.member_id DESC);";

                reader = command.ExecuteReader();
                List<UserDbModel> users = new List<UserDbModel>();
                int i = 1;

                using (var bqBuilder = new BatchQueryBuilder(config.ConnString))
                {
                    while (reader.Read())
                    {
                        var user = UserDbModel.Create(reader);

                        if (Convert.ToString(reader["steamid"]) != "")
                            AuditLog(Convert.ToString(reader["member_id"]), "Steam", "Added", null, Convert.ToString(reader["steamid"]), bqBuilder);
                        if (reader["steamid"] != DBNull.Value && !SteamIds.Contains(Convert.ToString(reader["steamid"])))
                            CreateNonParsedSteamId(Convert.ToString(reader["steamid"]), bqBuilder);

                        if (Convert.ToString(reader["discord_id"]) != "")
                            AuditLog(Convert.ToString(reader["member_id"]), "Discord", "Added", null, Convert.ToString(reader["discord_id"]), bqBuilder);
                        if (reader["discord_id"] != DBNull.Value && Convert.ToString(reader["discord_id"]) != null && !DiscordIds.Contains(Convert.ToString(reader["discord_id"])))
                            CreateNonParsedDiscordId(Convert.ToString(reader["discord_id"]), bqBuilder);

                        if (Convert.ToString(reader["tw_id"]) != "")
                            AuditLog(Convert.ToString(reader["member_id"]), "Twitch", "Added", null, Convert.ToString(reader["tw_id"]), bqBuilder);
                        if (reader["tw_id"] != DBNull.Value && Convert.ToString(reader["tw_id"]) != null && !TwitchIds.Contains(Convert.ToString(reader["tw_id"])))
                            CreateNonParsedTwitchId(Convert.ToString(reader["tw_id"]), bqBuilder);

                        if (Convert.ToString(reader["ip_address"]) != "")
                            AuditLog(Convert.ToString(reader["member_id"]), "IP", "Added", null, Convert.ToString(reader["ip_address"]), bqBuilder);
                        if (reader["ip_address"] != DBNull.Value && Convert.ToString(reader["ip_address"]) != null && !IpAddresses.Contains(Convert.ToString(reader["ip_address"])))
                            CreateIpAddress(Convert.ToInt32(reader["member_id"]), Convert.ToString(reader["ip_address"]), bqBuilder);

                        command = new MySqlCommand();

                        command.CommandText = $@"INSERT INTO FamilyRpServerAccess.Users (ForumId, Email, SteamId, DiscordId, TwitchId, ForumBanned, ForumPostCount, ServerBanned, ForumPmCount, ForumGroups, IsAdmin, IsDev, IsPolice, IsEMS, IsFireDept, LastLoggedInForum, ForumDbRowChecksum, CurrentIP) VALUES (@ForumId, @Email, @SteamId, @DiscordId, @TwitchId, @ForumBanned, @ForumPostCount, @ServerBanned, @ForumPmCount, @ForumGroups, @IsAdmin, @IsDev, @IsPolice, @IsEMS, @IsFireDept, @LastLoggedInForum, @ForumDbRowChecksum, @CurrentIP);";

                        command.Parameters.AddWithValue("ForumId", user.Forum.Id);
                        command.Parameters.AddWithValue("SteamId", user.Steam.Id);
                        command.Parameters.AddWithValue("DiscordId", user.Discord.Id);
                        command.Parameters.AddWithValue("TwitchId", user.Twitch.Id);
                        command.Parameters.AddWithValue("ForumBanned", user.Forum.BanTime != 0 ? 1 : 0);
                        command.Parameters.AddWithValue("Email", user.Forum.Email);
                        command.Parameters.AddWithValue("ForumPostCount", user.Forum.PostCount);
                        command.Parameters.AddWithValue("ServerBanned", 0);
                        command.Parameters.AddWithValue("ForumPmCount", user.Forum.MessageCount);
                        command.Parameters.AddWithValue("ForumGroups", String.Join(",", user.Forum.Groups));
                        command.Parameters.AddWithValue("IsAdmin", user.Forum.Groups.Contains(10) ? 1 : 0);
                        command.Parameters.AddWithValue("IsDev", user.Forum.Groups.Contains(11) ? 1 : 0);
                        command.Parameters.AddWithValue("IsPolice", user.Forum.Groups.Contains(9) ? 1 : 0);
                        command.Parameters.AddWithValue("IsEMS", user.Forum.Groups.Contains(20) ? 1 : 0);
                        command.Parameters.AddWithValue("IsFireDept", 0);
                        command.Parameters.AddWithValue("LastLoggedInForum", user.Forum.LastVisit);
                        command.Parameters.AddWithValue("ForumDbRowChecksum", user.Forum.RowChecksum);
                        command.Parameters.AddWithValue("CurrentIP", user.Forum.CurrentIP);
                        bqBuilder.AppendSqlCommand(command);
                        Console.WriteLine($"Populating row #{i} with forum id #{user.Forum.Id}");
                        AuditLog(user.Forum.Id.ToString(), "Forum", "Create", "", "", bqBuilder);
                        UpdateGroups(user.Forum.Id.ToString(), user.Forum.Groups, bqBuilder);
                        i++;
                    }
                }
                reader.Dispose();
            }
            return Task.CompletedTask;
        }

        private static void CreateNonParsedTwitchId(string twitchId, BatchQueryBuilder batchQueryBuilder)
        {
            TwitchIds.Add(twitchId);
            batchQueryBuilder.AppendSqlCommand(new MySqlCommand($"INSERT INTO FamilyRpServerAccess.ParsedTwitchAccounts (TwitchId, IsParsed, RowCreated, RowUpdated) VALUES ({MySqlHelper.EscapeString(twitchId)}, 0, NOW(), NOW());"));
        }

        private static void CreateIpAddress(int forumId, string Ip, BatchQueryBuilder batchQueryBuilder)
        {
            IpAddresses.Add(Ip);
            batchQueryBuilder.AppendSqlCommand(new MySqlCommand($"INSERT INTO FamilyRpServerAccess.IpAddresses (ForumId, IP, RowCreated, RowUpdated) VALUES ({forumId}, '{MySqlHelper.EscapeString(Ip)}', NOW(), NOW());"));
        }

        private static void CreateGtaId(string gtaId, BatchQueryBuilder batchQueryBuilder)
        {
            batchQueryBuilder.AppendSqlCommand(new MySqlCommand($"INSERT INTO FamilyRpServerAccess.GtaAccounts (GtaLicense, RowCreated, RowUpdated) VALUES ('{MySqlHelper.EscapeString(gtaId)}', NOW(), NOW());"));
        }

        private static void CreateNonParsedDiscordId(string discordId, BatchQueryBuilder batchQueryBuilder)
        {
            DiscordIds.Add(discordId);
            batchQueryBuilder.AppendSqlCommand(new MySqlCommand($"INSERT INTO FamilyRpServerAccess.ParsedDiscordAccounts (DiscordId, IsParsed, RowCreated, RowUpdated) VALUES ({Convert.ToUInt64(discordId)}, 0, NOW(), NOW());"));
        }

        private static void CreateNonParsedSteamId(string steamId, BatchQueryBuilder batchQueryBuilder)
        {
            SteamIds.Add(steamId);
            batchQueryBuilder.AppendSqlCommand(new MySqlCommand($"INSERT INTO FamilyRpServerAccess.ParsedSteamAccounts (SteamId, IsParsed, RowCreated, RowUpdated) VALUES ({MySqlHelper.EscapeString(steamId)}, 0, NOW(), NOW());"));
        }

        private static void PropagateChangedUsers()
        {
            DbDataReader reader;

            using (var conn = new MySqlConnection(config.ConnString))
            {
                conn.Open();
                var command = new MySqlCommand() { Connection = conn };

                command.CommandText = $@"
                                    SELECT member_id, 
                                            name, 
                                            fm.email,
                                            ip_address, 
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
                                                COALESCE(mgroup_others, ''))) AS checksum 
                                    FROM   familyrp.frpcore_members AS fm
                                    LEFT JOIN FamilyRpServerAccess.Users AS fsa ON fm.member_id = fsa.ForumId
                                    WHERE member_id IN (SELECT member_id FROM FamilyRpServerAccess.Users AS fsa
                                    INNER JOIN familyrp.frpcore_members AS fm ON fm.member_id = fsa.ForumId
                                    AND fsa.ForumDbRowChecksum <> MD5(CONCAT(COALESCE(fm.steamid, ''), COALESCE(fm.last_visit, ''), 
                                                    COALESCE(fm.last_activity, ''), COALESCE(fm.discord_id, ''), 
                                                COALESCE(fm.tw_id, ''), 
                                                    COALESCE(fm.temp_ban, ''), COALESCE(fm.member_posts, ''), 
                                                    COALESCE(fm.msg_count_total, ''), COALESCE(fm.member_group_id, ''), 
                                                COALESCE(fm.mgroup_others, ''))) 
                                    ORDER BY member_id DESC);";

                reader = command.ExecuteReader();


                List<UserDbModel> users = new List<UserDbModel>();
                int i = 0;

                using (var bqBuilder = new BatchQueryBuilder(config.ConnString))
                {
                    while (reader.Read())
                    {
                        var user = UserDbModel.Create(reader);

                        if (Convert.ToString(reader["steamid"]) != "" && Convert.ToString(reader["steamid"]) != Convert.ToString(reader["OldSteamId"]))
                            AuditLog(Convert.ToString(reader["member_id"]), "Steam", "Changed", Convert.ToString(reader["OldSteamId"]), Convert.ToString(reader["steamid"]), bqBuilder);
                        if (reader["steamid"] != DBNull.Value && !SteamIds.Contains(Convert.ToString(reader["steamid"])))
                            CreateNonParsedSteamId(Convert.ToString(reader["steamid"]), bqBuilder);

                        if (Convert.ToString(reader["discord_id"]) != "" && Convert.ToString(reader["discord_id"]) != Convert.ToString(reader["DiscordId"]))
                            AuditLog(Convert.ToString(reader["member_id"]), "Discord", "Changed", Convert.ToString(reader["DiscordId"]), Convert.ToString(reader["discord_id"]), bqBuilder);
                        if (reader["discord_id"] != DBNull.Value && Convert.ToString(reader["discord_id"]) != null && !DiscordIds.Contains(Convert.ToString(reader["discord_id"])))
                            CreateNonParsedDiscordId(Convert.ToString(reader["discord_id"]), bqBuilder);

                        if (Convert.ToString(reader["tw_id"]) != "" && Convert.ToString(reader["tw_id"]) != Convert.ToString(reader["TwitchId"]))
                            AuditLog(Convert.ToString(reader["member_id"]), "Twitch", "Changed", Convert.ToString(reader["TwitchId"]), Convert.ToString(reader["tw_id"]), bqBuilder);
                        if (reader["tw_id"] != DBNull.Value && Convert.ToString(reader["tw_id"]) != null && !TwitchIds.Contains(Convert.ToString(reader["tw_id"])))
                            CreateNonParsedTwitchId(Convert.ToString(reader["tw_id"]), bqBuilder);

                        if (Convert.ToString(reader["ip_address"]) != "" && Convert.ToString(reader["ip_address"]) != Convert.ToString(reader["OldIp"]))
                            AuditLog(Convert.ToString(reader["member_id"]), "IP", "Changed", Convert.ToString(reader["OldIp"]), Convert.ToString(reader["ip_address"]), bqBuilder);
                        if (reader["ip_address"] != DBNull.Value && Convert.ToString(reader["ip_address"]) != null && !IpAddresses.Contains(Convert.ToString(reader["ip_address"])))
                            CreateIpAddress(Convert.ToInt32(reader["member_id"]), Convert.ToString(reader["ip_address"]), bqBuilder);

                        command = new MySqlCommand();

                        command.CommandText = $@"UPDATE FamilyRpServerAccess.Users SET SteamId = @SteamId, DiscordId = @DiscordId, TwitchId = @TwitchId, GtaLicense = @GtaLicense, ForumBanned = @ForumBanned, ForumPostCount = @ForumPostCount, ForumPmCount = @ForumPmCount, ForumGroups = @ForumGroups, IsAdmin = @IsAdmin, IsDev = @IsDev, IsPolice = @IsPolice, IsEMS = @IsEMS, IsFireDept = @IsFireDept, LastLoggedInForum = @LastLoggedInForum, ForumDbRowChecksum = @ForumDbRowChecksum, CurrentIP = @CurrentIP, RowUpdated = NOW() WHERE ForumId = @ForumId;";

                        command.Parameters.AddWithValue("ForumId", user.Forum.Id);
                        command.Parameters.AddWithValue("SteamId", user.Steam.Id);
                        command.Parameters.AddWithValue("DiscordId", user.Discord.Id);
                        command.Parameters.AddWithValue("TwitchId", user.Twitch.Id);
                        command.Parameters.AddWithValue("GtaLicense", DBNull.Value);
                        command.Parameters.AddWithValue("ForumBanned", user.Forum.BanTime != 0 ? 1 : 0);
                        command.Parameters.AddWithValue("ForumPostCount", user.Forum.PostCount);
                        command.Parameters.AddWithValue("ForumPmCount", user.Forum.MessageCount);
                        command.Parameters.AddWithValue("ForumGroups", String.Join(",", user.Forum.Groups));
                        command.Parameters.AddWithValue("IsAdmin", user.Forum.Groups.Contains(10) ? 1 : 0);
                        command.Parameters.AddWithValue("IsDev", user.Forum.Groups.Contains(11) ? 1 : 0);
                        command.Parameters.AddWithValue("IsPolice", user.Forum.Groups.Contains(9) ? 1 : 0);
                        command.Parameters.AddWithValue("IsEMS", user.Forum.Groups.Contains(20) ? 1 : 0);
                        command.Parameters.AddWithValue("IsFireDept", 0);
                        command.Parameters.AddWithValue("LastLoggedInForum", user.Forum.LastVisit);
                        command.Parameters.AddWithValue("LastLoggedInGame", new DateTime(1900, 1, 1));
                        command.Parameters.AddWithValue("ForumDbRowChecksum", user.Forum.RowChecksum);
                        command.Parameters.AddWithValue("CurrentIP", user.Forum.CurrentIP);

                        bqBuilder.AppendSqlCommand(command);
                        Console.WriteLine($"Updating row #{i} with forum id #{user.Forum.Id}");
                        UpdateGroups(user.Forum.Id.ToString(), user.Forum.Groups, bqBuilder);
                        i++;
                    }
                }
                reader.Dispose();
            }
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