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
    static class VehicleWindowManager
    {
        static MenuModel WindowMenu;
        static List<MenuItem> _menuItems = new List<MenuItem>();

        static List<VehicleWindowIndex> VehicleWindowValues = Enum.GetValues(typeof(VehicleWindowIndex)).OfType<VehicleWindowIndex>().Where(w => (int)w < 4).ToList();
        static List<string> VehicleWindowNames = VehicleWindowValues.Select(d => d.ToString().AddSpacesToCamelCase()).ToList();
        static Dictionary<VehicleWindowIndex, bool> windowStates;

        public static void Init()
        {
            WindowMenu = new MenuModel { options = new MenuOptions { HeaderBackgroundColor = Color.FromArgb(140, 200, 200) }, headerTitle = "Vehicle Window Menu" };
            InteractionListMenu.RegisterInteractionMenuItem(new MenuItemSubMenu { SubMenu = WindowMenu, Title = "Vehicle Window Menu" }, () => Game.PlayerPed.IsInVehicle(), 500);
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        private static Task OnTick()
        {
            try
            {
                if (InteractionListMenu.Observer.CurrentMenu == WindowMenu)
                {
                    if (!Game.PlayerPed.IsInVehicle())
                    {
                        vehicle = null;
                        InteractionListMenu.Observer.CloseMenu();
                    }
                    else
                    { 
                        RefreshMenuItems();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"VehicleWindowManager OnTick Error: {ex.Message}");
            }
            return Task.FromResult(0);
        }

        static CitizenFX.Core.Vehicle vehicle = null;

        private static void RefreshMenuItems()
        {
            try
            {
                if (vehicle != Game.PlayerPed.CurrentVehicle)
                { 
                    windowStates = VehicleWindowValues.ToDictionary(v => v, v => false);
                    vehicle = Game.PlayerPed.CurrentVehicle;
                }
                _menuItems.Clear();
                VehicleWindowValues.Select((window, index) => new { window, index }).ToList().ForEach(o =>
                {
                    var window = vehicle.Windows[o.window];
                    //if (window.IsIntact)
                    _menuItems.Add(new MenuItemCheckbox { Title = $"Roll Down {window.Index.ToString().AddSpacesToCamelCase()}", state = windowStates[window.Index], OnActivate = (state, item) => { if (state) window.RollDown(); else window.RollUp(); windowStates[window.Index] = state; } });
                });
                WindowMenu.menuItems = _menuItems;
                WindowMenu.SelectedIndex = WindowMenu.SelectedIndex;
            }
            catch (Exception ex)
            {
                Log.Error($"VehicleWindowManager OnTick Error: {ex.Message}");
            }
        }
    }
}
