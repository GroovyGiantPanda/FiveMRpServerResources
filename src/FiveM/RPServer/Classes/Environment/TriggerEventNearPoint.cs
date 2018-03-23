using CitizenFX.Core;
using FamilyRP.Roleplay.SharedClasses;
using FamilyRP.Roleplay.SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedModels;

namespace FamilyRP.Roleplay.Server.Classes
{
    class TriggerEventNearPoint : BaseScript
    {
        public TriggerEventNearPoint()
        {
            Server.GetInstance().RegisterEventHandler("TriggerEventNearPoint", new Func<Player, string, Task>(Handle));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializedPointEvent">MsgPacked PointEvent object</param>
        /// <returns></returns>
        public Task Handle([FromSource] Player source, string serializedPointEvent)
        {
            PointEvent pe = Helpers.MsgPack.Deserialize<PointEvent>(serializedPointEvent);
            if(pe.EventName == "Communication.SlashMe" || pe.EventName == "Communication.LocalChat")
            { 
                Log.Info($"{source.Name}:{(pe.EventName == "Communication.SlashMe" ? " /me" : "")} {pe.SerializedArguments}");
            }
            else if(pe.EventName != "Weapons.ManipulationEvent")
            {
                Log.Debug("Received TriggerEventNearPoint");
            }
            TriggerClientEvent("TriggerEventNearPoint", serializedPointEvent);
            return Task.FromResult(0);
        }
    }
}
