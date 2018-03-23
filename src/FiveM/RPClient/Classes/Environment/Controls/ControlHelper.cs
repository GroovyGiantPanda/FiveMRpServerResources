using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyRP.Roleplay.Enums.Controls;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.Helpers;
using FamilyRP.Roleplay.Models;
using CitizenFX.Core;

namespace FamilyRP.Roleplay.Client
{
    /// <summary>
    /// Some control keycodes to get you started: https://wiki.fivem.net/wiki/Controls
    /// Alternatively, type /dev keycodetester in-game and press a key you wish to use
    /// Be way control codes can be bound to multiple keys as well
    /// (I.e. there are cross-binds in all directions)
    /// </summary>
    static class ControlHelper
    {
        // To be moved to constants
        const int defaultControlGroup = 0;
        const int controllerControlGroup = 2;
        public static Dictionary<ControlModifier, int> ModifierFlagToKeyCode = new Dictionary<ControlModifier, int>()
        {
            [ControlModifier.Ctrl] = 36,
            [ControlModifier.Alt] = 19,
            [ControlModifier.Shift] = 21
        };

        static ControlHelper()
        {
        }

        public static bool WasLastInputFromController()
        {
            return !NativeWrappers.IsInputDisabled(controllerControlGroup);
        }

        /// <summary>
        /// Lets you know if the specified modifier is pressed
        /// </summary>
        /// <param name="modifier">You can either specify just one Modifier or combine multiple (with |)</param>
        /// <returns></returns>
        public static bool IsControlModifierPressed(ControlModifier modifier)
        {
            if (modifier == ControlModifier.Any)
            { 
                return true;
            }
            else
            {
                ControlModifier BitMask = 0;
                ModifierFlagToKeyCode.ToList().ForEach(w =>
                {
                    if(Game.IsControlPressed(defaultControlGroup, (Control)w.Value))
                    {
                        BitMask = BitMask | w.Key;
                    }
                });
                if(BitMask == modifier)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool IsControlJustPressed(Control control, bool keyboardOnly = true, ControlModifier modifier = ControlModifier.None)
        {
            return Game.IsControlJustPressed(0, control) && (!keyboardOnly || (keyboardOnly && !WasLastInputFromController())) && IsControlModifierPressed(modifier);
        }

        public static bool IsControlPressed(Control control, bool keyboardOnly = true, ControlModifier modifier = ControlModifier.None)
        {
            return Game.IsControlPressed(0, control) && (!keyboardOnly || (keyboardOnly && !WasLastInputFromController())) && IsControlModifierPressed(modifier);
        }

        public static bool IsControlJustReleased(Control control, bool keyboardOnly = true, ControlModifier modifier = ControlModifier.None)
        {
            return Game.IsControlJustReleased(0, control) && (!keyboardOnly || (keyboardOnly && !WasLastInputFromController())) && IsControlModifierPressed(modifier);
        }

        public static bool IsDisabledControlJustPressed(Control control, bool keyboardOnly = true, ControlModifier modifier = ControlModifier.None)
        {
            return Game.IsDisabledControlJustPressed(0, control) && (!keyboardOnly || (keyboardOnly && !WasLastInputFromController())) && IsControlModifierPressed(modifier);
        }

        public static bool IsDisabledControlJustReleased(Control control, bool keyboardOnly = true, ControlModifier modifier = ControlModifier.None)
        {
            return Game.IsDisabledControlJustReleased(0, control) && (!keyboardOnly || (keyboardOnly && !WasLastInputFromController())) && IsControlModifierPressed(modifier);
        }

        public static bool IsDisabledControlPressed(Control control, bool keyboardOnly = true, ControlModifier modifier = ControlModifier.None)
        {
            return Game.IsDisabledControlPressed(0, control) && (!keyboardOnly || (keyboardOnly && !WasLastInputFromController())) && IsControlModifierPressed(modifier);
        }

        public static bool IsEnabledControlJustPressed(Control control, bool keyboardOnly = true, ControlModifier modifier = ControlModifier.None)
        {
            return Game.IsEnabledControlJustPressed(0, control) && (!keyboardOnly || (keyboardOnly && !WasLastInputFromController())) && IsControlModifierPressed(modifier);
        }

        public static bool IsEnabledControlJustReleased(Control control, bool keyboardOnly = true, ControlModifier modifier = ControlModifier.None)
        {
            return Game.IsEnabledControlJustPressed(0, control) && (!keyboardOnly || (keyboardOnly && !WasLastInputFromController())) && IsControlModifierPressed(modifier);
        }

        public static bool IsEnabledControlPressed(Control control, bool keyboardOnly = true, ControlModifier modifier = ControlModifier.None)
        {
            return Game.IsEnabledControlPressed(0, control) && (!keyboardOnly || (keyboardOnly && !WasLastInputFromController())) && IsControlModifierPressed(modifier);
        }

        // Below: ControlSetting overloads for the above
        public static bool IsControlJustPressed(ControlSetting setting)
        {
            return IsControlJustPressed(setting.Control, setting.KeyboardOnly, setting.Modifier);
        }

        public static bool IsControlPressed(ControlSetting setting)
        {
            return IsControlPressed(setting.Control, setting.KeyboardOnly, setting.Modifier);
        }

        public static bool IsControlJustReleased(ControlSetting setting)
        {
            return IsControlJustReleased(setting.Control, setting.KeyboardOnly, setting.Modifier);
        }

        public static bool IsDisabledControlJustPressed(ControlSetting setting)
        {
            return IsDisabledControlJustPressed(setting.Control, setting.KeyboardOnly, setting.Modifier);
        }

        public static bool IsDisabledControlPressed(ControlSetting setting)
        {
            return IsDisabledControlPressed(setting.Control, setting.KeyboardOnly, setting.Modifier);
        }

        public static bool IsDisabledControlJustReleased(ControlSetting setting)
        {
            return IsDisabledControlJustReleased(setting.Control, setting.KeyboardOnly, setting.Modifier);
        }

        public static bool IsEnabledControlJustPressed(ControlSetting setting)
        {
            return IsEnabledControlJustPressed(setting.Control, setting.KeyboardOnly, setting.Modifier);
        }

        public static bool IsEnabledControlJustReleased(ControlSetting setting)
        {
            return IsEnabledControlJustReleased(setting.Control, setting.KeyboardOnly, setting.Modifier);
        }

        public static bool IsEnabledControlPressed(ControlSetting setting)
        {
            return IsEnabledControlPressed(setting.Control, setting.KeyboardOnly, setting.Modifier);
        }
    }
}
