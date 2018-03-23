using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FamilyRP.Roleplay.Helpers;
using System.Collections.Generic;
using FamilyRP.Roleplay.Client.Helpers;
using FamilyRP.Roleplay.SharedClasses;
using SharedModels;

namespace RPClient.Chat
{
    public class Chat : BaseScript
    {
        private static bool isChatInputActive = false;
        private static bool isChatScrollEnabled = true;
        private static bool isChatInputActivating = false;

        // Just the current default color for player messages etc.
        private const string chatMessageColor = "#0000FF";
        const float localChatAoe = 25f;

        public Chat()
        {
            NativeWrappers.SetNuiFocus(false);
            NativeWrappers.SetTextChatEnabled(false);

            RegisterEventHandler("Chat.Message", new Action<string, string, string>(HandleChatMessage));
            RegisterEventHandler("Chat.EnableChatBox", new Action<bool>(EnableChatBox));
            // If you want to pass or add unescaped HTML to the chatbox
            // e.g. <marquee>NYAAAAAAAAAAAAN</marquee>
            RegisterEventHandler("Chat.Message.Unsafe", new Action<string>(HandleChatMessageUnsafe));
            RegisterEventHandler("Chat.EnableScroll", new Action<bool>(EnableChatScroll));
            NativeWrappers.RegisterNuiCallbackType("chatResult");
            RegisterEventHandler("__cfx_nui:chatResult", new Action<System.Dynamic.ExpandoObject>(HandleChatResult));
            
            Tick += Update;
        }

        /// <summary>
        /// Send a chat message to all users on the server
        /// </summary>
        internal void HandleChatMessage(string name, string color, string message)
        {
            try
            {
                NativeWrappers.SendNuiMessage($@"{{""name"": ""{name}"", ""color"": ""{color}"", ""message"": ""{message.Replace(@"'", @"&apos;").Replace(@"""", @"&quot;")}""}}");
            }
            catch (Exception ex)
            {
                Log.Error($"HandleChatMessage ERROR: ${ex.Message}");
            }
        }

        /// <summary>
        /// Anybody using this has to HTML escape the quotes that are not your own
        /// </summary>
        /// <param name="html"></param>
        internal void HandleChatMessageUnsafe(string html)
        {
            try
            {
                NativeWrappers.SendNuiMessage($@"{{""html"": ""{html}""}}");
            }
            catch (Exception ex)
            {
                Log.Error($"HandleChatMessageUnsafe ERROR: ${ex.Message}");
            }
        }

        internal void TriggerChatAction(string name)
        {
            NativeWrappers.SendNuiMessage($@"{{""meta"": ""{name}""}}");
        }

        internal void EnableChatScroll(bool enable)
        {
            isChatScrollEnabled = enable;
        }

        internal void EnableChatBox(bool enable)
        {
            string triggerEvent = "enableChatBox";
            if (!enable) triggerEvent = "disableChatBox";
            TriggerChatAction(triggerEvent);
        }

        internal void HandleChatResult(System.Dynamic.ExpandoObject data)
        {
            try
            {
				// The below might be handled by the Controls class when implemented
				TriggerChatAction( "scrollBottom" );

				NativeWrappers.EnableControlAction(0, Control.CursorScrollUp, true);
                NativeWrappers.EnableControlAction(0, Control.CursorScrollDown, true);
                NativeWrappers.SetPedCanSwitchWeapon(Game.PlayerPed, true);
				NativeWrappers.SetNuiFocus( false );
				isChatInputActive = false;

				IDictionary<string, object> chatResult = data;

				if( !chatResult.ContainsKey( "message" ) || string.IsNullOrWhiteSpace( (string)chatResult["message"] ) )
					return;

				string Message = (String)chatResult["message"];

				var spaceSplit = Message.Split(' ');
                if (Message.Substring(0, 1) == "/" && Message.Length >= 2)
                {
                    TriggerEvent("Chat.CommandEntered", Message);

                    // If we want the user to see the command they just entered
                    //TriggerEvent("Chat.Message", "COMMAND", "#888888", Message);
                }
                else
                {
                    // Commented as we want local chat by default
                    //TriggerServerEvent("Chat.MessageEntered", Game.Player.Name, chatMessageColor, Message);
                    //Debug.WriteLine("Sending local chat");
                    PointEvent pointEvent = new PointEvent("Communication.LocalChat", CitizenFX.Core.Game.PlayerPed.Position.ToArray(), localChatAoe, Message, Game.Player.ServerId, false);
                    BaseScript.TriggerServerEvent("TriggerEventNearPoint", FamilyRP.Roleplay.Client.Helpers.MsgPack.Serialize(pointEvent));
                }
            }
            catch(Exception ex)
            {
                Log.Error($"HandleChatResult ERROR: ${ex.Message}");
            }
        }

        public async Task Update()
        {
            try
            {
                // Currently it seems this assembly loads way earlier than the Lua ever did
                // So SetTextChatEnabled has to be called at a later point than before
                // Not sure what would be a suitable solution, so temporarily
                // it's done on each tick... Overkill.
                NativeWrappers.SetTextChatEnabled(false);

                if (isChatScrollEnabled)
                {
                    if (Game.IsControlJustPressed(0, Control.ReplayCameraUp)) // PgUp
                    {
                        TriggerChatAction("scrollUp");
                    }
                    else if (Game.IsControlJustPressed(0, Control.ReplayCameraDown)) // PgDn
                    {
                        TriggerChatAction("scrollDown");
                    }
                }

                if (isChatInputActive && Game.IsDisabledControlPressed(0, Control.CursorScrollUp)) // Scrollwheel Up
                {
                    TriggerChatAction("scrollUp");
                }
                else if (isChatInputActive && Game.IsDisabledControlPressed(0, Control.CursorScrollDown)) // Scrollwheel Down
                {
                    TriggerChatAction("scrollDown");
                }

                if (Game.IsControlJustPressed(0, Control.FrontendCancel)) // Escape
                {
                    isChatInputActive = false;
                    isChatInputActivating = false;
                    NativeWrappers.EnableControlAction(0, Control.CursorScrollUp, true); // Scrollwheel Up
                    NativeWrappers.EnableControlAction(0, Control.CursorScrollDown, true); // Scrollwheel Down
                    NativeWrappers.SetPedCanSwitchWeapon(Game.PlayerPed, true); // Disable weapon select
                    NativeWrappers.SetNuiFocus(false);

                    TriggerChatAction("forceCloseChatBox");
                }

                if (!isChatInputActive && Game.IsControlPressed(0, Control.MpTextChatAll))
                {
                    isChatInputActive = true;
                    isChatInputActivating = true;
                    TriggerChatAction("openChatBox");
                }

                if (isChatInputActivating && !Game.IsControlPressed(0, Control.MpTextChatAll))
                {
                    NativeWrappers.SetNuiFocus(true);
                    isChatInputActivating = false;
                }

                if (isChatInputActive)
                {
                    TriggerChatAction("focusChatBox");
                    NativeWrappers.DisableControlAction(0, Control.CursorScrollUp, true);
                    NativeWrappers.DisableControlAction(0, Control.CursorScrollDown, true);
                    NativeWrappers.SetPedCanSwitchWeapon(Game.PlayerPed, false);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Chat Update ERROR: ${ex.Message}");
            }

            await Task.FromResult(0);
        }

        public void RegisterEventHandler(string name, Delegate action)
        {
            EventHandlers[name] += action;
        }
    }
}
