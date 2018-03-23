using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.Enums.Character;
using FamilyRP.Roleplay.Server.Enums;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Vehicle
{
    // To set fuel on a car when taken out of a garage, set the Vehicle.Fuel decor on it.
    // TODO: Potentially make the vehicles have different fuel tank sizes
    static class FuelManager
    {
        static float startingMultiplier = 1/1600f;
        static float FuelPumpRange = 4f;

        // For random vehicles with unassigned fuel levels
        static float minRandomFuel = 14f;
        static float maxRandomFuel = 97f;

        // Just placeholders for testing, feel free to change
        static Dictionary<VehicleClass, float> FuelConsumptionClassMultiplier = new Dictionary<VehicleClass, float>()
        {
            [VehicleClass.Planes] = 1 / 4f,
            [VehicleClass.Helicopters] = 1 / 4f,
            [VehicleClass.Super] = 1.2f,
            [VehicleClass.Sports] = 1.2f
        };

        static Dictionary<VehicleHash, float> FuelConsumptionModelMultiplier = new Dictionary<VehicleHash, float>()
        {
            //[VehicleHash.Infernus] = 100f // For testing
        };

        static List<ObjectHash> FuelPumpModelHashes = new List<ObjectHash>()
        {
            ObjectHash.prop_gas_pump_1a,
            ObjectHash.prop_gas_pump_1b,
            ObjectHash.prop_gas_pump_1c,
            ObjectHash.prop_gas_pump_1d,
            ObjectHash.prop_gas_pump_old2,
            ObjectHash.prop_gas_pump_old3,
            ObjectHash.prop_vintage_pump
        };

        static float fuelUsageMultiplier = -1;
        public static float vehicleFuel = -1;
        static private int currentUpdate = -1;
        static private int lastUpdate;
        static ObjectList ObjectList = new ObjectList();

        static bool isDev = true;


        static public void Init()
        {
            Function.Call(Hash.DECOR_REGISTER, "Vehicle.Fuel", 1);
            Function.Call(Hash.DECOR_REGISTER, "Vehicle.FuelUsageMultiplier", 1);
            //Client.GetInstance().ClientCommands.Register("/refuel", HandleRefuel);

            //var RefuelPayCashItem = new MenuItemStandard
            //{
            //    Title = "Refuel to Full: Pay Cash",
            //    OnActivate = (item) => Refuel(100)
            //};

            //var RefuelPayDebitItem = new MenuItemStandard
            //{
            //    Title = "Refuel to Full",
            //    OnActivate = (item) => Refuel(100)
            //};
            
            MenuItem ToRefuelMenuItem = new MenuItemStandard { Title = "Refuel to Full", OnActivate = (item) => Refuel(100) };
            InteractionListMenu.RegisterInteractionMenuItem(ToRefuelMenuItem, () => { return isNearFuelPump; }, 1150);
            
            PeriodicCheck();
        }

        private static Random random = new Random();
        private static double PlayerToVehicleRefuelRange = 5f;
        private static bool isNearFuelPump;
        private static int cooldown = 0;

        /// <summary>
        /// </summary>
        static async void PeriodicCheck()
        {
            while (true)
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    if (!Function.Call<bool>(Hash.DECOR_EXIST_ON, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel"))
                    {
                        // For very large random float numbers this method does not yield a uniform distribution
                        // But for this magnitude it is perfectly fine
                        float randomFuel = (float)(minRandomFuel + (maxRandomFuel - minRandomFuel) * (random.NextDouble()));
                        Function.Call(Hash._DECOR_SET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel", randomFuel);
                    }
                    vehicleFuel = Function.Call<float>(Hash._DECOR_GET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel");

                    if (!Function.Call<bool>(Hash.DECOR_EXIST_ON, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.FuelUsageMultiplier"))
                    {
                        fuelUsageMultiplier = startingMultiplier;
                        //Log.ToChat($"{fuelUsageMultiplier:0.00000}");
                        VehicleClass VehicleClass = (VehicleClass)Function.Call<int>(Hash.GET_VEHICLE_CLASS, Game.PlayerPed.CurrentVehicle.Handle);
                        fuelUsageMultiplier *= (FuelConsumptionClassMultiplier.ContainsKey(VehicleClass) ? FuelConsumptionClassMultiplier[VehicleClass] : 1.0f);
                        fuelUsageMultiplier *= FuelConsumptionModelMultiplier.ContainsKey((VehicleHash)(uint)Game.PlayerPed.CurrentVehicle.Model.Hash) ? FuelConsumptionModelMultiplier[(VehicleHash)(uint)Game.PlayerPed.CurrentVehicle.Model.Hash] : 1f;
                        Function.Call(Hash._DECOR_SET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.FuelUsageMultiplier", fuelUsageMultiplier);
                    }
                    if (lastUpdate == -1)
                    {
                        lastUpdate = Function.Call<int>(Hash.GET_GAME_TIMER);
                    }
                    if (fuelUsageMultiplier < 0)
                    {
                        fuelUsageMultiplier = Function.Call<float>(Hash._DECOR_GET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.FuelUsageMultiplier");
                    }
                    currentUpdate = Function.Call<int>(Hash.GET_GAME_TIMER); 
                    double deltaTime = (currentUpdate - lastUpdate) / 1000f;
                    float vehicleSpeed = Math.Abs(Game.PlayerPed.CurrentVehicle.Speed);
                    vehicleFuel = Math.Max(0f, vehicleFuel - (float)(deltaTime * fuelUsageMultiplier * vehicleSpeed));
                    Function.Call(Hash._DECOR_SET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel", vehicleFuel);
                    lastUpdate = currentUpdate;
                }
                else
                {
                    fuelUsageMultiplier = -1;
                    //vehicleFuel = -1;
                    lastUpdate = -1;
                }
                isNearFuelPump = ObjectList.Select(o => new Prop(o)).Where(o => FuelPumpModelHashes.Contains((ObjectHash)(uint)o.Model.Hash)).Any(o => o.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(2*FuelPumpRange, 2));
                await BaseScript.Delay(500);
            }
        }

        public static async void Refuel(float amount)
        {
            try
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#BB5555", $"You can't refuel while in your vehicle!");
                    return;
                }

                amount = Math.Max(0f, amount); // Selling gas to gas stations... Why not?
                amount = Math.Min(100f - vehicleFuel, amount);
                var NearbyVehicles = new VehicleList().Select(v => (CitizenFX.Core.Vehicle)Entity.FromHandle(v)).Where(v => v.Bones["wheel_rr"].Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(PlayerToVehicleRefuelRange, 2)).OrderBy(v => v.Bones["wheel_rr"].Position.DistanceToSquared(Game.PlayerPed.Position));
                if (!NearbyVehicles.Any())
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AA5555", "You are not close enough to a vehicle.");
                    return;
                }
                CitizenFX.Core.Vehicle vehicle = NearbyVehicles.First();

                var NearbyPumps = ObjectList.Select(o => new Prop(o)).Where(o => FuelPumpModelHashes.Contains((ObjectHash)(uint)o.Model.Hash)).Where(o => o.Position.DistanceToSquared(vehicle.Position) < Math.Pow(FuelPumpRange, 2));
                if (!NearbyPumps.Any())
                {
                    BaseScript.TriggerEvent("Chat.Message", "", "#AA5555", "You are not close enough to a pump.");
                    return;
                }

                int refuelTick = 1;
                float refuelTickAmount = 0.07f;
                float refuelRate = 0.35f;
                float refueled = 0f;

                vehicleFuel = Function.Call<float>(Hash._DECOR_GET_FLOAT, vehicle.Handle, "Vehicle.Fuel");
                if (vehicleFuel >= 0f) // -1f used as null
                {
                    Vector3 startingPosition = vehicle.Position;
                    while (refueled < amount)
                    {
                        if (startingPosition != vehicle.Position)
                        {
                            BaseScript.TriggerEvent("Chat.Message", "", "#BB5555", $"Your vehicle moved while refuelling.");
                            return;
                        }

                        //if too far away, print and return
                        refueled += refuelTickAmount;
                        vehicleFuel += refuelTickAmount;
                        await BaseScript.Delay(refuelTick);
                        vehicleFuel = Math.Min(100f, vehicleFuel);
                        Function.Call(Hash._DECOR_SET_FLOAT, vehicle.Handle, "Vehicle.Fuel", vehicleFuel);
                    }

                    BaseScript.TriggerEvent("Chat.Message", "", "#55BB55", $"You have finished refuelling.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"FuelManager Refuel Error: {ex.Message}");
            }
        }

        ///// <summary>
        ///// Refuels the local player's vehicle
        ///// </summary>
        ///// <param name="amount"></param>
        ///// <returns>Whether the refuel was successful or not</returns>
        //static public void HandleRefuel(Command command)
        //{
        //    try
        //    {
        //        float amount;
        //        if (command.Args.Count == 1)
        //        {
        //            amount = 100f;
        //            paymentType = command.Args.Get(0).ToLower() == "debit" ? MoneyType.Debit : MoneyType.Cash;
        //        }
        //        else if(command.Args.Count == 2)
        //        {
        //            amount = command.Args.GetFloat(0);
        //            paymentType = command.Args.Get(1).ToLower() == "debit" ? MoneyType.Debit : MoneyType.Cash;
        //        }
        //        else
        //        {
        //            BaseScript.TriggerEvent("Chat.Message", "", "#CC7777", "You need to specify either cash or debit, e.g. /refuel cash or refuel 30 debit.");
        //            return;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error($"FuelManager HandleRefuel Error: {ex.Message}");
        //    }
        //}
    }
}
