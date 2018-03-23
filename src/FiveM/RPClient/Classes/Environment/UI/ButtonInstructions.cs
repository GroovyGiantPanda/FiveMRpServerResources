using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Environment.UI
{
    // TODO: Test and debug
    // Implemented, just not yet used
    // I need to explain how to use it later
    // Basically, register a callback that either returns null or a KVP containing a button and a string for what it does
    static class ButtonInstructions
    {
        static internal Dictionary<Control, string> instructions = new Dictionary<Control, string>();
        // This list is for other classes to register for a check every frame
        // On whether they want an instructional button to show or not
        static public List<Func<KeyValuePair<Control, string>?>> StatusCheckCallbacks = new List<Func<KeyValuePair<Control, string>?>>();
        static internal CitizenFX.Core.Scaleform scaleform;
        static internal bool hasLoadedScaleform = false;

        static public void Init()
        {
            Client.ActiveInstance.RegisterTickHandler(OnTick);
        }

        static public async Task OnTick()
        {
            if(instructions.Count() > 0)
            {
                if(!hasLoadedScaleform)
                { 
                    scaleform = new Scaleform("instructional_buttons");
                    hasLoadedScaleform = true;
                }
                else
                {
                    instructions.Clear();
                    StatusCheckCallbacks
                        .ForEach(cb =>
                        { KeyValuePair<Control, string>? kvn = cb();
                            if (kvn != null)
                            {
                                KeyValuePair<Control, string> kv = (KeyValuePair<Control, string>)kvn;
                                instructions.Add(kv.Key, kv.Value);
                            }
                        });

                    if (instructions.Count() > 0)
                    {
                        scaleform.CallFunction("CLEAR_ALL", -1);
                        scaleform.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
                        scaleform.CallFunction("CREATE_CONTAINER");
                        int slot = 0;
                        instructions
                            .ToList()
                            .ForEach(i => 
                            {
                                scaleform.CallFunction("SET_DATA_SLOT", slot, Function.Call<string>(Hash.GET_CONTROL_INSTRUCTIONAL_BUTTON, 0, i.Key, 1), i.Value);
                                slot++;
                            });
                        scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
                        scaleform.Render2D();
                    }
                }
            }
            await Task.FromResult(0);
        }
    }
}
