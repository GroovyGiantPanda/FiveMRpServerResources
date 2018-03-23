using System;
using System.Linq;
using CitizenFX.Core;
using System.Collections.Generic;
using CitizenFX.Core.Native;

namespace RPServer.Chat
{
    public class Chat : BaseScript
    {
        public Chat()
        {
            RegisterEventHandler("Chat.MessageEntered", new Action<Player, string, string, string>(ChatMessageEntered));
            RegisterEventHandler("playerActivated", new Action<Player>(PlayerActivated));
            RegisterEventHandler("playerDropped", new Action<Player, string>(PlayerDropped));
            RegisterEventHandler("rconCommand", new Action<string, List<object>>(RconCommand));
        }

        internal void ChatMessageEntered([FromSource] CitizenFX.Core.Player source, string name, string color, string message)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(name) || String.IsNullOrWhiteSpace(color) || String.IsNullOrWhiteSpace(message))
                {
                    // TODO: Use Log instead
                    Debug.WriteLine($"{name}: [CHAT] Invalid chat message '{message}' entered by '{source.Name}' (#{source.Handle})");
                    return;
                }
                
                TriggerClientEvent("Chat.Message", name, color, message);

                Debug.WriteLine($"{name}: {message}");
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"ChatMessageEntered ERROR: {ex.Message}");
            }
        }

        public void PlayerActivated([FromSource] CitizenFX.Core.Player source)
        {
            try
            {
                //TriggerClientEvent("Chat.Message", "", "#FFFFFF", $@"^2*{source.Name} ^0joined.");
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"PlayerActivated ERROR: {ex.Message}");
            }
        }

        PlayerList playerList = new PlayerList();
        public void PlayerDropped([FromSource] Player source, string reason)
        {
            try
            {
                //playerList.Where(p => p.Handle != source.Handle).ToList().ForEach(p => p.TriggerEvent("Chat.Message", "", "#AAAAAA", $@"{source.Name} disconnected. (Reason: {reason})"));
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error when player was dropped: {ex.Message}");
            }
        }

        internal void RconCommand(string commandName, List<object> objargs)
        {
            try
            { 
                List<string> args = objargs.Cast<string>().ToList();
                if (commandName == "say" && args.Count > 0)
                {
                    string message = String.Join(" ", args);
                    Debug.WriteLine($"console: {message}");
                    TriggerClientEvent("Chat.Message", "console", "#FF0000", message);
                    Function.Call(Hash.CANCEL_EVENT);
                }
                else if (commandName == "tell" && args.Count > 1)
                {
                    int target = Int32.Parse(args[1]);
                    string message = String.Join(" ", args.Skip(1).Take(args.Count - 1));
                    PlayerList playerList = new PlayerList();
                    CitizenFX.Core.Player player = playerList[target];
                    Debug.WriteLine($"console to {player.Name}: {message}");

                    TriggerClientEvent(player, "Chat.Message", "console", "#FF0000", message);
                    Function.Call(Hash.CANCEL_EVENT);
                }
                else
                {
                    // Let's cancel the event regardless or we get errors all over the place...
                    // Will hopefully be fixed by FiveM
                    Debug.WriteLine("No such command exists or invalid arguments");
                    Function.Call(Hash.CANCEL_EVENT);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RconCommand ERROR: {ex.Message}");
            }
        }

        public void RegisterEventHandler(string name, Delegate action)
        {
            EventHandlers[name] += action;
        }
    }
}