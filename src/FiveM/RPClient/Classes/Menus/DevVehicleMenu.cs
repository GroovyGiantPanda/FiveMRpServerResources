using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Common;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client
{
    static class VehicleMenuTest
    {
        public static MenuModel VehicleMenu;

        // Vehicle models
        // Filtering vehicle hashes to contain only the types of models we want (no damn train models etc.)
        static List<int> VehicleHashFilteredIndices = new List<int>();

        static List<VehicleHash> VehicleHashValues = Enum.GetValues(typeof(VehicleHash)).OfType<VehicleHash>().Select((hash, index) => new { hash, index }).Where(h => { Model t = new Model(h.hash); if (t.IsCar || t.IsBike || t.IsBicycle ||t.IsQuadbike) return true; else return false; }).Select(i => { VehicleHashFilteredIndices.Add(i.index); return i.hash; }).ToList();
        static List<string> VehicleHashNames = Enum.GetNames(typeof(VehicleHash)).Select(c => c.AddSpacesToCamelCase()).Select((name, index) => new { name, index }).Where(i => VehicleHashFilteredIndices.Contains(i.index)).Select(i => i.name).ToList();

        // Vehicle colors
        static List<VehicleColor> VehicleColorValues = Enum.GetValues(typeof(VehicleColor)).OfType<VehicleColor>().ToList();
        static List<string> VehicleColorNames = Enum.GetNames(typeof(VehicleColor)).Select(c => c.AddSpacesToCamelCase()).ToList();

        // Wheel types
        static List<VehicleWheelType> VehicleWheelTypeValues = Enum.GetValues(typeof(VehicleWheelType)).OfType<VehicleWheelType>().ToList();
        static List<string> VehicleWheelTypeNames = Enum.GetNames(typeof(VehicleWheelType)).Select(c => c.AddSpacesToCamelCase()).ToList();

        static int currentlySelectedVehicleOnFoot = 0;

        class VehicleMenuModel : MenuModel
        {
            public static async Task ReplaceCurrentVehicleByIndex(int index)
            {
                await new Model(VehicleHashValues[index]).Request(10000);

                if(Game.PlayerPed.IsInVehicle())
                { 
                    Game.PlayerPed.CurrentVehicle.Delete();
                }

                Game.PlayerPed.Task.WarpIntoVehicle(await World.CreateVehicle(VehicleHashValues[index], Game.PlayerPed.Position, Game.PlayerPed.Heading), VehicleSeat.Driver);
            }
            public static void SetVehicleWheelTypeByIndex(int index)
            {
                Game.PlayerPed.CurrentVehicle.Mods.WheelType = VehicleWheelTypeValues[index];
            }
            public static Task SetColorByIndex(int index, int layer)
            { 
                if(layer == 0)
                {
                    Game.PlayerPed.CurrentVehicle.Mods.PrimaryColor = VehicleColorValues[index];
                }
                else if (layer == 1)
                {
                    Game.PlayerPed.CurrentVehicle.Mods.SecondaryColor = VehicleColorValues[index];
                }
                else if (layer == 2)
                {
                    Game.PlayerPed.CurrentVehicle.Mods.PearlescentColor = VehicleColorValues[index];
                }
                else if (layer == 3)
                {
                    Game.PlayerPed.CurrentVehicle.Mods.RimColor = VehicleColorValues[index];
                }

                return Task.FromResult(0);
            }

            Vehicle vehicle = null;

            // TODO: Only check what's valid for a vehicle model once (i.e. save into a Dictionary with Model key)
            public override void Refresh()
            {
                var _menuItems = new List<MenuItem>();
                if (Game.PlayerPed.Exists() && Game.PlayerPed.IsInVehicle())
                {
                    if (vehicle != Game.PlayerPed.CurrentVehicle)
                    {
                        vehicle = Game.PlayerPed.CurrentVehicle;
                        vehicle.Mods.InstallModKit();
                    }
                    try
                    {
                        int selectedVehicleIndex = VehicleHashValues.Contains((VehicleHash)vehicle.Model.Hash) ? VehicleHashValues.IndexOf((VehicleHash)vehicle.Model.Hash) : VehicleHashValues.IndexOf(VehicleHash.Monster);
                        VehicleColor primaryColor = vehicle.Mods.PrimaryColor;
                        VehicleColor secondaryColor = vehicle.Mods.SecondaryColor;
                        VehicleColor pearlescentColor = vehicle.Mods.PearlescentColor;
                        VehicleColor RimColor = vehicle.Mods.RimColor;
                        VehicleWheelType WheelType = (VehicleWheelType)Function.Call<int>(Hash.GET_VEHICLE_WHEEL_TYPE, vehicle.Handle);
                        int LiveryCount = vehicle.Mods.LiveryCount;
                        int Livery = vehicle.Mods.Livery;

                        // R/G/B selection?
                        //System.Drawing.Color NeonLightsColor = vehicle.Mods.NeonLightsColor;

                        // Do we even want this one?
                        //LicensePlateStyle LicensePlateStyle = vehicle.Mods.LicensePlateStyle;

                        // I have gotten several requests not to implement tire smoke (realism level -150)

                        VehicleMod[] AllMods = vehicle.Mods.GetAllMods();
                        _menuItems.Add(new MenuItemHorNamedSelector
                        {
                            Title = $"Vehicle",
                            Description = "Activate to replace your current vehicle.",
                            state = menuItems[0] is MenuItemHorNamedSelector ? (menuItems[0] as MenuItemHorNamedSelector).state : selectedVehicleIndex,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            optionList = VehicleHashNames,
                            OnActivate = new Action<int, string, MenuItemHorNamedSelector>(async (selectedAlternative, selName, menuItem) => { await ReplaceCurrentVehicleByIndex(selectedAlternative); })
                        });
                        if (LiveryCount != -1)
                        {
                            if (Livery == -1) vehicle.Mods.Livery = 1;
                            _menuItems.Add(new MenuItemHorSelector<int>
                            {
                                Title = $@"Livery",
                                state = menuItems.Count > 1 ? ((MenuItemHorSelector<int>)menuItems[1]).state : 1,
                                Type = MenuItemHorizontalSelectorType.NumberAndBar,
                                wrapAround = true,
                                minState = 1,
                                maxState = LiveryCount,
                                overrideDetailWith = vehicle.Mods.LocalizedLiveryName != "" ? $"{vehicle.Mods.LocalizedLiveryName}" : "",
                                OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { vehicle.Mods.Livery = selectedAlternative; })
                            });
                        }
                        _menuItems.Add(new MenuItemHorNamedSelector
                        {
                            Title = $"Primary Color",
                            state = VehicleColorValues.Contains(primaryColor) ? VehicleColorValues.IndexOf(primaryColor) : 0,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            optionList = VehicleColorNames,
                            OnChange = new Action<int, string, MenuItemHorNamedSelector>(async (selectedAlternative, selName, menuItem) => { await SetColorByIndex(selectedAlternative, 0); })
                        });
                        _menuItems.Add(new MenuItemHorNamedSelector
                        {
                            Title = $"Secondary Color",
                            state = VehicleColorValues.Contains(secondaryColor) ? VehicleColorValues.IndexOf(secondaryColor) : 0,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            optionList = VehicleColorNames,
                            OnChange = new Action<int, string, MenuItemHorNamedSelector>(async (selectedAlternative, selName, menuItem) => { await SetColorByIndex(selectedAlternative, 1); })
                        });
                        _menuItems.Add(new MenuItemHorNamedSelector
                        {
                            Title = $"Pearlescent Color",
                            state = VehicleColorValues.Contains(pearlescentColor) ? VehicleColorValues.IndexOf(pearlescentColor) : 0,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            optionList = VehicleColorNames,
                            OnChange = new Action<int, string, MenuItemHorNamedSelector>(async (selectedAlternative, selName, menuItem) => { await SetColorByIndex(selectedAlternative, 2); })
                        });
                        _menuItems.Add(new MenuItemHorNamedSelector
                        {
                            Title = $"Rim Color",
                            state = VehicleColorValues.Contains(RimColor) ? VehicleColorValues.IndexOf(RimColor) : 0,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            optionList = VehicleColorNames,
                            OnChange = new Action<int, string, MenuItemHorNamedSelector>(async (selectedAlternative, selName, menuItem) => { await SetColorByIndex(selectedAlternative, 3); })
                        });
                        var allowedWheelTypes = vehicle.Mods.AllowedWheelTypes.ToList();
                        if(allowedWheelTypes.Contains(VehicleWheelType.HighEnd)) allowedWheelTypes.Remove(VehicleWheelType.HighEnd); // Removing for now, seems to not want to set this
                        // Wheel Types not a thing for bikes I believe; real and front wheel already a mod category
                        if (vehicle.Model.IsCar && allowedWheelTypes.Count() > 0)
                        {
                            _menuItems.Add(new MenuItemHorNamedSelector
                            {
                                Title = $"Wheel Type",
                                state = allowedWheelTypes.ToList().Contains(WheelType) ? (int)WheelType : (int)allowedWheelTypes[0],
                                Type = MenuItemHorizontalSelectorType.NumberAndBar,
                                wrapAround = true,
                                optionList = allowedWheelTypes.Select(i => $"{i}").ToList(),
                                OnChange = new Action<int, string, MenuItemHorNamedSelector>((selectedAlternative, selName, menuItem) => { vehicle.Mods.WheelType = (VehicleWheelType)selectedAlternative; vehicle.Mods[VehicleModType.FrontWheel].Index = 0; })
                            });
                        }
                        AllMods.ToList().ForEach(m =>
                        {
                            try
                            { 
                                _menuItems.Add(new MenuItemHorSelector<int>
                                {
                                    Title = $@"{m.LocalizedModTypeName}",
                                    state = (m.Index >= -1 && m.Index <= m.ModCount - 1) ? m.Index : 0,
                                    Type = MenuItemHorizontalSelectorType.NumberAndBar,
                                    wrapAround = true,
                                    minState = -1,
                                    maxState = m.ModCount - 1,
                                    overrideDetailWith = m.LocalizedModName,
                                    OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { m.Index = selectedAlternative; })
                                });
                                if (m.ModType == VehicleModType.FrontWheel)
                                {
                                    _menuItems.Add(new MenuItemCheckbox
                                    {
                                        Title = $"Special Tire Variation",
                                        Description = "This will only work for some wheels.",
                                        state = m.Variation,
                                        OnActivate = new Action<bool, MenuItemCheckbox>((selectedAlternative, menuItem) => { m.Variation = selectedAlternative; })
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"[VEHICLETESTMENU] Exception in vehicle mods code; {ex.Message}");
                            }
                        });

                        // Yes, there are this many extra indices
                        // TODO: Save these for each vehicle after iterating once; this is the first iteration
                        // No performance hit on my own PC though
                        Enumerable.Range(0, 50).ToList().ForEach(i =>
                        {
                            try
                            { 
                                if (Function.Call<bool>(Hash.DOES_EXTRA_EXIST, vehicle.Handle, i))
                                {
                                    _menuItems.Add(new MenuItemCheckbox
                                    {
                                        Title = $"Extra #{i + 1}",
                                        state = Function.Call<bool>(Hash.IS_VEHICLE_EXTRA_TURNED_ON, vehicle.Handle, i),
                                        OnActivate = new Action<bool, MenuItemCheckbox>((selectedAlternative, menuItem) => { Function.Call(Hash.SET_VEHICLE_EXTRA, vehicle.Handle, i, selectedAlternative ? 0 : -1); })
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"[VEHICLETESTMENU] Exception in extras code; {ex.Message}");
                            }
                        });

                        visibleItems = _menuItems.Slice(visibleItems.IndexOf(menuItems.First()), visibleItems.IndexOf(menuItems.Last()));
                        menuItems = _menuItems;
                        SelectedIndex = SelectedIndex; // refreshes state
                    }
                    catch(Exception ex)
                    {
                        Log.Error($"[VEHICLETESTMENU] Outer exception {ex.Message}");
                    }
                }
                else
                {
                    if (!currentlySelectedVehicleOnFoot.IsBetween(0, VehicleHashValues.Count-1)) currentlySelectedVehicleOnFoot = 0;
                    _menuItems.Add(new MenuItemHorNamedSelector
                    {
                        Title = $"Spawn Vehicle",
                        Description = "Activate to spawn vehicle.",
                        state = currentlySelectedVehicleOnFoot,
                        Type = MenuItemHorizontalSelectorType.NumberAndBar,
                        wrapAround = true,
                        optionList = VehicleHashNames,
                        OnChange = new Action<int, string, MenuItemHorNamedSelector>((selectedAlternative, selName, menuItem) => { currentlySelectedVehicleOnFoot = selectedAlternative; }),
                        OnActivate = new Action<int, string, MenuItemHorNamedSelector>(async (selectedAlternative, selName, menuItem) => { await ReplaceCurrentVehicleByIndex(selectedAlternative); })
                    });
                    menuItems = _menuItems;
                    SelectedIndex = 0; // refreshes state
                }
            }
        }

        public static void Init()
        {
            VehicleMenu = new VehicleMenuModel { numVisibleItems = 7/*, options = new MenuOptions { StatusBarBottomMargin = 6, ItemBottomMargin = 6 }*/  }; // Comment out that if we want some spacing between items
            VehicleMenu.headerTitle = "Vehicle Customization";
            VehicleMenu.statusTitle = "";
            VehicleMenu.menuItems = new List<MenuItem>() { new MenuItemStandard { Title = "Populating menu..." } }; // Currently we need at least one item in a menu; could make it work differently, but eh.
            //MenuManager.TestMenu.menuItems.Insert(0, new MenuItemSubMenu { Title = $"Vehicle Customization", SubMenu = VehicleMenu });
            InteractionListMenu.RegisterInteractionMenuItem(new MenuItemSubMenu
            {
                Title = $"[DEV] Spawn/Customize Vehicle",
                SubMenu = VehicleMenu
            }, () => true, 1000);
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        private static Task OnTick()
        {
            try
            {
                if (InteractionListMenu.Observer.CurrentMenu == VehicleMenu)
                {
                    VehicleMenu.Refresh();
                }
            }
            catch(Exception ex)
            {
                Log.Error($"[VEHICLETESTMENU] Exception in OnTick; {ex.Message}");
            }

            return Task.FromResult(0);
        }
    }
}
