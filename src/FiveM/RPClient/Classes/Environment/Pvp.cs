using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes.Environment
{
    static class Pvp
    {
        static public async void Init()
        {
            while(true)
            { 
                PlayerList playerList = new PlayerList();
                playerList.ToList().ForEach(player => Function.Call(Hash.SET_CAN_ATTACK_FRIENDLY, player.Character.Handle, true, true));
                Function.Call(Hash.NETWORK_SET_FRIENDLY_FIRE_OPTION, true);
                await BaseScript.Delay(10000);
            }
        }
    }
}
