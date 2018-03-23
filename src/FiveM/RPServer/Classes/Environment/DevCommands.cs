using System;
using System.Linq;
using CitizenFX.Core;
using FamilyRP.Roleplay.SharedClasses;

namespace FamilyRP.Roleplay.Server
{
    class DevCommands : CommandProcessor
    {
		public DevCommands()
        {
            Server.GetInstance().RegisterEventHandler("Chat.DevCommandEntered", new Action<Player, string>(Handle));
            Server.GetInstance().RegisterEventHandler("Dev.Bring", new Action<Player, int>(Bring));

            Register("source", DebugSource);
        }

        private static void DebugSource(Command command)
        {
            try
            {
                Log.Verbose($"DevCommands DebugSource");
                BaseScript.TriggerClientEvent(command.Source, "Chat.Message", "DEVTOOLS", "#00FF00", $@"Handle {command.Source.Handle}: {String.Join(", ", command.Source.Identifiers)}");
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.DebugSource exception: {ex.Message}");
            }
        }

        public static void Bring([FromSource] Player source, int bringId)
        {
            try
            {
                var targetPlayer = new PlayerList().Where(p => p.Handle == bringId.ToString());
                if (targetPlayer.Count() > 0)
                {
                    Log.Debug($"[DEVTOOLS] Player '{source.Name}' brought '{targetPlayer.First().Name}'");
                    BaseScript.TriggerClientEvent(targetPlayer.First(), "Dev.Bring", source.Handle);
                }
                else
                {
                    Log.Debug($"[DEVTOOLS] Player '{source.Name}' tried to bring somebody but ID '{bringId}' matched nobody");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DevTools.DebugSource exception: {ex.Message}");
            }
        }
    }
}