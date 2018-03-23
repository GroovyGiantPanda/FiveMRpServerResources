using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.Client.Helpers;
using CitizenFX.Core.Native;

namespace FamilyRP.Roleplay.Client.Classes.Vehicle
{
    // TODO: Potentially add multiplier based on either vehicle value or vehicle model/class
    // TODO: Polish; targeting from just character raycast to alternative methods
    // TODO: Add scenario/animation
    // TODO: Factor out lockpick functionality further so it can be called from context menus etc.
    static class Lockpicking
    {
        static Random random = new Random();

        static public void Init()
        {
            Client.GetInstance().ClientCommands.Register("/lockpick", Lockpick);
        }

        static private async void Lockpick(Command command)
        {
            try
            {
                var vehicle = WorldProbe.GetVehicleInFrontOfPlayer(3f);
                if (vehicle != null && vehicle != default(CitizenFX.Core.Vehicle))
                {
                    bool locked = Function.Call<bool>(Hash.DECOR_GET_BOOL, vehicle.Handle, "Vehicle.Locked") 
                        || vehicle.LockStatus == VehicleLockStatus.Locked 
                        || vehicle.LockStatus == VehicleLockStatus.LockedForPlayer;
                    if (locked)
                    {
                        Log.Verbose("Attempting to lockpick vehicle...");
                        await BaseScript.Delay(3000);
                        if (random.NextBool(30) || WorldProbe.GetVehicleInFrontOfPlayer() != vehicle) // Chance lockpicking is successful
                        {
                            BaseScript.TriggerEvent("Chat.Message", "[Lockpick]", "#ffffff", $@"Picked lock successfully!");
                            Function.Call(Hash.DECOR_SET_BOOL, vehicle.Handle, "Vehicle.Locked", false);
                            Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED_FOR_ALL_PLAYERS, vehicle.Handle, false);
                            vehicle.LockStatus = VehicleLockStatus.Unlocked;
                        }
                        else
                        {
                            BaseScript.TriggerEvent("Chat.Message", "[Lockpick]", "#ffffff", $@"Lockpick failed!");

                            int i = 30;
                            while (i > 0)
                            {
                                vehicle.SoundHorn(125);
                                Function.Call(Hash.SET_VEHICLE_LIGHTS, vehicle.Handle, 2);
                                await BaseScript.Delay(125);
                                Function.Call(Hash.SET_VEHICLE_LIGHTS, vehicle.Handle, 1);
                                await BaseScript.Delay(125);
                                i--;
                            }
                            Function.Call(Hash.SET_VEHICLE_LIGHTS, vehicle.Handle, 0);
                        }
                    }
                    else
                    {
                        Log.Verbose($"Unable to lockpick: Vehicle does not appear to be locked");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Lockpicking Lockpick Error: {ex.Message}");
            }
        }
    }
}
