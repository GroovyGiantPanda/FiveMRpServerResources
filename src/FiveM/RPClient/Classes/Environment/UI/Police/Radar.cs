using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Common;
using FamilyRP.Roleplay.Enums.Controls;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyRP.Roleplay.Client.Helpers;

namespace FamilyRP.Roleplay.Client.Classes.Environment.UI.Police
{
    static class Radar
    {
        // TODO: implement check
        static bool isCop = true;
        static public bool enabled = false;
        static public bool locked = false;

        static bool initialized = false;
        static string[] unitsMap = new string[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };

        static string modelName;
        static string modelClassName;
        static string vehicleColor;
        static int vehicleNumOccupants;
        static string vehiclePlate;
        static float vehicleSpeed;
        static MenuItem menuItemRadar = new MenuItemCheckbox
        {
            Title = "Enable Radar",
            OnActivate = (state, item) =>
            {
                item.state = state;
                Toggle(new Command("/radar")); // Ugly temporary hack
            }
        };

        static MenuItem menuItemLock = new MenuItemCheckbox
        {
            Title = "Lock Radar",
            state = locked,
            OnActivate = (state, item) =>
            {
                item.state = locked = state;
            }
        };

        static public void Init()
        {
            if (isCop)
            {
                Client.GetInstance().ClientCommands.Register("/radar", Toggle);
                InteractionListMenu.RegisterInteractionMenuItem(menuItemRadar, () => Game.PlayerPed.IsInVehicle() /*CurrentPlayer.CharacterData.Duty.HasFlag(Enums.Character.Duty.Police)*/, 1012);
                InteractionListMenu.RegisterInteractionMenuItem(menuItemLock, () => enabled && Game.PlayerPed.IsInVehicle() /*CurrentPlayer.CharacterData.Duty.HasFlag(Enums.Character.Duty.Police)*/, 1011);
            }
        }

        static internal void Toggle(Command command)
        {
            if (command.Args.Count == 0)
            {
                enabled = !enabled;
                (menuItemRadar as MenuItemCheckbox).state = enabled;
            }
            else
            {
                bool toggle = command.Args.GetBool(0);
                if (enabled == toggle) return;
            }
            if (enabled == true)
            {
                Log.ToChat("Radar on");
                Client.GetInstance().RegisterTickHandler(Draw);
            }
            else
            {
                Log.ToChat("Radar off");
                Client.GetInstance().DeregisterTickHandler(Draw);
            }
            (menuItemRadar as MenuItemCheckbox).state = enabled;
        }

        static public async Task Draw()
        {
            if (CinematicMode.DoHideHud) return;
            if (CitizenFX.Core.Game.PlayerPed.IsInVehicle())
            {
                if (ControlHelper.IsControlJustPressed(Control.MpTextChatTeam, true, ControlModifier.Ctrl))
                {
                    locked = !locked;
                    (menuItemLock as MenuItemCheckbox).state = locked;
                }
                System.Drawing.Color textColor = System.Drawing.Color.FromArgb(160, 255, 255, 255);
                CitizenFX.Core.Vehicle vehicle = WorldProbe.GetVehicleInFrontOfPlayer(60.0f);
                if (!ReferenceEquals(vehicle, null) && !locked)
                {
                    textColor = System.Drawing.Color.FromArgb(255, 255, 255, 255);
                    modelName = Game.GetGXTEntry(Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, vehicle.Model));
                    modelClassName = Game.GetGXTEntry(CitizenFX.Core.Vehicle.GetClassDisplayName((VehicleClass)Function.Call<int>(Hash.GET_VEHICLE_CLASS, vehicle.Handle)));
                    vehiclePlate = Function.Call<string>(Hash.GET_VEHICLE_NUMBER_PLATE_TEXT, vehicle.Handle);
                    vehicleColor = Enum.GetName(typeof(VehicleColor), vehicle.Mods.PrimaryColor).AddSpacesToCamelCase();
                    vehicleNumOccupants = Function.Call<int>(Hash.GET_VEHICLE_NUMBER_OF_PASSENGERS, vehicle.Handle) + (Function.Call<bool>(Hash.IS_VEHICLE_SEAT_FREE, vehicle.Handle, -1) ? 0 : 1);
                    vehicleSpeed = vehicle.Speed;
                    initialized = true;
                }

                if (initialized)
                {
                    float x = 0.18f;
                    float y = 0.79f;
                    FamilyRP.Roleplay.Client.UI.DrawText($"Speed: {2.24 * vehicleSpeed:0.0} mph / {3.60 * vehicleSpeed:0.0} kph", new Vector2(x, y), textColor, 0.25f);
                    y += 0.02f;
                    FamilyRP.Roleplay.Client.UI.DrawText($"Model: {modelName} ({modelClassName})", new Vector2(x, y), textColor, 0.25f);
                    y += 0.02f;
                    FamilyRP.Roleplay.Client.UI.DrawText($"Color: {vehicleColor}", new Vector2(x, y), textColor, 0.25f);
                    y += 0.02f;
                    FamilyRP.Roleplay.Client.UI.DrawText($"Plate: {vehiclePlate}", new Vector2(x, y), textColor, 0.25f);
                    y += 0.02f;
                    FamilyRP.Roleplay.Client.UI.DrawText($"{unitsMap[vehicleNumOccupants].FirstLetterToUpper()} occupant{(vehicleNumOccupants != 1 ? "s" : "")}", new Vector2(x, y), textColor, 0.25f);
                    y += 0.02f;
                    if (locked)
                        FamilyRP.Roleplay.Client.UI.DrawText($"RADAR LOCK", new Vector2(x, y), Color.FromArgb(255, 180, 180, 0), 0.25f);
                }
                else
                {
                    float x = 0.18f;
                    float y = 0.79f;
                    FamilyRP.Roleplay.Client.UI.DrawText($"RADAR ACTIVE", new Vector2(x, y), textColor, 0.25f);
                }
            }
            await Task.FromResult(0);
        }
    }
}
