using System;
using FamilyRP.Roleplay;
using CitizenFX.Core;
using CitizenFX.Core.Native;

#if SERVER
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FamilyRP.Roleplay.Server;
#elif CLIENT
using FamilyRP.Roleplay.Client;
#endif

namespace FamilyRP.Roleplay.SharedClasses
{
	public static class Log
	{
        // If you are confused by the below log functions (ServerToClient, ClientToServer) just look at their access modifiers.
        // Hopefully the changes below didn't mess too much with your mock setup @TournyMasterBot
#if SERVER
        static Log()
        {
			try {
                    Server.Server.GetInstance().RegisterEventHandler("Log.PrintToClientConsole", new Action<string>(Info));
            }
			// This should only happen in the mock environment
			catch( Exception ex ) {
				Debug.WriteLine( $@"Log Init Error > {ex.Message}" );
			}
        }
            
        internal static void ClientToServer([FromSource] CitizenFX.Core.Player source, string message)
        {
            CitizenFX.Core.Debug.WriteLine($@"[LOG FROM CLIENT ('{source.Name}', #{source.Handle})] {message}");
        }

        public static void ServerToClient(string message, CitizenFX.Core.Player player = null, bool toChat = true)
        {
            if(toChat)
            { 
                if(player == null)
                { 
                    BaseScript.TriggerClientEvent("Log.PrintToClientConsole", message);
                }
                else
                {
                    BaseScript.TriggerClientEvent(player, "Log.PrintToClientConsole", message);
                }
            }
            else
            {
                if (player == null)
                {
                    BaseScript.TriggerClientEvent("Chat.Message", "LOG FROM SERVER", consoleNameColorInClientChat, message);
                }
                else
                {
                    BaseScript.TriggerClientEvent(player, "Chat.Message", "LOG FROM SERVER", consoleNameColorInClientChat, message);
                }
            }
        }
#elif CLIENT
        static Log()
        {
            Client.GetInstance().RegisterEventHandler("Log.PrintToClientConsole", new Action<string>(ServerToClient));
        }
        
        internal static void ServerToClient(string message)
        {
            CitizenFX.Core.Debug.WriteLine($@"[LOG FROM SERVER] {message}");
        }
        
        public static void ClientToServer(string message)
        {
            BaseScript.TriggerServerEvent("Log.PrintToServerConsole", $@"{message}");
        }
        
        public static void ToChat(string message)
        {
            BaseScript.TriggerEvent("Chat.Message", "LOG", "#574AE2", $@"{message}");
        }
#endif
	}
}