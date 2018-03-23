using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Environment
{
    // Tested and working
    static class AfkKick
    {
        static bool isDev = false;

        static Vector3 lastPlayerLocation = new Vector3();
        static int lastMovement = 0;
        static int lastKeyPress = 0;
        static float afkLimit = 30f; // How many minutes since last movement or keypress we allow before kick
        static float afkWarning = 10f; // How many minutes before to warn player

        // Also, if dev don't kick
        static public void Init()
        {
            lastMovement = lastKeyPress = Function.Call<int>(Hash.GET_GAME_TIMER);
            Client.GetInstance().RegisterTickHandler(OnTick);
            MinuteLoop();
        }

        static public Task OnTick()
        {
            if(ControlHelper.IsControlJustPressed(Control.PushToTalk) || ControlHelper.IsControlJustPressed(Control.MpTextChatAll))
            {
                lastKeyPress = Function.Call<int>(Hash.GET_GAME_TIMER);
            }
            return Task.FromResult(0);
        }

        static public async Task MinuteLoop()
        {
            while(true)
            {
                await BaseScript.Delay(60000);
                if(lastPlayerLocation != (lastPlayerLocation = Game.PlayerPed.Position))
                {
                    lastMovement = Function.Call<int>(Hash.GET_GAME_TIMER);
                }

                int currentTime = Function.Call<int>(Hash.GET_GAME_TIMER);
                if (((currentTime - lastMovement) > 60000 * afkLimit || (currentTime - lastKeyPress) > 60000 * afkLimit) && !isDev)
                {
                    BaseScript.TriggerServerEvent("AfkKick.RequestDrop");
                }
                else if ((currentTime - lastMovement) > 60000 * (afkLimit - afkWarning) || (currentTime - lastKeyPress) > 60000 * (afkLimit - afkWarning))
                {
                    float timeRemaining = Math.Max((currentTime - lastMovement), (currentTime - lastKeyPress)) / 60000f;
                    Log.ToChat($@"You will be kicked for being AFK in {(afkLimit - timeRemaining):0} minute{(afkLimit - timeRemaining > 1 ? "s" : "")}.");
                }
            }
        }
    }
}
