using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Enums.Controls;

namespace FamilyRP.Roleplay.Client
{
    public class Controls
    {
        public static Dictionary<string, ControlSetting> Settings = new Dictionary<string, ControlSetting>()
        {
            ["VoipRange.Toggle"] = new ControlSetting(Control.HUDSpecial),
            ["Helicopter.ToggleVisionMode"] = new ControlSetting(Control.Context, true, ControlModifier.Ctrl),
            ["Helicopter.SwitchTarget"] = new ControlSetting(Control.VehicleFlyUnderCarriage, true, ControlModifier.Ctrl),
            ["Helicopter.ToggleZoom"] = new ControlSetting(Control.VehicleFlyUnderCarriage, true, ControlModifier.None),
            ["Helicopter.ToggleCamera"] = new ControlSetting(Control.VehicleFlyUnderCarriage, true, ControlModifier.Shift),
            ["Helicopter.Rappel"] = new ControlSetting(Control.Context, true, ControlModifier.Shift),
            ["Helicopter.ToggleSearchLight"] = new ControlSetting(Control.VehicleHeadlight, true, ControlModifier.Ctrl),
            ["SpikeStrip.Toggle"] = new ControlSetting(Control.VehicleHeadlight, true, ControlModifier.Ctrl)
            // Move all control definitions in other classes here (but keep checks there)
        };

        public ControlSetting this[string action] => Settings[action];
    }

    public class ControlSetting
    {
        public Control Control { get; internal set; }
        public ControlModifier Modifier { get; internal set; }
        public bool KeyboardOnly { get; internal set; }

        public ControlSetting(Control control, bool keyboardOnly = true, ControlModifier modifier = ControlModifier.None)
        {
            this.Control = control;
            this.Modifier = modifier;
            this.KeyboardOnly = keyboardOnly;
        }
    }

    /// <summary>
    /// Creates Controls singleton
    /// Singleton required to make simple Settings.Controls["Helicopter.Rappel"] syntax possible (only available for non-static classes)
    /// </summary>
    public class Settings
    {
        static Controls _controls = new Controls();
        public static Controls Controls
        {
            get
            {
                return _controls;
            }
        }
    }
}
