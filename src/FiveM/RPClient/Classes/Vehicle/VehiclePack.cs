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
    public class VehicleMemoryModel
    {
        public VehicleHash? Model { get; set; } = null;
        public int? Livery { get; set; } = null;
        public VehicleColor? PrimaryColor { get; set; } = null;
        public VehicleColor? SecondaryColor { get; set; } = null;
        public VehicleColor? PearlescentColor { get; set; } = null;
        public VehicleColor? RimColor { get; set; } = null;
        public VehicleWheelType? WheelType { get; set; } = null; // Only sets wheel category; actual wheel variation is in Mods below
        public Dictionary<VehicleModType, int> Mods { get; set; } = null;
        public Dictionary<int, int> ToggleMods { get; set; } = null; // Not currently used (also only has like 3 options apparently)
        public Dictionary<int, bool> Extras { get; set; } = null;
        public bool? CustomWheelVariation { get; set; } = null;
        public Dictionary<string, string> OtherMetaData { get; set; } = null;

        public Dictionary<int, VehicleColor> Underglow { get; set; } = null; // Not currently used
        public Dictionary<int, VehicleColor> NumberPlateLook { get; set; } = null; // Not currently used
    }

    public static class VehiclePack
    {
        public static VehicleMemoryModel Unpack(string serializedVehicle)
        {
            try
            {
                return Helpers.MsgPack.Deserialize<VehicleMemoryModel>(serializedVehicle);
            }
            catch (Exception ex)
            {
                Log.Error($"VehicleMemoryModel Unpack error: {ex.Message}");
            }
            return null;
        }

        public static async Task<CitizenFX.Core.Vehicle> UnpackToWorld(VehicleMemoryModel vehicleModel, Vector3 Position, float Heading)
        {
            try
            {
                CitizenFX.Core.Vehicle vehicle = await World.CreateVehicle(vehicleModel.Model, Position, Heading);
                if (vehicleModel.Livery != null) vehicle.Mods.Livery = (int)vehicleModel.Livery;
                if (vehicleModel.PrimaryColor != null) vehicle.Mods.PrimaryColor = (VehicleColor)vehicleModel.PrimaryColor;
                if (vehicleModel.SecondaryColor != null) vehicle.Mods.SecondaryColor = (VehicleColor)vehicleModel.SecondaryColor;
                if (vehicleModel.PearlescentColor != null) vehicle.Mods.PearlescentColor = (VehicleColor)vehicleModel.PearlescentColor;
                if (vehicleModel.RimColor != null) vehicle.Mods.RimColor = (VehicleColor)vehicleModel.RimColor;
                if (vehicleModel.WheelType != null) vehicle.Mods.WheelType = (VehicleWheelType)vehicleModel.WheelType;
                if (vehicleModel.Mods != null)
                {
                    vehicleModel.Mods.ToList().ForEach(m =>
                    {
                        vehicle.Mods[m.Key].Index = m.Value;
                    });
                }
                if (vehicleModel.CustomWheelVariation != null) Function.Call(Hash.SET_VEHICLE_EXTRA, vehicle.Handle, VehicleModType.FrontWheel, (bool)(vehicleModel.CustomWheelVariation) ? 0 : -1);
                if (vehicleModel.Extras != null)
                {
                    vehicleModel.Extras.ToList().ForEach(e =>
                    {
                        Function.Call(Hash.SET_VEHICLE_EXTRA, vehicle.Handle, e.Key, e.Value ? 0 : -1);
                    });
                }
                //if (vehicleModel.ToggleMods != null) // Only includes like 3 mods; Xenon etc
                //if (vehicleModel.UnderGlow != null) // Not currently used
                //if (vehicleModel.NumberPlateStyle != null) // Not currently used

                return vehicle;
            }
            catch (Exception ex)
            {
                Log.Error($"VehicleMemoryModel UnpackToWorld error: {ex.Message}");
            }
            return null;
        }

        public static string PackFromWorld(CitizenFX.Core.Vehicle vehicle)
        {
            try
            {
                var data = new VehicleMemoryModel();
                data.Livery = (int)vehicle.Mods.Livery;
                data.PrimaryColor = vehicle.Mods.PrimaryColor;
                data.SecondaryColor = vehicle.Mods.SecondaryColor;
                data.PearlescentColor = vehicle.Mods.PearlescentColor;
                data.RimColor = vehicle.Mods.RimColor;
                data.WheelType = vehicle.Mods.WheelType;
                foreach(var mod in vehicle.Mods.GetAllMods())
                {
                    if (!data.Mods.ContainsKey(mod.ModType))
                    {
                        data.Mods.Add(mod.ModType, mod.Index);
                    }
                }
                data.CustomWheelVariation = API.IsVehicleExtraTurnedOn(vehicle.Handle, (int)VehicleModType.FrontWheel);
                Enumerable.Range(0, 50).ToList().ForEach(e => { if (API.IsVehicleExtraTurnedOn(vehicle.Handle, e)) data.Extras[e] = true; });

                return Pack(data);
            }
            catch (Exception ex)
            {
                Log.Error($"VehiclePack PackVehicle Error: {ex.Message}");
                return null;
            }
        }

        public static string Pack(VehicleMemoryModel vehicleModel)
        {
            try
            {
                return Helpers.MsgPack.Serialize(vehicleModel);
            }
            catch (Exception ex)
            {
                Log.Error($"VehicleMemoryModel Pack error: {ex.Message}");
            }
            return null;
        }
    }
}
