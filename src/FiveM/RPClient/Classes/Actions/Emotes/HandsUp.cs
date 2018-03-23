using CitizenFX.Core;
using FamilyRP.Roleplay.Enums.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Actions.Emotes
{
    static class HandsUp
    {
        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static private async Task OnTick()
        {
            if (ControlHelper.IsControlPressed(Control.MultiplayerInfo, true, ControlModifier.Ctrl))
            {
                await Game.PlayerPed.Task.PlayAnimation("random@mugging3", "handsup_standing_base", 3f, 3f, -1, (AnimationFlags)49, 0);
                while(ControlHelper.IsControlPressed(Control.MultiplayerInfo, true, ControlModifier.Ctrl))
                {
                    await BaseScript.Delay(0);
                }
                Game.PlayerPed.Task.ClearAll();
            }
            else if (ControlHelper.IsControlPressed(Control.MultiplayerInfo, true, ControlModifier.Alt))
            {
                await Game.PlayerPed.Task.PlayAnimation("random@arrests@busted", "idle_a", 3f, 3f, -1, (AnimationFlags)49, 0);
                while (ControlHelper.IsControlPressed(Control.MultiplayerInfo, true, ControlModifier.Alt))
                {
                    await BaseScript.Delay(0);
                }
                Game.PlayerPed.Task.ClearAll();
            }
            await Task.FromResult(0);
        }
    }
}
