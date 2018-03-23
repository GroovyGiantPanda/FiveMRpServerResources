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
    // TODO: Test and debug
    static class Stay
    {
        static bool stayToggle = true;

        static public void Init()
        {
            Client.GetInstance().ClientCommands.Register("/stay", StayToggle);
        }

        static public void StayToggle(Command command)
        {
            try
            {
                stayToggle = !stayToggle;
                if (stayToggle)
                {
                    StayLoop();
                }
            }
            catch (Exception)
            {

            }
        }

        static private async void StayLoop()
        {
            if (Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.GetPedOnSeat(VehicleSeat.Passenger) == Game.PlayerPed)
            {
                while (stayToggle)
                {
                    if (ControlHelper.IsControlJustPressed(Control.Enter))
                    {
                        stayToggle = false;
                    }
                    if ((Game.PlayerPed.CurrentVehicle.GetPedOnSeat(VehicleSeat.Passenger) == Game.PlayerPed) || Function.Call<bool>(Hash.CAN_SHUFFLE_SEAT, Game.PlayerPed.CurrentVehicle.Handle, true))
                    {
                        Game.PlayerPed.Task.ClearAll();
                    }
                    await BaseScript.Delay(0);
                }
            }
            else
            {
                stayToggle = false;
            }
        }
    }
}