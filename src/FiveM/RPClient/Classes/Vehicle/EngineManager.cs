using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Vehicle
{
    static class EngineManager
    {
        static bool isInVehicle = false;
        static bool EngineDisabled = false;
        static MenuItem menuItem = new MenuItemCheckbox { Title = "Turn On Engine", state = true, OnActivate = (state, item) => { ToggleEngine(state); } };

        static public void Init()
        {
            PeriodicCheck();
            Client.GetInstance().ClientCommands.Register("/engine", HandleEngineToggle);
            InteractionListMenu.RegisterInteractionMenuItem(menuItem, () => Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed, 500);
        }

        static private async void HandleEngineToggle(Command command)
        {
            try
            {
                if (Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Game.PlayerPed)
                {
                    if (command.Args.Count == 0)
                    {
                        ToggleEngine(!Game.PlayerPed.CurrentVehicle.IsEngineRunning);
                    }
                    else if (command.Args.Get(0) == "on")
                    {
                        ToggleEngine(true);
                    }
                    else if (command.Args.Get(0) == "off")
                    {
                        ToggleEngine(false);
                    }
                }
                else
                {
                    Log.ToChat("You need to be driving a vehicle first.");
                }
            }
            catch (Exception)
            {

            }
        }

        static async void ToggleEngine(bool toggle)
        {
            try
            {
                if (toggle && Game.PlayerPed.CurrentVehicle.EngineHealth > 0f)
                {
                    Game.PlayerPed.CurrentVehicle.IsEngineRunning = true;
                    EngineDisabled = false;
                }
                else
                {
                    EngineDisabled = true;
                    while (Game.PlayerPed.IsInVehicle() && EngineDisabled)
                    {
                        if (Game.PlayerPed.CurrentVehicle.IsEngineRunning)
                            Game.PlayerPed.CurrentVehicle.IsEngineRunning = false;
                        await BaseScript.Delay(0);
                    }
                    // When player leaves vehicle:
                    EngineDisabled = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"ToggleEngine error: {ex.Message}");
            }
        }

        static private async void PeriodicCheck()
        {
            while (true)
            {
                if (Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Game.PlayerPed)
                {
                    // Makes sure player tries to start the engine the moment he gets into the driver's seat
                    if (!isInVehicle && Game.PlayerPed.CurrentVehicle.EngineHealth > 0 && !Game.PlayerPed.CurrentVehicle.NeedsToBeHotwired)
                    {
                        Game.PlayerPed.CurrentVehicle.IsEngineRunning = true;
                        isInVehicle = true;
                    }

                    if (FuelManager.vehicleFuel <= 0f && Game.PlayerPed.CurrentVehicle.IsEngineRunning == true && Function.Call<bool>(Hash.DECOR_EXIST_ON, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel") && Function.Call<float>(Hash._DECOR_GET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel") <= 0f)
                    {
                        // Update rates are different for these so maybe fuel just has not updated yet after getting into the vehicle
                        // (Fuel does not update as often)
                        // (Need to turn off engine every tick or lights flicker and ped freaks out)
                        Game.PlayerPed.CurrentVehicle.IsEngineRunning = false;
                    }
                }
                else
                {
                    isInVehicle = false;
                }
                await BaseScript.Delay(0);
            }
        }
    }
}
