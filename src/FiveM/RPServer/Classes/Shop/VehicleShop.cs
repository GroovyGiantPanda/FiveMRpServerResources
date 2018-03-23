using FamilyRP.Roleplay.Enums.Item;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Server.Classes.Shop
{
    public static class VehicleShop
    {
        private static Dictionary<string, ItemsModel> vehicles = new Dictionary<string, ItemsModel>();//SharedModels.ItemsListModel vehicles = null;

        static VehicleShop()
        {
            try
            {
                Log.Info("Loading Vehicles");
                var vehs = BaseItemShop.ItemByType(ItemType.Vehicle).Result;
                foreach (var veh in vehs.ItemList)
                {
                    if (!vehicles.ContainsKey(veh.ItemKey))
                    {
                        vehicles.Add(veh.ItemKey, veh);
                    }
                }
                Log.Info("Vehicles Loaded");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public static ItemsModel[] GetAllVehicles()
        {
            return vehicles.Values.ToArray();
        }

        public static ItemsModel GetVehicleByName(string itemKey)
        {
            if (vehicles.ContainsKey(itemKey))
            {
                return vehicles[itemKey];
            }
            else
            {
                return null;
            }
        }
    }
}
