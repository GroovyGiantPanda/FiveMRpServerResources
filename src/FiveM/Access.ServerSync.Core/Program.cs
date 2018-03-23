using System;
using System.Text;
using MySql.Data.MySqlClient;
using YamlDotNet.Serialization;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using Access.WebParse.Core;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Security;

namespace Access.ServerSync.Core
{
	class Program
	{

		static Dictionary<WhitelistType, Dictionary<string, string>> WhitelistState = new Dictionary<WhitelistType, Dictionary<string, string>>();

		public enum WhitelistActionFilter
		{
			Remove,
			Keep,
			Add,
			RemoveAdd,
			All
		}

		public enum WhitelistType
		{
			Tester,
			Developer,
			Whitelisted,
			PublicPlayer
		}

		static Dictionary<WhitelistType, string> schemaTable = new Dictionary<WhitelistType, string>()
		{
			[WhitelistType.Developer] = "FullSpectrumDevWhitelist",
			[WhitelistType.Tester] = "DevServerState",
			[WhitelistType.PublicPlayer] = "PublicServerState",
			[WhitelistType.Whitelisted] = "WhitelistServerState"
		};

		static Dictionary<WhitelistActionFilter, string> qualifiers = new Dictionary<WhitelistActionFilter, string>()
		{
			[WhitelistActionFilter.Add] = " WHERE State = 'Add'",
			[WhitelistActionFilter.Keep] = " WHERE State = 'Keep'",
			[WhitelistActionFilter.Remove] = " WHERE State = 'Remove'",
			[WhitelistActionFilter.RemoveAdd] = " WHERE State IN ('Remove', 'Add')",
			[WhitelistActionFilter.All] = ""
		};

		static ConfigModel config;
		static void Main(string[] args)
		{
			DeserializerBuilder deserializerBuilder = new DeserializerBuilder();
			Deserializer deserializer = deserializerBuilder.Build();
			config = deserializer.Deserialize<ConfigModel>(new StringReader(File.ReadAllText("config.yaml")));
			Console.WriteLine($"Web Server MySql Connection String: {config.ConnString}");

			RunEvery(UpdateWhitelists);

			bool quitFlag = false;
			while (!quitFlag)
			{
				var keyInfo = Console.ReadKey();
				quitFlag = keyInfo.Key == ConsoleKey.C && keyInfo.Modifiers == ConsoleModifiers.Control;
			}
		}

		public static Task UpdateWhitelists()
		{
			using (BatchQueryBuilder batchQueryBuilder = new BatchQueryBuilder(config.ConnString))
			{
				foreach (WhitelistType type in Enum.GetValues(typeof(WhitelistType)))
				{
					Dictionary<string, string> Ips = FetchIps(type, WhitelistActionFilter.Add);
					foreach (KeyValuePair<string, string> Ip in Ips)
					{
						AddUserToFirewall(type, Ip.Key, batchQueryBuilder);
					}

					Ips = FetchIps(type, WhitelistActionFilter.Remove);
					foreach (KeyValuePair<string, string> Ip in Ips)
					{
						RemoveUserFromFirewall(type, Ip.Key, batchQueryBuilder);
					}
				}
			}

			return Task.CompletedTask;
		}

		private static void AuditLog(string WhitelistType, string Ip, string ActionType, BatchQueryBuilder batchQueryBuilder)
		{
			batchQueryBuilder.AppendSqlCommand(new MySqlCommand($@"INSERT INTO FamilyRpServerAccess.ServerWhitelistLog (Date, ActionType, Ip, WhitelistType) VALUES
                                (NOW(), '{ActionType}', '{Ip}', '{WhitelistType}');"));
		}

		private static void AddUserToFirewall(WhitelistType whitelistType, string Ip, BatchQueryBuilder batchQueryBuilder)
		{
			Console.WriteLine($"Adding IP {Ip} as {whitelistType}");

			// TODO: Delete IP from server here
			// Example:
			// var ci = new CsfInterface.Client();
			// await ci.LoginToCsf("username", "password", "baseUrl");
			// await ci.AddTempWhiteListIp(Ip, "", "30124", 200);
			// Use Config.MasterIps and whitelistType to map to correct server
			// If success:
			UpdateSyncStatus(whitelistType, Ip, "Keep", batchQueryBuilder);
		}

		private static void RemoveUserFromFirewall(WhitelistType whitelistType, string Ip, BatchQueryBuilder batchQueryBuilder)
		{
			Console.WriteLine($"Removing IP {Ip} as {whitelistType}");

			// TODO: Delete IP from server here
			// Use Config.MasterIps and whitelistType to map to correct server
			// If success:
			DeleteSyncRow(whitelistType, Ip, batchQueryBuilder);
		}

		private static Dictionary<string, string> FetchIps(WhitelistType whitelistType, WhitelistActionFilter whitelistActionFilter)
		{
			using (var conn = new MySqlConnection(config.ConnString))
			{
				conn.Open();

				MySqlCommand command = new MySqlCommand($"SELECT IP, State FROM FamilyRpServerAccess.{schemaTable[whitelistType]}{qualifiers[whitelistActionFilter]};", conn);
				var reader = command.ExecuteReader();
				Dictionary<string, string> result = new Dictionary<string, string>();
				while (reader.Read()) result.Add(Convert.ToString(reader["IP"]), Convert.ToString(reader["State"]));
				reader.Dispose();
				return result;
			}
		}

		private static void UpdateSyncStatus(WhitelistType whitelistType, string Ip, string targetState, BatchQueryBuilder batchQueryBuilder)
		{
			var command = new MySqlCommand($"UPDATE FamilyRpServerAccess.{schemaTable[whitelistType]} SET State = @State WHERE IP = @IP;");
			command.Parameters.AddWithValue("State", targetState);
			command.Parameters.AddWithValue("IP", Ip);
			batchQueryBuilder.AppendSqlCommand(command);
		}

		private static void DeleteSyncRow(WhitelistType whitelistType, string Ip, BatchQueryBuilder batchQueryBuilder)
		{
			var command = new MySqlCommand($"DELETE FROM FamilyRpServerAccess.{schemaTable[whitelistType]} WHERE IP = @IP;");
			command.Parameters.AddWithValue("IP", Ip);
			batchQueryBuilder.AppendSqlCommand(command);
		}

		public static async void RunEvery(Func<Task> action, int periodicity = 60000)
		{
			await Task.Run(async () =>
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
			});
		}
	}

	internal class ConfigModel
	{
		public string ConnString { get; set; }
		public Dictionary<string, string> MasterIps { get; set; }
		// TODO: Add CSF credentials here too
	}
}
