using CitizenFX.Core;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Common;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Vehicle
{
    static class DoorManager
    {
        static MenuModel DoorMenu;
        static List<MenuItem> _menuItems = new List<MenuItem>();

        static List<VehicleDoorIndex> VehicleDoorValues = Enum.GetValues(typeof(VehicleDoorIndex)).OfType<VehicleDoorIndex>().ToList();
        static List<string> VehicleDoorNames = Enum.GetNames(typeof(VehicleDoorIndex)).Select(d => d.AddSpacesToCamelCase()).ToList();

        public static void Init()
        {
            DoorMenu = new MenuModel { options = new MenuOptions { HeaderBackgroundColor = Color.FromArgb(140, 200, 200) }, headerTitle = "Vehicle Door Menu" };
            InteractionListMenu.RegisterInteractionMenuItem(new MenuItemSubMenu { SubMenu = DoorMenu, Title = "Vehicle Door Menu" }, () => Game.PlayerPed.IsInVehicle(), 500);
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        private static Task OnTick()
        {
            try
            {
                if(InteractionListMenu.Observer.CurrentMenu == DoorMenu)
                {
                    if (!Game.PlayerPed.IsInVehicle())
                    {
                        InteractionListMenu.Observer.CloseMenu();
                    }
                    else
                    {
                        RefreshMenuItems();
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error($"DoorManager OnTick Error: {ex.Message}");
            }
            return Task.FromResult(0);
        }

        private static void RefreshMenuItems()
        {
            try
            { 
                _menuItems.Clear();
                CitizenFX.Core.Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
                VehicleDoor[] doors = vehicle.Doors.GetAll();
                doors.ToList().ForEach(door =>
                {
                    if (!door.IsBroken)
                        _menuItems.Add(new MenuItemCheckbox { Title = $"Open {door.Index.ToString().AddSpacesToCamelCase()}", state = door.IsOpen, OnActivate = (state, item) => { if (state) door.Open(); else door.Close(); } });
                });
                DoorMenu.menuItems = _menuItems;
                DoorMenu.SelectedIndex = DoorMenu.SelectedIndex;
            }
            catch (Exception ex)
            {
                Log.Error($"DoorManager OnTick Error: {ex.Message}");
            }
        }
    }
}
