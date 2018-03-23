using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.UI;
using CitizenFX.Core;
using FamilyRP.Roleplay.SharedClasses;
using CitizenFX.Core.Native;

namespace FamilyRP.Roleplay.Client.Classes.Environment.UI
{
    static class CinematicMode 
    {
        // To allow for the faster Invoke over DynamicInvoke, we require Tasks and Func instead of just using Delegate
        static List<Func<bool, Task>> callbacks = new List<Func<bool, Task>>();
        static public bool DoHideHud { get; set; } = false;
        static float blackBarHeight = 0.0f;

        static List<CitizenFX.Core.UI.HudComponent> hideComponents = new List<HudComponent>()
        {
            CitizenFX.Core.UI.HudComponent.WantedStars,
            CitizenFX.Core.UI.HudComponent.WeaponIcon,
            CitizenFX.Core.UI.HudComponent.Cash,
            CitizenFX.Core.UI.HudComponent.MpCash,
            CitizenFX.Core.UI.HudComponent.MpMessage,
            CitizenFX.Core.UI.HudComponent.VehicleName,
            CitizenFX.Core.UI.HudComponent.AreaName,
            CitizenFX.Core.UI.HudComponent.Unused,
            CitizenFX.Core.UI.HudComponent.StreetName,
            CitizenFX.Core.UI.HudComponent.HelpText,
            CitizenFX.Core.UI.HudComponent.FloatingHelpText1,
            CitizenFX.Core.UI.HudComponent.FloatingHelpText2,
            CitizenFX.Core.UI.HudComponent.CashChange,
            CitizenFX.Core.UI.HudComponent.Reticle,
            CitizenFX.Core.UI.HudComponent.SubtitleText,
            CitizenFX.Core.UI.HudComponent.RadioStationsWheel,
            CitizenFX.Core.UI.HudComponent.Saving,
            CitizenFX.Core.UI.HudComponent.GamingStreamUnusde,
            //CitizenFX.Core.UI.HudComponent.WeaponWheel, // I think this one caused players to be unable to switch weapons?
            CitizenFX.Core.UI.HudComponent.WeaponWheelStats
        };

        static public Task RegisterCallback(Func<bool, Task> callback)
        {
            callbacks.Add(callback);
            return Task.FromResult(0);
        }

        static public void Init()
        {
            // May have to make this actually receive a string for an exported function instead
            // (Yet to be tested)
            //Exports.Add("CinematicMode.RegisterCallback", new Func<Func<bool, Task>, Task>(RegisterCallback));
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static public async Task OnTick()
        {
            if (ControlHelper.IsControlJustPressed(Control.ReplayHidehud, true, Enums.Controls.ControlModifier.Shift))
            {
                DoHideHud = !DoHideHud;
                if (DoHideHud)
                {
                    Log.Debug("HUD hidden");
                }
                else
                {
                    Log.Debug("HUD unhidden");
                }
                callbacks.ForEach(cb => { cb.Invoke(!DoHideHud); });
                Function.Call(Hash.DISPLAY_RADAR, !DoHideHud);
                BaseScript.TriggerEvent("Chat.EnableChatBox", !DoHideHud);
            }
            else if (ControlHelper.IsControlJustPressed(Control.ReplayHidehud, true, Enums.Controls.ControlModifier.Alt))
            {
                switch(blackBarHeight)
                {
                    case 0f:
                        blackBarHeight = 0.15f;
                        break;
                    case 0.15f:
                        blackBarHeight = 0.19f;
                        break;
                    case 0.19f:
                        blackBarHeight = 0f;
                        break;
                }
            }
            if (DoHideHud)
            {
                hideComponents.ForEach(c => CitizenFX.Core.UI.Screen.Hud.HideComponentThisFrame(c));
            }
            if(blackBarHeight > 0f)
            {
                Function.Call(Hash.DRAW_RECT, 0.5f, blackBarHeight / 2, 1f, blackBarHeight, 0, 0, 0, 255);
                Function.Call(Hash.DRAW_RECT, 0.5f, 1 - blackBarHeight / 2, 1f, blackBarHeight, 0, 0, 0, 255);
            }
            await Task.FromResult(0);
        }
    }
}
