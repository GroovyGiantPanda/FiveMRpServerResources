using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;
using FamilyRP.Roleplay.Client.Classes.Jobs.Police.Vehicle;
using FamilyRP.Common;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client
{
    static class PoliceVehicleMenu
    {
        public static MenuModel Menu;
        public static Vehicle serviceVehicle;

        // Vehicle models
        static List<VehicleHash> VehicleHashValues;
        static List<string> VehicleHashNames;

        // Vehicle colors
        static List<VehicleColor> VehicleColorValues;
        static List<string> VehicleColorNames;

        // Wheel types
        static List<VehicleWheelType> VehicleWheelTypeValues;
        static List<string> VehicleWheelTypeNames;

        static int currentlySelectedVehicleOnFoot;

        class PoliceVehicleMenuModel : MenuModel
        {
            public static async Task ReplaceCurrentVehicleByIndex(int index)
            {
                await new Model(VehicleHashValues[index]).Request(10000);
                if (PoliceVehicleMenu.serviceVehicle != null)
                {
                    PoliceVehicleMenu.serviceVehicle.Delete();
                    serviceVehicle = null;
                }
                var v = await World.CreateVehicle(new Model(VehicleHashValues[index]), Game.PlayerPed.GetOffsetPosition(new Vector3(0, 3, 0)));
                serviceVehicle = v;
            }
            public static void SetVehicleWheelTypeByIndex(int index)
            {
                PoliceVehicleMenu.serviceVehicle.Mods.WheelType = VehicleWheelTypeValues[index];
            }
            public static Task SetColorByIndex(int index, int layer)
            {
                if (layer == 0)
                {
                    PoliceVehicleMenu.serviceVehicle.Mods.PrimaryColor = VehicleColorValues[index];
                }
                else if (layer == 1)
                {
                    PoliceVehicleMenu.serviceVehicle.Mods.SecondaryColor = VehicleColorValues[index];
                }
                else if (layer == 2)
                {
                    PoliceVehicleMenu.serviceVehicle.Mods.PearlescentColor = VehicleColorValues[index];
                }
                else if (layer == 3)
                {
                    PoliceVehicleMenu.serviceVehicle.Mods.RimColor = VehicleColorValues[index];
                }

                return Task.FromResult(0);
            }
            
            List<MenuItem> _menuItems = new List<MenuItem>();
            int selectedVehicleIndex = 0;
            int selectedLivery = 1;
            // TODO: Only check what's valid for a vehicle model once (i.e. save into a Dictionary with Model key)
            public override void Refresh()
            {
                _menuItems.Clear();
                if (serviceVehicle != null)
                {
                    try
                    {
                        //Log.Debug($"X {VehicleHashNames.Count()}");
                        VehicleColor primaryColor = serviceVehicle.Mods.PrimaryColor;
                        VehicleColor secondaryColor = serviceVehicle.Mods.SecondaryColor;
                        VehicleColor pearlescentColor = serviceVehicle.Mods.PearlescentColor;
                        VehicleColor RimColor = serviceVehicle.Mods.RimColor;
                        VehicleWheelType WheelType = (VehicleWheelType)Function.Call<int>(Hash.GET_VEHICLE_WHEEL_TYPE, serviceVehicle.Handle);
                        int LiveryCount = serviceVehicle.Mods.LiveryCount;
                        int Livery = serviceVehicle.Mods.Livery;

                        // R/G/B selection?
                        //System.Drawing.Color NeonLightsColor = vehicle.Mods.NeonLightsColor;

                        // Do we even want this one?
                        //LicensePlateStyle LicensePlateStyle = vehicle.Mods.LicensePlateStyle;

                        // I have gotten several requests not to implement tire smoke (realism level -150)
                        //Log.Debug($"{selectedVehicleIndex}");
                        VehicleMod[] AllMods = serviceVehicle.Mods.GetAllMods();
                        _menuItems.Add(new MenuItemHorNamedSelector
                        {
                            Title = "Vehicle",
                            Description = "Activate to replace your current vehicle.",
                            state = selectedVehicleIndex,
                            Type = MenuItemHorizontalSelectorType.NumberAndBar,
                            wrapAround = true,
                            optionList = VehicleHashNames,
                            OnChange = (selectedAlternative, selName, item) => { selectedVehicleIndex = (item as MenuItemHorNamedSelector).state; },
                            OnActivate = new Action<int, string, MenuItemHorNamedSelector>(async (selectedAlternative, selName, menuItem) => { await ReplaceCurrentVehicleByIndex(selectedAlternative); })
                        });
                        if (LiveryCount != -1)
                        {
                            if (Livery == -1) serviceVehicle.Mods.Livery = 1;
                            _menuItems.Add(new MenuItemHorSelector<int>
                            {
                                Title = $@"Livery",
                                state = selectedLivery,
                                Type = MenuItemHorizontalSelectorType.NumberAndBar,
                                wrapAround = true,
                                minState = 1,
                                maxState = LiveryCount,
                                overrideDetailWith = serviceVehicle.Mods.LocalizedLiveryName != "" ? $"{serviceVehicle.Mods.LocalizedLiveryName}" : "",
                                OnChange = new Action<int, MenuItemHorSelector<int>>((selectedAlternative, menuItem) => { serviceVehicle.Mods.Livery = selectedAlternative; selectedLivery = selectedAlternative; })
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
                        var allowedWheelTypes = serviceVehicle.Mods.AllowedWheelTypes.ToList();
                        if (allowedWheelTypes.Contains(VehicleWheelType.HighEnd)) allowedWheelTypes.Remove(VehicleWheelType.HighEnd); // Removing for now, seems to not want to set this
                        // Wheel Types not a thing for bikes I believe; real and front wheel already a mod category
                        if (serviceVehicle.Model.IsCar && allowedWheelTypes.Count() > 0)
                        {
                            _menuItems.Add(new MenuItemHorNamedSelector
                            {
                                Title = $"Wheel Type",
                                state = allowedWheelTypes.ToList().Contains(WheelType) ? (int)WheelType : (int)allowedWheelTypes[0],
                                Type = MenuItemHorizontalSelectorType.NumberAndBar,
                                wrapAround = true,
                                optionList = allowedWheelTypes.Select(i => $"{i}").ToList(),
                                OnChange = new Action<int, string, MenuItemHorNamedSelector>((selectedAlternative, selName, menuItem) => { serviceVehicle.Mods.WheelType = (VehicleWheelType)selectedAlternative; serviceVehicle.Mods[VehicleModType.FrontWheel].Index = 0; })
                            });
                        }
                        AllMods.Where(m => m.ModType != VehicleModType.Engine && m.ModType != VehicleModType.Transmission && m.ModType != VehicleModType.Brakes).ToList().ForEach(m =>
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
                                Log.Error($"[COPVEHICLEMENU] Exception in vehicle mods code; {ex.Message}");
                            }
                        });

                        // Yes, there are this many extra indices
                        // TODO: Save these for each vehicle after iterating once; this is the first iteration
                        // No performance hit on my own PC though
                        Enumerable.Range(0, 50).ToList().ForEach(i =>
                        {
                            try
                            {
                                if (Function.Call<bool>(Hash.DOES_EXTRA_EXIST, serviceVehicle.Handle, i))
                                {
                                    _menuItems.Add(new MenuItemCheckbox
                                    {
                                        Title = $"Extra #{i + 1}",
                                        state = Function.Call<bool>(Hash.IS_VEHICLE_EXTRA_TURNED_ON, serviceVehicle.Handle, i),
                                        OnActivate = new Action<bool, MenuItemCheckbox>((selectedAlternative, menuItem) => { Function.Call(Hash.SET_VEHICLE_EXTRA, serviceVehicle.Handle, i, selectedAlternative ? 0 : -1); })
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"[COPVEHICLEMENU] Exception in extras code; {ex.Message}");
                            }
                        });
                        int firstVisibleItem = visibleItems.IndexOf(menuItems.First());
                        int lastVisibleItem = visibleItems.IndexOf(menuItems.Last());
                        visibleItems = _menuItems.Slice(firstVisibleItem, lastVisibleItem);
                        //if (lastVisibleItem - firstVisibleItem >= _menuItems.Count)
                        //    visibleItems = _menuItems;
                        //else
                        //{
                        //    visibleItems = _menuItems.Slice(firstVisibleItem, firstVisibleItem + Math.Min(Math.Max(lastVisibleItem - firstVisibleItem, _menuItems.Count - 1 - firstVisibleItem), numVisibleItems));
                        //}

                        menuItems = _menuItems;
                        SelectedIndex = SelectedIndex; // refreshes state
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[COPVEHICLEMENU] Outer exception {ex.Message}");
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
                        OnChange = new Action<int, string, MenuItemHorNamedSelector>((selectedAlternative, selName, menuItem) => { selectedVehicleIndex = currentlySelectedVehicleOnFoot = selectedAlternative; }),
                        OnActivate = new Action<int, string, MenuItemHorNamedSelector>(async (selectedAlternative, selName, menuItem) => { await ReplaceCurrentVehicleByIndex(selectedAlternative); })
                    });
                    menuItems = _menuItems;
                    SelectedIndex = SelectedIndex; // refreshes state
                }
            }
        }

        public static void Init()
        {
            serviceVehicle = null;
            
            VehicleHashValues = VehicleLoadoutPresets.PoliceVehicles.Select(v => (VehicleHash)Game.GenerateHash(v)).ToList();
            VehicleHashNames = VehicleLoadoutPresets.PoliceVehicles.Select(v => v.ToTitleCase().AddSpacesToCamelCase()).ToList();
            VehicleColorValues = Enum.GetValues(typeof(VehicleColor)).OfType<VehicleColor>().ToList();
            VehicleColorNames = Enum.GetNames(typeof(VehicleColor)).Select(c => c.AddSpacesToCamelCase()).ToList();
            VehicleWheelTypeValues = Enum.GetValues(typeof(VehicleWheelType)).OfType<VehicleWheelType>().ToList();
            VehicleWheelTypeNames = Enum.GetNames(typeof(VehicleWheelType)).Select(c => c.AddSpacesToCamelCase()).ToList();
            currentlySelectedVehicleOnFoot = 0;

            Menu = new PoliceVehicleMenuModel { numVisibleItems = 7  };
            Menu.headerTitle = "Vehicle Customization";
            Menu.statusTitle = "";
            Menu.Refresh();
            //Menu.menuItems = new List<MenuItem>() { new MenuItemStandard { Title = "Populating menu..." } }; // Currently we need at least one item in a menu; could make it work differently, but eh.
            InteractionListMenu.RegisterInteractionMenuItem(new MenuItemSubMenu { Title = $"Spawn/Customize Vehicle", SubMenu = Menu }, () => true/*CurrentPlayer.CharacterData.Duty.HasFlag(Enums.Character.Duty.Police)*/, 1000);

            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        private static Task OnTick()
        {
            try
            {
                if (InteractionListMenu.Observer.CurrentMenu == Menu)
                {
                    Menu.Refresh();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[COPVEHICLEMENU] Exception in OnTick; {ex.Message}");
            }

            return Task.FromResult(0);
        }
    }
}
