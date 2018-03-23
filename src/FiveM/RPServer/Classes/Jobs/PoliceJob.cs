using CitizenFX.Core;
using FamilyRP.Roleplay.Server.ClassesProcessing;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.SharedModels;
using System;
using System.Threading.Tasks;
using FamilyRP.Roleplay.SharedModels.PoliceEventModels;
using System.Linq;
using System.Collections.Generic;
using FamilyRP.Roleplay.Server.Shops;
using System.Text;
using FamilyRP.Roleplay.Helpers;
using System.ComponentModel;

namespace FamilyRP.Roleplay.Server.Classes.Jobs
{
	// You are welcome to turn this back into static later and initialize this somewhere
	public class PoliceJob : BaseScript
	{
		PlayerList playerList = new PlayerList();

		public PoliceJob()
		{
			EventHandlers["Arrest.ToggleCuffs"] += new Action<Player, int, int>(ToggleCuffs);
			EventHandlers["Police.Tackle"] += new Func<Player, string, Task>(Tackle);
			EventHandlers["Player.GsrResult"] += new Action<Player, int, string, bool>(HandleGsrResult);
			EventHandlers["Player.GsrTest"] += new Action<Player, int>(HandleGsrTest);
			EventHandlers["Police.AI911"] += new Action<Player, string>(HandleAI911);
			EventHandlers["Police.RunNcicBySessionID"] += new Action<Player, int>(HandleRunNcicBySessionID);
			EventHandlers["Police.RunNcicByCharID"] += new Action<Player, string, string, int>(HandleRunNcicByCharID);
			EventHandlers["Police.RunNcic"] += new Action<Player, string, string>(HandleRunNcic);
			EventHandlers["Police.RunPlate"] += new Action<Player, string>(HandleRunPlate);
			EventHandlers["Police.BillCharacter"] += new Action<Player, int, int, string>(HandleBillCharacter);
			EventHandlers["Police.TicketCharacter"] += new Action<Player, int, int, string>(HandleTicketCharacter);
			EventHandlers["Police.ImprisonCharacter"] += new Action<Player, int, int, string>(HandleImprisonCharacter);
			EventHandlers["Investigate.FriskInventory"] += new Action<Player, string>(HandleFriskCharacter);
			EventHandlers["Investigate.SearchInventory"] += new Action<Player, string>(HandleSearchCharacter);
			EventHandlers["NCIC.CharacterHistory"] += new Action<Player, string, string>(HandleCharacterHistory);
		}

		private void HandleAI911([FromSource]Player source, string data)
		{
			try
			{
				Log.Verbose($"PoliceJob HandleAI911");
				var deserialized = Helpers.MsgPack.Deserialize<CadAlertAIModel>(data);

				foreach (var player in SessionManager.PlayerList)
				{
					Log.Verbose($"Checking {player.Value.Character.CharID} duty = {player.Value.Character.Duty}");
					if (player.Value.Character.Duty.HasFlag(deserialized.SendAlertToGroup))
					{
						BaseScript.TriggerClientEvent(player.Value.Player, "Communication.IncomingAIAlert.Police", data);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error($"PoliceJob HandleAI911 Error: {ex.Message}");
			}
		}

		private void HandleGsrResult([FromSource] Player source, int requester, string name, bool result)
		{
			try
			{
				new PlayerList()[requester].TriggerEvent("Player.GsrResult", name, result);
			}
			catch (Exception ex)
			{
				Log.Error($"HandleGsrResult error: {ex}");
			}
		}

		private void HandleGsrTest([FromSource] Player source, int targetPlayer)
		{
			try
			{
				TriggerClientEvent(new PlayerList()[targetPlayer], "Player.GsrTest", source.Handle);
			}
			catch (Exception ex)
			{
				Log.Error($"HandleGsrTest error: {ex.Message}");
			}
		}

		/// <summary>
		/// Toggle cuffs on player
		/// </summary>
		public static void ToggleCuffs([FromSource] Player source, int targetPlayer, int cuffState)
		{
			try
			{
				Player target = new PlayerList()[targetPlayer];
				TriggerClientEvent(target, "Arrest.ToggleCuffs", cuffState);
				//Log.Info($"{source.Name} (#{source.Handle}) has toggled cuff state of {target.Name} (#{target.Handle}) to '{cuffState.ToString()}'");
			}
			catch (Exception ex)
			{
				Log.Error($"HandleToggleCuffsGsrTest error: {ex.Message}");
			}
		}

		/// <summary>
		/// Request tackle of specified players
		/// </summary>
		public Task Tackle([FromSource] Player source, string serializedArguments)
		{
			try
			{
				//Log.Debug("In tackle routine!");
				TackleEvent arguments = Helpers.MsgPack.Deserialize<TackleEvent>(serializedArguments);
				arguments.SourceId = Int32.Parse(source.Handle);
				string reserializedArguments = Helpers.MsgPack.Serialize(arguments);
				arguments.Targets.ForEach(t =>
				{
					Player target = this.playerList[t];
					TriggerClientEvent(target, "Police.Tackle", Helpers.MsgPack.Serialize(arguments));
				});
				//Log.Debug($"{source.Name} (#{source.Handle}) has tackled player(s) {String.Join(", ", arguments.Targets.ConvertAll(p => playerList[p].Name))}");
			}
			catch (Exception ex)
			{
				Log.Error($"Tackle (server) error: {ex.Message}");
			}
			return Task.CompletedTask;
		}

	}
}
