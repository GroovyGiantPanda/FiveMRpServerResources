using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client
{
    static class VehicleDamage
    {
        private static Random rand = new Random();
        const float BikeDamageMultiplier = 1.0f; // Handling file changes to structural may make this no longer needed
        static float currentVehicleBodyHealth = 0;
        static float currentVehicleEngineHealth = 0;
        static float oldVehicleBodyHealth = 0;
        static float oldVehicleEngineHealth = 0;

        static private bool notInVehicle = true;

        static MenuItemStandard menuItemFix = new MenuItemStandard { Title = "Fix Vehicle", OnActivate = (item) => Fix() };

        static public void Init()
        {
            InteractionListMenu.RegisterInteractionMenuItem(menuItemFix, () => Game.PlayerPed.IsInVehicle() /* && isCop */, 999);
            PeriodicUpdate();
        }

        static private async Task PeriodicUpdate()
        {
            while (true)
            {
                await BaseScript.Delay(250);
                if(Game.PlayerPed.IsInVehicle())
                {
                    if(notInVehicle)
                    { 
                        notInVehicle = false;
                        oldVehicleBodyHealth = 0.0f;
                        oldVehicleEngineHealth = 0.0f;
                    }
                    var vehicle = Game.PlayerPed.CurrentVehicle;
                    bool isMotorBike = false;//vehicle.Model.IsBike;
                    currentVehicleBodyHealth = vehicle.BodyHealth;
                    currentVehicleEngineHealth = vehicle.EngineHealth;

                    float vehicleBodyDamageTaken = Math.Max(0.0f, oldVehicleBodyHealth - currentVehicleBodyHealth);
                    if(isMotorBike)
                    {
                        currentVehicleBodyHealth += vehicleBodyDamageTaken * (1.0f - BikeDamageMultiplier);
                        vehicle.BodyHealth = currentVehicleBodyHealth;
                        vehicleBodyDamageTaken *= BikeDamageMultiplier;
                        currentVehicleEngineHealth = vehicle.EngineHealth;

                        // Engine damage seems to be very broken for motorbikes
                        // Nearly all hits do 0 damage
                        // Only hits > 500 register
                        // Could be due to something in the handling file
                        // Or just GTA
                        // I also tried letting EngineHealth mirror BodyHealth but that made the bike break on small hits again
                        if (currentVehicleBodyHealth > 0f)
                        {
                            vehicle.EngineHealth = 1000f;
                        }
                    }
                    //Log.ToChat($"{currentVehicleBodyHealth} {currentVehicleEngineHealth}");
                    float vehicleEngineDamageTaken = isMotorBike ? 0.0f : Math.Max(0.0f, oldVehicleEngineHealth - currentVehicleEngineHealth);

                    // Disable cruise control on any damage taken
                    if (vehicleEngineDamageTaken > 0.0f || vehicleBodyDamageTaken > 0.0f)
                    {
                        CruiseControl.CruiseControlActive = false;
                    }
                    //Log.ToChat($"{vehicleBodyDamageTaken} {vehicleEngineDamageTaken}");
                    oldVehicleBodyHealth = currentVehicleBodyHealth;
                    oldVehicleEngineHealth = currentVehicleEngineHealth;
                    if (vehicleBodyDamageTaken > 10.875f || vehicleEngineDamageTaken > 10.5f)
                    {
                        float higherDamage = vehicleBodyDamageTaken > vehicleEngineDamageTaken ? vehicleBodyDamageTaken : vehicleEngineDamageTaken;
                        Game.PlayerPed.Health = (int)Math.Floor(Math.Max(110, Game.PlayerPed.Health - 0.1f * higherDamage));
                    }
                    if ((vehicleBodyDamageTaken >= 100.0f || currentVehicleEngineHealth < 75f) && rand.NextBool(10) && vehicleBodyDamageTaken >= 10f)
                    {
                        vehicle.Wheels[rand.Next(0, 5)].Burst();
                    }
                    // Last part is custom for bikes
                    // As engine damage is relatively useless
                    if (vehicleEngineDamageTaken > 90.5f || currentVehicleBodyHealth == 0.0f || (isMotorBike && vehicleBodyDamageTaken >= 100.0f)) 
                    {
                        // Because of my new bike logic
                        oldVehicleBodyHealth = 0.0f;
                        oldVehicleEngineHealth = 0.0f;
                        vehicle.IsEngineRunning = false;
                        vehicle.EngineHealth = 0.0f;
                        vehicle.BodyHealth = 0.0f;
                    }
                    if (!vehicle.IsOnAllWheels && Function.Call<bool>(Hash.IS_VEHICLE_STUCK_ON_ROOF, vehicle) && vehicle.IsStopped)
                    {
                        Function.Call(Hash.SET_PED_GET_OUT_UPSIDE_DOWN_VEHICLE, Game.PlayerPed.Handle, true);
                        vehicle.IsEngineRunning = false;
                        Game.PlayerPed.Task.LeaveVehicle((LeaveVehicleFlags)4160);
                    }
                }
                else
                {
                    notInVehicle = true;
                }
            }
        }

        public static void Fix()
        {
            if(Game.PlayerPed.IsInVehicle())
            { 
                Game.PlayerPed.CurrentVehicle.Repair();
                oldVehicleBodyHealth = 1000f;
                oldVehicleEngineHealth = 1000f;
                Game.PlayerPed.CurrentVehicle.EngineHealth = 1000f;
                Game.PlayerPed.CurrentVehicle.BodyHealth = 1000f;
                Game.PlayerPed.CurrentVehicle.PetrolTankHealth = 1000f;
                Game.PlayerPed.CurrentVehicle.DirtLevel = 0.0f;
                // Leave engine alone if it's already running to prevent the off/on animation
                if (!Game.PlayerPed.CurrentVehicle.IsEngineRunning)
                {
                    Game.PlayerPed.CurrentVehicle.IsEngineRunning = true;
                }
            }
        }
    }
}