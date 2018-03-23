using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using FamilyRP.Common;
using FamilyRP.Roleplay.SharedClasses;
using MsgPack.Serialization;
using SharedModels;

namespace Chat.Client
{
    class LocalEvent
    {
        public Dictionary<string, Func<PointEvent, Task>> PointEventHandlers = new Dictionary<string, Func<PointEvent, Task>>();

        public void HandleLocalEvent(string serializedPointEvent)
        {
            try
            {
                var serializer = MessagePackSerializer.Get<PointEvent>();
                var buffer = new MemoryStream(StringExtensions.StringToBytes(serializedPointEvent));
               PointEvent pointEvent = serializer.Unpack(buffer);

                if (pointEvent.IgnoreOwnEvent && pointEvent.SourceServerId == Game.Player.ServerId)
                    return;

                if (Game.PlayerPed.Position.DistanceToSquared2D(pointEvent.Position.ToVector3()) < Math.Pow(pointEvent.AoeRange, 2))
                    PointEventHandlers[pointEvent.EventName].Invoke(pointEvent);
            }
            catch (Exception ex)
            {
                Log.Error($"HandleLocalEvent error: {ex}");
            }
        }

        private Task HandleLocalChat(PointEvent pointEvent)
        {
            BaseScript.TriggerEvent("Chat.Message", new PlayerList()[pointEvent.SourceServerId].Name, "#FFD23F", pointEvent.SerializedArguments);

            return Task.FromResult(0);
        }
    }
}
