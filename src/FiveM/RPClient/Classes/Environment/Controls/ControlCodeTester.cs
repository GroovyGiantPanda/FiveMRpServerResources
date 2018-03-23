using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FamilyRP.Roleplay.SharedClasses;

namespace FamilyRP.Roleplay.Client.Classes.Environment.Controls
{
    static class ControlCodeTester
    {
        internal static Dictionary<Control, bool> KeyStates;
        internal static bool _enabled;
        public static bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled == value)
                    return;
                else
                {
                    Init();
                    if (value)
                    { 
                        Client.GetInstance().RegisterTickHandler(KeyCheck);
                    }
                    else
                    { 
                        Client.GetInstance().DeregisterTickHandler(KeyCheck);
                    }
                    _enabled = value;
                }
            }
        }

        public static void Init()
        {
            KeyStates = new Dictionary<Control, bool>();
        }

        public static async Task KeyCheck()
        {
            try { 
                foreach (Control control in Enum.GetValues(typeof(Control)))
                {
                    if (CitizenFX.Core.Game.IsControlJustPressed(0, control))
                    {
                        if (KeyStates.Keys.Count(k => k == control) == 0)
                        {
                            Log.ToChat($"Key Control.{control.ToString()} (#{(int) control}) pressed");
                            KeyStates.Add(control, true);
                        }
                        else
                        {
                            if (!KeyStates[control])
                            {
                                Log.ToChat($"Key Control.{control.ToString()} (#{(int)control}) pressed");
                            }
                            KeyStates[control] = true;
                        }
                    }
                    else
                    {
                        KeyStates[control] = false;
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error($"[KEYCODETESTER] {ex.GetType().ToString()} thrown");
            }
            await Task.FromResult(0);
        }

    }
}
