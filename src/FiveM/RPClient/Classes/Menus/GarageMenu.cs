using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Roleplay.Client.Models;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;

namespace FamilyRP.Roleplay.Client
{
    internal static partial class Vehicles
    {
        private static MenuModel GarageMenu;
        private static MenuItemSubMenu VehicleSelectMenu;

        internal static void InitMenu()
        {
            Log.Verbose($"GarageMenu InitMenu");
            try
            {
                var item = new MenuItemStandard()
                {
                    Title = "Park Vehicle",
                    OnActivate = ParkCarFromMenu
                };
                InteractionListMenu.RegisterInteractionMenuItem(item, CheckPedGarageProximity, 500);

                // Vehicle Selection, requires sub menu
                var items = new List<MenuItemStandard>()
                {
                     new MenuItemStandard()
                     {
                         Title = "SomeCar",
                         OnActivate = TestEvent
                     }
                };
                VehicleSelectMenu = new MenuItemSubMenu()
                {
                    Title = "Vehicle Select",
                    SubMenu = new MenuModel()
                    {
                        headerTitle = "Some sub menu"
                    }
                };
                VehicleSelectMenu.SubMenu.MenuItems.AddRange(items);
                InteractionListMenu.RegisterInteractionMenuItem(VehicleSelectMenu, CheckPedGarageProximity, 500);
            }
            catch (Exception ex)
            {
                Log.Error($"GarageMenu InitMenu Error: {ex.Message}");
            }
        }
    }
}
