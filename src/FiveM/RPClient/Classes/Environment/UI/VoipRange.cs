using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyRP.Roleplay.Client;
using System.Collections.Specialized;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Client.Classes.Environment.UI;

namespace FamilyRP.Roleplay.Client.Classes.Environment
{
    // TODO: Test and debug
    static class Voip
    {
        static SortedDictionary<float, string> voipRange = new SortedDictionary<float, string>()
        {
            [3.0f] = "Whispering",
            [17.0f] = "Low",
            [25.0f] = "Normal",
            [43.0f] = "Shouting"
        };
        static public KeyValuePair<float, string> currentRange = voipRange.ElementAt(2);

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
            Function.Call(Hash.NETWORK_SET_TALKER_PROXIMITY, currentRange.Key);
        }

        static public Task OnTick()
        {
            if (ControlHelper.IsControlJustPressed(Settings.Controls["VoipRange.Toggle"]))
            {
                currentRange = voipRange.ElementAt((voipRange.Keys.ToList().IndexOf(currentRange.Key) + 1) % (voipRange.Count - 1));
                BaseScript.TriggerEvent("Chat.Message", "", "#AAFF99", $"VOIP range set to {currentRange.Value}.");
                Function.Call(Hash.NETWORK_SET_TALKER_PROXIMITY, currentRange.Key);
            }

			if( !CinematicMode.DoHideHud ) {
				System.Drawing.Color colorInactive = System.Drawing.Color.FromArgb( 200, 200, 200, 200 );
				System.Drawing.Color colorActive = System.Drawing.Color.FromArgb( 255, 138, 201, 38 );

				Roleplay.Client.UI.DrawText( $"Range: {currentRange.Value}", new Vector2( 0.18f, 0.95f ), ControlHelper.IsControlPressed( Control.PushToTalk, false, Enums.Controls.ControlModifier.Any ) ? colorActive : colorInactive, 0.25f, 0, false );
			}

            return Task.FromResult(0);
        }
    }
}
