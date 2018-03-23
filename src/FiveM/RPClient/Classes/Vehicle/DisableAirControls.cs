using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Vehicle
{
    static class DisableAirControls
    {
        public static void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        private static Task OnTick()
        {
            if (Game.PlayerPed.Exists() && Game.PlayerPed.IsInVehicle() && (Game.PlayerPed.CurrentVehicle.Model.IsCar || Game.PlayerPed.CurrentVehicle.Model.IsBike) && (Game.PlayerPed.CurrentVehicle.IsInAir/* || Game.PlayerPed.CurrentVehicle.IsUpsideDown*/))
            {
                if (ControlHelper.IsControlJustPressed(Control.VehicleExit))
                    Game.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.BailOut);
                else
                    Game.DisableAllControlsThisFrame(27);
            }
            return Task.FromResult(0);
        }
    }
}
