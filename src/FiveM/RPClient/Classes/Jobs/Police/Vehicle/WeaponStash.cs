using CitizenFX.Core;
using FamilyRP.Common;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Jobs.Police.Vehicle
{
    // TODO: Test and polish
    static class WeaponStash
    {
        static private bool isPolice = true;

        static WeaponHash currentWeapon = WeaponHash.Unarmed;
        static List<WeaponHash> StashedWeapons = new List<WeaponHash>()
        {
            WeaponHash.Unarmed,
            WeaponHash.CombatPistol,
            WeaponHash.StunGun,
            WeaponHash.CarbineRifle,
            WeaponHash.PumpShotgun
        };

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static private Task OnTick()
        {
            if(isPolice && ControlHelper.IsControlJustPressed(Control.MultiplayerInfo) && Game.PlayerPed.IsInVehicle())
            {
                WeaponHash selectedWeapon = currentWeapon = StashedWeapons[(StashedWeapons.IndexOf(currentWeapon) + 1) % StashedWeapons.Count];
                Game.PlayerPed.Weapons.Select(currentWeapon);
                BaseScript.TriggerEvent("Chat.Message", "", "#AA77DD", currentWeapon != WeaponHash.Unarmed ? $"Readied {Enum.GetName(typeof(WeaponHash), currentWeapon).AddSpacesToCamelCase()}." : "Stashed weapons.");
                Task.Factory.StartNew(async () =>
                {
                    while (Game.PlayerPed.IsInVehicle() && currentWeapon == selectedWeapon)
                    {
                        await BaseScript.Delay(0);
                    }
                    Game.PlayerPed.Weapons.Select(selectedWeapon);
                });
            }
            return Task.FromResult(0);
        }
    }
}