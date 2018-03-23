using CitizenFX.Core;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Server.Classes.Environment
{
    // TODO: 
    class TriggerEventForMultiple : BaseScript
    {
        PlayerList playerList = new PlayerList();

        public TriggerEventForMultiple()
        {
            Server.GetInstance().RegisterEventHandler("TriggerEventForPlayers", new Func<Player, string, Task>(TriggerEventForPlayers));
            Server.GetInstance().RegisterEventHandler("TriggerEventForAll", new Func<Player, string, Task>(TriggerEventForAll));
        }

        // Works
        private Task TriggerEventForPlayers([FromSource] Player source, string serializedModel)
        {
            try
            { 
                var eventData = Helpers.MsgPack.Deserialize<TriggerEventForPlayersModel>(serializedModel);
                if (eventData.passFullSerializedModel)
                {
                    eventData.sourceServerId = Int32.Parse(source.Handle);
                    serializedModel = Helpers.MsgPack.Serialize(eventData);
                    eventData.serverIds.ToList().ForEach(p => playerList[p].TriggerEvent(eventData.eventName, serializedModel));
                }
                else
                {
                    eventData.serverIds.ToList().ForEach(p => playerList[p].TriggerEvent(eventData.eventName, eventData.Payload));
                }
            }
            catch(Exception ex)
            {
                Log.Error($"[TriggerEventForPlayers] ERROR: {ex.Message}");
            }
            return Task.FromResult(0);
        }

        private Task TriggerEventForAll([FromSource] Player source, string serializedModel)
        {
            try
            {
                var eventData = Helpers.MsgPack.Deserialize<TriggerEventForAllModel>(serializedModel);
                if (eventData.passFullSerializedModel)
                {
                    eventData.sourceServerId = Int32.Parse(source.Handle);
                    serializedModel = Helpers.MsgPack.Serialize(eventData);
                    TriggerClientEvent(eventData.eventName, serializedModel);
                }
                else
                {
                    TriggerClientEvent(eventData.eventName, eventData.Payload);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[TriggerEventForAll] ERROR: {ex.Message}");
            }
            return Task.FromResult(0);
        }
    }
}
