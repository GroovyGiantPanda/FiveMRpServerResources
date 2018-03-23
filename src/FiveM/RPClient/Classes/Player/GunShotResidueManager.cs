using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Common;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.Enums.Character;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyRP.Roleplay.Enums.Session;

namespace FamilyRP.Roleplay.Client.Classes.Player
{
	static class GunShotResidueManager
	{
		private static readonly List<WeaponHash> BlackList = new List<WeaponHash>()
		{
			WeaponHash.PetrolCan,
			WeaponHash.Snowball,
			WeaponHash.Ball,
			WeaponHash.FlareGun,
			WeaponHash.PetrolCan,
			WeaponHash.Snowball,
			WeaponHash.Ball,
			WeaponHash.FlareGun,
			WeaponHash.Grenade,
			WeaponHash.SmokeGrenade,
			WeaponHash.Flare,
			WeaponHash.BZGas,
			WeaponHash.Firework,
			WeaponHash.Molotov,
			WeaponHash.ProximityMine,
			WeaponHash.StickyBomb,
			WeaponHash.StunGun,
			WeaponHash.PipeBomb
		};

		private const int GsrDuration = 60 * 30 * 1000;
		private const float GsrRange = 4f;

		private static readonly PlayerList PlayerList = new PlayerList();
		private static int _lastShotTime = -1;

		public static void Init()
		{
			Client.GetInstance().RegisterTickHandler(OnTick);
			Client.GetInstance().ClientCommands.Register("/gsr", TriggerChecks);
			Client.GetInstance().RegisterEventHandler("Player.GSRTest", new Action<int>(Check));
			Client.GetInstance().RegisterEventHandler("Player.GsrResult", new Action<string, bool>(ReceiveResult));
			MenuItem gsrTestMenuItem = new MenuItemStandard { Title = "GSR Test Nearby", OnActivate = (item) => TriggerChecks(new Command("")) };
			InteractionListMenu.RegisterInteractionMenuItem(gsrTestMenuItem, () => true, 980);
		}

		private static void ReceiveResult(string name, bool result)
		{
			try
			{
				BaseScript.TriggerEvent("Chat.Message", "", "#AAEE66", $"{name}'s GSR test has returned \"{(result ? "Positive" : "Negative")}\".");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "ReceiveResult error");
			}
		}

		private static async Task OnTick()
		{
			if(!BlackList.Contains(Game.PlayerPed.Weapons.Current.Hash)) {
				_lastShotTime = Function.Call<int>(Hash.GET_GAME_TIMER);
			}
		}

		private static void Check(int requester)
		{
			var gsrResult = (_lastShotTime != -1 && ( Function.Call<int>(Hash.GET_GAME_TIMER) - _lastShotTime > GsrDuration ));
			BaseScript.TriggerServerEvent("Player.GSRResult", requester, $"{Game.Player.Name}", gsrResult);
		}

		private static void TriggerChecks(Command command)
		{
			var targetList = PlayerList.Where(p => p.Handle != Game.Player.Handle &&
					p.Character.Position.DistanceToSquared(Game.PlayerPed.Position) < GsrRange)
						.OrderBy(p => p.Character.Position.DistanceToSquared(Game.PlayerPed.Position));
			if(targetList.Any())
			{
				BaseScript.TriggerServerEvent("Player.GSRTest", targetList.First().ServerId);
			}
			else
			{
				BaseScript.TriggerEvent("Chat.Message", "GSR", "#FF0000", "Couldn't find a valid nearby person for a GSR test.");
			}
		}
	}
}
