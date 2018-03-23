using CitizenFX.Core;
using Server.Helpers.NativeWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveM.Server
{
    public class HardCap : BaseScript
    {
        public Dictionary<int, DateTime> activePlayers = new Dictionary<int, DateTime>();
        DateTime serverStartTime = DateTime.UtcNow;
        int playerCount = 0;
        int maxClients;

        public HardCap()
        {
            RegisterEventHandler("HardCap.PlayerActivated", new Action<Player>(PlayerActivated));
            RegisterEventHandler("playerDropped", new Action<Player, string>(PlayerDropped));
            RegisterEventHandler("playerConnecting", new Action<Player, string, CallbackDelegate>(PlayerConnecting));
            RegisterEventHandler("HardCap.RequestPlayerTimestamps", new Action<Player>(HandleRequest));

            maxClients = NativeWrappers.GetConvarInt("sv_maxclients", 32);
            Debug.WriteLine("HardCap initialized");
        }

        private void HandleRequest([FromSource] Player source)
        {
            try
            {
                source.TriggerEvent("Scoreboard.ReceivePlayerTimestamps", FamilyRP.Roleplay.Helpers.MsgPack.Serialize(activePlayers), FamilyRP.Roleplay.Helpers.MsgPack.Serialize(serverStartTime));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HardCap HandleRequest Error: {ex.Message}");
            }
        }

        internal void PlayerActivated([FromSource] Player source)
        {
            int sessionId = Int32.Parse(source.Handle);
            if (!activePlayers.ContainsKey(sessionId))
            {
                activePlayers.Add(sessionId, DateTime.UtcNow);
            }
        }

        internal void PlayerDropped([FromSource] Player source, string reason)
        {
            try
            { 
                int sessionId = Int32.Parse(source.Handle);
                if(activePlayers.ContainsKey(sessionId)) activePlayers.Remove(sessionId);
                BaseScript.TriggerClientEvent("playerDropped", source.Handle, reason);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"HardCap PlayerDropped Error: {ex.Message}");
            }
        }

        internal void PlayerConnecting([FromSource] Player source, string playerName, CallbackDelegate DenyWithReason)
        {
            try
            {
                Debug.WriteLine($"Connecting: '{source.Name}' (steam: {source.Identifiers.Where(i => i.Contains("steam")).FirstOrDefault().ToString()} ip: {source.Identifiers.Where(i => i.Contains("ip")).FirstOrDefault().ToString()}) | Player count {activePlayers.Count}/{maxClients}");

                if (activePlayers.Count >= maxClients)
                {
                    DenyWithReason?.Invoke($"The server is full with {playerCount}/{maxClients} players on.");
                    NativeWrappers.CancelEvent();
                }
                BaseScript.TriggerClientEvent("playerConnecting", source.Handle, playerName);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"HardCap PlayerConnecting Error: {ex.Message}");
            }
        }

        public void RegisterEventHandler(string name, Delegate action)
        {
            EventHandlers[name] += action;
        }
    }
}
