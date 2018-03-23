using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyRP.Roleplay.SharedClasses;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace FamilyRP.Roleplay.Client.Classes.Jobs.Police.Vehicle
{
    static class VehicleLoadoutPresets
    {
        public static List<string> PoliceVehicles = new List<string>()
            {
                "police", "police2"
            }.ToList();

        struct ModSetting
        {
            public VehicleModType modType;
            public int modIndex;
            public bool modStatus;
        }

        struct VehicleSetting
        {
            public Dictionary<int, int> Extras;
            public int Livery;
        }

        static Dictionary<string, VehicleSetting> VehicleSettings = new Dictionary<string, VehicleSetting>();
        static List<ModSetting> ModSettings = new List<ModSetting>()
        {
            new ModSetting { modType = VehicleModType.Engine, modIndex = 2, modStatus = false },
            new ModSetting { modType = VehicleModType.Brakes, modIndex = 2, modStatus = false },
            new ModSetting { modType = VehicleModType.Transmission, modIndex = 2, modStatus = false },
            new ModSetting { modType = VehicleModType.Suspension, modIndex = 3, modStatus = false },
            new ModSetting { modType = VehicleModType.Armor, modIndex = 4, modStatus = false }
        };

        static public void Init()
        {
            Client.GetInstance().ClientCommands.Register("/sv", ServiceVehicle);

            VehicleSettings.Add("police", new VehicleSetting
            {
                Livery = 0,
                Extras = new Dictionary<int, int>() { [1] = -1, [2] = 0 }
            });

            VehicleSettings.Add("police2", new VehicleSetting
            {
                Livery = 2
            });
        }

        static private async void ServiceVehicle(Command command)
        {
            try
            {
                CitizenFX.Core.Model vehicleModel = null;
                string vehicleName = "";

                int result;
                if (Int32.TryParse(command.Args.Get(0), out result)) // if the argument is an int
                {
                    if (result < 1 || result > (PoliceVehicles.Count)) return;
                    vehicleName = PoliceVehicles[result-1];
                    vehicleModel = new Model(PoliceVehicles[result-1]);
                }
                else if (PoliceVehicles.Contains(command.Args.Get(0).ToLower()))
                {
                    vehicleName = command.Args.Get(0);
                    vehicleModel = new Model(command.Args.Get(0));
                }
                else
                {
                    return;
                }

                float heading = Game.PlayerPed.Heading + 90;
                if (heading > 180f) heading -= 360f;
                CitizenFX.Core.Vehicle vehicle = await World.CreateVehicle(vehicleModel, Game.PlayerPed.GetOffsetPosition(new Vector3(0, 2, 0)), heading);
                await BaseScript.Delay(50);
                vehicle.Mods.InstallModKit();
                ModSettings.ForEach(m => Function.Call(Hash.SET_VEHICLE_MOD, vehicle.Handle, (int)m.modType, m.modIndex, m.modStatus));
                if (VehicleSettings.ContainsKey(vehicleName))
                { 
                    vehicle.Mods.Livery = VehicleSettings[vehicleName].Livery;
                    if (VehicleSettings[vehicleName].Extras?.Count > 0)
                    {
                        VehicleSettings[vehicleName].Extras.ToList().ForEach(e =>
                        {
                            Function.Call(Hash.SET_VEHICLE_EXTRA, vehicle.Handle, e.Key, e.Value);

                        });
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}