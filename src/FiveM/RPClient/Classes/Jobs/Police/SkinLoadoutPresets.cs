using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyRP.Roleplay.SharedClasses;

namespace FamilyRP.Roleplay.Client.Classes.Jobs.Police.Vehicle
{
    static class SkinLoadoutPresets
    {
        static List<string> PoliceSkins = new List<string>() { "s_m_y_cop_01", "s_m_y_sheriff_01",  "s_f_y_cop_01" };

        static Dictionary<WeaponHash, int> PoliceGear = new Dictionary<WeaponHash, int>()
        {
            [WeaponHash.Flashlight] = 1,
            [WeaponHash.Nightstick] = 1,
            [WeaponHash.CombatPistol] = 150,
            [WeaponHash.SmokeGrenade] = 6,
            [WeaponHash.FireExtinguisher] = 1150,
            [WeaponHash.PetrolCan] = 1150,
            [WeaponHash.PumpShotgun] = 150,
            [WeaponHash.FlareGun] = 2,
            [WeaponHash.Flare] = 20,
            [WeaponHash.CarbineRifle] = 1150,
            [WeaponHash.StunGun] = 1
        };

        static Dictionary<WeaponHash, WeaponComponentHash> PoliceGearComponents = new Dictionary<WeaponHash, WeaponComponentHash>()
        {
            [WeaponHash.CombatPistol] = WeaponComponentHash.AtPiFlsh,
            [WeaponHash.PumpShotgun] = WeaponComponentHash.AtArFlsh,
            [WeaponHash.CarbineRifle] = WeaponComponentHash.AtArAfGrip,
            [WeaponHash.CarbineRifle] = WeaponComponentHash.SpecialCarbineClip02,
            [WeaponHash.CarbineRifle] = WeaponComponentHash.AtArFlsh,
            [WeaponHash.CarbineRifle] = WeaponComponentHash.AtScopeMedium
        };

        static Dictionary<WeaponHash, int> PoliceGearSwat = new Dictionary<WeaponHash, int>()
        {
            [WeaponHash.Pistol50] = 250,
            [WeaponHash.AssaultSMG] = 200,
            [WeaponHash.HeavySniper] = 150,
            [WeaponHash.SpecialCarbine] = 1600
        };

        static Dictionary<WeaponHash, WeaponComponentHash> PoliceGearSwatComponents = new Dictionary<WeaponHash, WeaponComponentHash>()
        {
            [WeaponHash.Pistol50] = WeaponComponentHash.AtPiFlsh,
            [WeaponHash.AssaultSMG] = WeaponComponentHash.AtArFlsh,
            [WeaponHash.AssaultSMG] = WeaponComponentHash.AssaultSMGClip02,
            [WeaponHash.AssaultSMG] = WeaponComponentHash.AtScopeMacro,
            [WeaponHash.HeavySniper] = WeaponComponentHash.AtScopeMax,
            [WeaponHash.SpecialCarbine] = WeaponComponentHash.SpecialCarbineClip02,
            [WeaponHash.SpecialCarbine] = WeaponComponentHash.AtArFlsh,
            [WeaponHash.SpecialCarbine] = WeaponComponentHash.AtArAfGrip,
            [WeaponHash.SpecialCarbine] = WeaponComponentHash.AtScopeMedium
        };

        static public void Init()
        {
            Client.GetInstance().ClientCommands.Register("/duty", HandleDuty);
        }

        static private async void HandleDuty(Command command)
        {
            try
            {
                int arg = command.Args.GetInt32(0);
                if (arg >= 1 && arg <= PoliceSkins.Count)
                {
                    await Game.Player.ChangeModel(new Model(PoliceSkins[arg-1]));
                    await BaseScript.Delay(0);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exception in HandleDuty: {ex}");
            }
        }

        public static void ApplyLoadout(string skinName)
        { 
            Game.PlayerPed.Armor = 100;
            PoliceGear.ToList().ForEach(g => Game.PlayerPed.Weapons.Give(g.Key, g.Value, true, true));
            PoliceGearComponents.ToList().ForEach(g => Game.PlayerPed.Weapons[g.Key].Components[g.Value].Active = true);

            if (skinName == "s_m_y_swat_01")
            {
                PoliceGearSwat.ToList().ForEach(g => Game.PlayerPed.Weapons.Give(g.Key, g.Value, true, true));
                PoliceGearSwatComponents.ToList().ForEach(g => Game.PlayerPed.Weapons[g.Key].Components[g.Value].Active = true);
            }
        }
    }
}