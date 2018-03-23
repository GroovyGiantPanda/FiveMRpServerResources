using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Vehicle
{
    // Tested and working
    static class BannedMilitaryVehicles
    {
        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static public async Task OnTick()
        {
            List<VehicleHash> bannedVehicles = new List<VehicleHash>()
            { VehicleHash.Rhino, VehicleHash.Lazer, VehicleHash.Hydra };
            if (Game.PlayerPed.IsInVehicle() && bannedVehicles.Contains((VehicleHash)Game.PlayerPed.CurrentVehicle.Model.Hash))
            {
                Log.ToChat("In illegal vehicle");
                Function.Call(Hash.TASK_LEAVE_VEHICLE, Game.PlayerPed.Handle, Game.PlayerPed.CurrentVehicle.Handle, 4160);
                //Tick -= OnTick;
                await BaseScript.Delay(3000);
                //Tick += OnTick;
                // Also report it and log it here
                // TriggerServerEvent(...)
            }
        }
    }
}
