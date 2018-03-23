using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Server.Helpers.NativeWrappers
{
    public static class NativeWrappers
    {
        public static void GetPlayerEP(string netId)
        {
            Function.Call(Hash.GET_PLAYER_ENDPOINT, netId);
        }

        public static int GetConvarInt(string convar, int defaultValue)
        {
            return Function.Call<int>(Hash.GET_CONVAR_INT, convar, defaultValue);
        }

        /// <summary>
        /// I assume this is the correct fingerprint, just guessed based on GetConvarInt (check FiveM reference)
        /// </summary>
        /// <param name="convar"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetConvar(string convar, string defaultValue)
        {
            return Function.Call<string>(Hash.GET_CONVAR, convar, defaultValue);
        }

        public static int GetPlayerLastMsg(string netId)
        {
            return Function.Call<int>(Hash.GET_PLAYER_LAST_MSG, netId);
        }

        public static string GetPlayerName(string netId)
        {
            return Function.Call<string>(Hash.GET_PLAYER_NAME, netId);
        }

        public static string GetHostId()
        {
            return Function.Call<string>(Hash.GET_HOST_ID, new InputArgument[0]);
        }

        public static void EnableEnhancedHostSupport(bool enabled)
        {
            Function.Call(Hash.ENABLE_ENHANCED_HOST_SUPPORT, enabled);
        }

        public static string[] GetPlayerIdentifiers(string netId)
        {
            int numIndentifiers = Function.Call<int>(Hash.GET_NUM_PLAYER_IDENTIFIERS, netId);

            List<string> identities = new List<string>();

            int i = numIndentifiers - 1;
            do
            {
                string identity = Function.Call<string>(Hash.GET_PLAYER_IDENTIFIER, netId, i);
                identities.Add(identity);
                i--;
            }
            while (i >= 0);

            return identities.ToArray();
        }

        public static string GetSteamID(string netId, bool withoutLabel = false)
        {
            string[] idents = GetPlayerIdentifiers(netId);

            if (idents != null)
            {
                foreach (string ident in idents)
                {
                    if (ident[0] == 's')
                    {
                        if (withoutLabel)
                        {
                            return ident.Substring(6);
                        }

                        return ident;
                    }
                }
            }

            return null;
        }

        public static void CancelEvent()
        {
            Function.Call(Hash.CANCEL_EVENT);
        }

        public static bool WasEventCancelled()
        {
            return Function.Call<bool>(Hash.WAS_EVENT_CANCELED);
        }

        public static void DropPlayer(string netId, string reason = "Dropped")
        {
            Function.Call(Hash.DROP_PLAYER, netId, reason);
        }

        public static void GetPlayerPing(string netId)
        {
            Function.Call(Hash.GET_PLAYER_PING, netId);
        }
    }
}
