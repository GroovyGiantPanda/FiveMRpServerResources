using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Vehicle
{
    static class RandomCarLocks
    {
        static float CarJackScareDistance = 25f;
        static double EnterVehicleRange = 1f;

        static Random random = new Random();
        static PlayerList PlayerList = new PlayerList();
        static VehicleList VehicleList = new VehicleList();

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
            Function.Call(Hash.DECOR_REGISTER, "Vehicle.PlayerOwned", 2);
            Function.Call(Hash.DECOR_REGISTER, "Vehicle.PlayerTouched", 2);
            Function.Call(Hash.DECOR_REGISTER, "Vehicle.Locked", 2);
        }

        static private async Task OnTick()
        {
            CitizenFX.Core.Vehicle vehicle = Game.PlayerPed.VehicleTryingToEnter;
            if (vehicle != null)
            {
                bool isPlayerOwned = Function.Call<bool>(Hash.DECOR_EXIST_ON, vehicle.Handle, "Vehicle.PlayerOwned");
                if (isPlayerOwned)
                {
                    // Just use current lock status if this is a player vehicle
                    return;
                }
                bool isLocked = Function.Call<bool>(Hash.DECOR_EXIST_ON, vehicle.Handle, "Vehicle.Locked");
                if (isLocked && Function.Call<bool>(Hash.DECOR_GET_BOOL, vehicle.Handle, "Vehicle.Locked"))
                {
                    // Only lock vehicle the moment the player gets close enough
                    // Circumvents an occasional glitch with LockedForPlayer
                    if (vehicle.Bones["handle_dside_f"].Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(EnterVehicleRange, 2) || vehicle.Bones["handle_pside_f"].Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(EnterVehicleRange, 2))
                    {

                        VehicleList.Select(v => new CitizenFX.Core.Vehicle(v)).Where(v => v.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(CarJackScareDistance, 2) && !v.IsSeatFree(VehicleSeat.Driver) && PlayerList.Where(p => p.Character == v.Driver).Count() == 0).ToList().ForEach(v =>
                        {
                            //Log.ToChat("Scaring driver");
                            Function.Call(Hash.DECOR_SET_BOOL, v.Handle, "Vehicle.Locked", true);
                            v.Driver.DrivingStyle = DrivingStyle.IgnoreLights | DrivingStyle.Rushed;
                            Function.Call(Hash._PLAY_AMBIENT_SPEECH1, v.Driver.Handle, random.NextBool(50) ? "GENERIC_FRIGHTENED_HIGH" : "GENERIC_FRIGHTENED_MED", "SPEECH_PARAMS_FORCE_SHOUTED_CRITICAL");
                            v.Driver.Task.CruiseWithVehicle(v, 60f, 2883621);
                        });

                        vehicle.LockStatus = VehicleLockStatus.Locked;
                    }
                    else
                    {
                        //Log.Debug("Not close");
                        vehicle.LockStatus = VehicleLockStatus.Unlocked;
                    }
                    return;
                }

                bool hasPlayerTouched = Function.Call<bool>(Hash.DECOR_EXIST_ON, vehicle.Handle, "Vehicle.PlayerTouched");
                if(hasPlayerTouched)
                {
                    // A player has already done these checks once; don't do them again
                    // Just keep current lock status
                    return;
                }
                else
                {
                    Function.Call<bool>(Hash.DECOR_SET_BOOL, vehicle.Handle, "Vehicle.PlayerTouched", true);
                }

                bool isDriverless;
                if (vehicle.IsSeatFree(VehicleSeat.Driver))
                {
                    isDriverless = true;
                }
                else
                {
                    isDriverless = false;
                }

                if(isDriverless)
                { 
                    if(random.NextBool(12)) // Chance stationary driverless vehicle will be unlocked
                    {
                        //Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED_FOR_ALL_PLAYERS, vehicle.Handle, false);
                        vehicle.NeedsToBeHotwired = true;
                        vehicle.LockStatus = VehicleLockStatus.CanBeBrokenIntoPersist;
                        //if (random.NextBool(75)) // Chance it will need to be hotwired
                        //{ 
                            //vehicle.NeedsToBeHotwired = true;
                        //}
                    }
                    else
                    {
                        Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED_FOR_ALL_PLAYERS, vehicle.Handle, true);
                        vehicle.LockStatus = VehicleLockStatus.Locked;
                        Function.Call<bool>(Hash.DECOR_SET_BOOL, vehicle.Handle, "Vehicle.Locked", true);
                    }
                }
                else
                {
                    // IRL probability is probably a lot higher
                    // But for balance I reduced it a bit
                    // Feel free to up it again
                    // Take into account the driver freaking out because somebody is running up to their door in the middle of the freeway
                    // If you have a weapon out I made the likelihood the driver locks his vehicle much higher
                    if ((Game.PlayerPed.Weapons.Current == WeaponHash.Unarmed && random.NextBool(50)) || random.NextBool(30)) // Chance moving vehicle will be unlocked
                    {
                        // This is what caused you to sometimes get you thrown out of the vehicle when pressing forward
                        // Not really needed anyway since a vehicle in this state will be unlocked at all times
                        //    Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED_FOR_ALL_PLAYERS, vehicle.Handle, false);
                        //    vehicle.LockStatus = VehicleLockStatus.Unlocked;
                        VehicleList.Select(v => new CitizenFX.Core.Vehicle(v)).Where(v => v.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(CarJackScareDistance, 2) && !v.IsSeatFree(VehicleSeat.Driver) && PlayerList.Where(p => p.Character == v.Driver).Count() == 0 && v != vehicle).ToList().ForEach(v =>
                        {
                            //Log.ToChat("Scaring driver");
                            Function.Call(Hash.DECOR_SET_BOOL, v.Handle, "Vehicle.Locked", true);
                            Function.Call(Hash._PLAY_AMBIENT_SPEECH1, v.Driver.Handle, random.NextBool(50) ? "GENERIC_FRIGHTENED_HIGH" : "GENERIC_FRIGHTENED_MED", "SPEECH_PARAMS_FORCE_SHOUTED_CRITICAL");
                            v.Driver.DrivingStyle = DrivingStyle.IgnoreLights | DrivingStyle.Rushed;
                            v.Driver.Task.CruiseWithVehicle(v, 60f, 2883621);
                        });
                    }
                    else
                    {
                        VehicleList.Select(v => new CitizenFX.Core.Vehicle(v)).Where(v => v.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(CarJackScareDistance, 2) && !v.IsSeatFree(VehicleSeat.Driver) && PlayerList.Where(p => p.Character == v.Driver).Count() == 0).ToList().ForEach(v =>
                        {
                            //Log.ToChat("Scaring driver");
                            Function.Call(Hash.DECOR_SET_BOOL, v.Handle, "Vehicle.Locked", true);
                            Function.Call(Hash._PLAY_AMBIENT_SPEECH1, v.Driver.Handle, random.NextBool(50) ? "GENERIC_FRIGHTENED_HIGH" : "GENERIC_FRIGHTENED_MED", "SPEECH_PARAMS_FORCE_SHOUTED_CRITICAL");
                            v.Driver.DrivingStyle = DrivingStyle.IgnoreLights | DrivingStyle.Rushed;
                            v.Driver.Task.CruiseWithVehicle(v, 60f, 2883621);
                        });
                        vehicle.LockStatus = VehicleLockStatus.Locked;
                    }
                }
                if(Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.LockStatus == VehicleLockStatus.CanBeBrokenIntoPersist)
                {
                    Game.PlayerPed.CurrentVehicle.LockStatus = VehicleLockStatus.Unlocked;
                }
            }
        }
    }
}