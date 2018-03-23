using CitizenFX.Core;
using Client.Helpers.NativeWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveM.Client
{
    class Scoreboard : BaseScript
    {
        bool scoreboardVisible = false;
        PlayerList playerList = new PlayerList();
        Dictionary<int, DateTime> playerJoins = new Dictionary<int, DateTime>();
        DateTime? lastReboot = null;

        public Scoreboard()
        {
            EventHandlers["playerSpawned"] += new Action(() => { Debug.WriteLine("Requesting timestamps from server"); TriggerServerEvent("HardCap.RequestPlayerTimestamps"); });
            EventHandlers["playerConnecting"] += new Action<int>((serverId) => { Debug.WriteLine("A player is connecting; adding to scoreboard"); playerJoins.Add(serverId, DateTime.Now); });
            EventHandlers["playerDropped"] += new Action<int, string>((serverId, reason) => {
                Debug.WriteLine("A player is discconnecting; removing from scoreboard");
                if (playerJoins.ContainsKey(serverId)) playerJoins.Remove(serverId); });
            EventHandlers["Scoreboard.ReceivePlayerTimestamps"] += new Action<string, string>((dict, serializedTimeStamp) => {
                Debug.WriteLine("Received timestamps for scoreboard");
                var res = FamilyRP.Roleplay.Helpers.MsgPack.Deserialize<Dictionary<int, DateTime>>(dict);
                res.ToList().ForEach(i => { if (!playerJoins.ContainsKey(i.Key)) playerJoins.Add(i.Key, i.Value.ToLocalTime()); } );
                lastReboot = FamilyRP.Roleplay.Helpers.MsgPack.Deserialize<DateTime>(serializedTimeStamp).ToLocalTime();
            });

            Tick += OnTick;
        }

        private Task OnTick()
        {
            try
            {
                if(Game.IsControlPressed(0, Control.ReplayStartStopRecording))
                {
                    if(!scoreboardVisible)
                    {

                        scoreboardVisible = true;
                        string Json = $@"{{""text"": ""{String.Join("", playerList.Select(player => $@"<tr class=\""playerRow\""><td class=\""playerId\"">{player.ServerId}</td><td class=\""playerName\"">{player.Name.Replace(@"'", @"&apos;").Replace(@"""", @"&quot;")}</td><td class=\""playerTime\"">{(playerJoins.ContainsKey(player.ServerId) ? $@"{(DateTime.Now.Subtract(playerJoins[player.ServerId]).Hours > 0 ? $"{DateTime.Now.Subtract(playerJoins[player.ServerId]).Hours}h {DateTime.Now.Subtract(playerJoins[player.ServerId]).Minutes}h" : $"{DateTime.Now.Subtract(playerJoins[player.ServerId]).Minutes}m")}" : "")}</td></tr>"))}""";
                        if (lastReboot != null)
                            Json += $@", ""uptime"": ""{(DateTime.Now.Subtract((DateTime)lastReboot).Hours > 0 ? $"{DateTime.Now.Subtract((DateTime)lastReboot).Hours}h {DateTime.Now.Subtract((DateTime)lastReboot).Minutes}h" : $"{DateTime.Now.Subtract((DateTime)lastReboot).Minutes}m")}""}}";
                        else
                            Json += $@", ""uptime"": """"}}";
                        NativeWrappers.SendNuiMessage(Json);
                        Debug.WriteLine("Sent JSON to Scoreboard");
                    }
                }
                else
                {
                    if(scoreboardVisible)
                    {
                        scoreboardVisible = false;
                        string Html = $@"{{""meta"": ""close""}}";
                        NativeWrappers.SendNuiMessage(Html);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Scoreboard Error: {ex.Message}");
            }
            return Task.FromResult(0);
        }
        
        // Temporary
        public static string CleanForJSON(string s)
        {
            if (s == null || s.Length == 0) {
                return "";
            }

            char         c = '\0';
            int          i;
            int          len = s.Length;
            StringBuilder sb = new StringBuilder(len + 4);
            String       t;

            for (i = 0; i < len; i += 1) {
                c = s[i];
                switch (c) {
                    case '\\':
                    case '"':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '/':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    default:
                        if (c < ' ') {
                            t = "000" + String.Format("X", c);
                            sb.Append("\\u" + t.Substring(t.Length - 4));
                        } else {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }
    }

}
