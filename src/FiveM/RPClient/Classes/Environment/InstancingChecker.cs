using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Environment
{
    static class InstancingChecker
    {
        static PlayerList playerList;
        static bool lastCheckFailed = false;

        public static void Init()
        {
            playerList = new PlayerList();

            Client.GetInstance().RegisterEventHandler("InstancingChecker.PlayerCount", new Func<int, Task>(PlayerCountUpdate));
            // Server sends its playercount to all clients once a minute to trigger InstancingChecker.PlayerCount
            // Compare player numbers, and if player count locally is lower TWICE IN A ROW
            // yell at player and trigger the servertrigger InstancingChecker.RequestDrop after N seconds

            // If this does not work as intended when we go live just disable it completely
        }

        static public async Task PlayerCountUpdate(int remoteCount)
        {
            int localCount = playerList.Count();
            if(localCount < remoteCount)
            {
                if(lastCheckFailed)
                {
                    BaseScript.TriggerEvent("Chat.Message", "INSTANCED", "#FF0000", "The playercount as reported by your local client is lower than the one reported by the server. You will be dropped in 60 seconds.");
                    await BaseScript.Delay(60000);
                    BaseScript.TriggerServerEvent("InstancingChecker.RequestDrop");
                }
                else
                {
                    lastCheckFailed = true;
                }
            }
            else
            {
                lastCheckFailed = false;
            }
        }
    }
}
