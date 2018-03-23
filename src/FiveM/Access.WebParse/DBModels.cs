using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Access.WebParse
{

    public class ForumDbModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int BanTime { get; set; }
        public int PostCount { get; set; }
        public int MessageCount { get; set; }
        public DateTime Joined { get; set; }
        public DateTime LastVisit { get; set; }
        public DateTime LastActivity { get; set; }
        public List<int> Groups { get; set; }
        public string RowChecksum { get; set; }
        public string CurrentIP { get; set; }
    }

    public class SteamDbModel
    {
        public string Id { get; set; }
    }

    public class TwitchDbModel
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }

    public class DiscordDbModel
    {
        public string Id { get; set; }
        public string Token { get; set; }
    }

    public class GtaLicenseDbModel
    {
        public string Id { get; set; }
    }

    public class UserDbModel
    {
        public ForumDbModel Forum;
        public SteamDbModel Steam;
        public TwitchDbModel Twitch;
        public DiscordDbModel Discord;
        public GtaLicenseDbModel GtaLicense;

        public static List<int> ParseForumGroups(object MainGroup, object OtherGroups)
        {
            List<int> Groups = Convert.ToString(OtherGroups).Split(',').Where(i => i != "").Select(g => Int32.Parse(g)).ToList();
            Groups.Add(Convert.ToInt32(MainGroup));
            return Groups.Distinct().ToList();
        }

        public static UserDbModel Create(DbDataReader record)
        {
            return new UserDbModel
            {
                Forum = new ForumDbModel
                {
                    Id = Convert.IsDBNull(record["member_id"]) ? -1 : Convert.ToInt32(record["member_id"]),
                    Name = Convert.IsDBNull(record["name"]) ? null : Convert.ToString(record["name"]),
                    Email = Convert.IsDBNull(record["email"]) ? null : Convert.ToString(record["email"]),
                    BanTime = Convert.IsDBNull(record["temp_ban"]) ? -1 : Convert.ToInt32(record["temp_ban"]),
                    PostCount = Convert.IsDBNull(record["member_posts"]) ? -1 : Convert.ToInt32(record["member_posts"]),
                    MessageCount = Convert.IsDBNull(record["msg_count_total"]) ? -1 : Convert.ToInt32(record["msg_count_total"]),
                    Joined = Convert.IsDBNull(record["joined"]) ? new DateTime(1900, 1, 1) : Convert.ToDateTime(record["joined"]),
                    LastVisit = Convert.IsDBNull(record["last_visit"]) ? new DateTime(1900, 1, 1) : Convert.ToDateTime(record["last_visit"]),
                    LastActivity = Convert.IsDBNull(record["last_activity"]) ? new DateTime(1900, 1, 1) : Convert.ToDateTime(record["last_activity"]),
                    RowChecksum = Convert.IsDBNull(record["checksum"]) ? null : Convert.ToString(record["checksum"]),
                    CurrentIP = Convert.IsDBNull(record["ip_address"]) ? null : Convert.ToString(record["ip_address"]),
                    Groups = ParseForumGroups(record["member_group_id"], record["mgroup_others"])
                },
                Steam = new SteamDbModel
                {
                    Id = Convert.IsDBNull(record["steamid"]) ? null : Convert.ToString(record["steamid"])
                },
                Twitch = new TwitchDbModel
                {
                    Id = Convert.IsDBNull(record["tw_id"]) ? null : Convert.ToString(record["tw_id"]),
                    Token = Convert.IsDBNull(record["tw_token"]) ? null : Convert.ToString(record["tw_token"]),
                    Name = Convert.IsDBNull(record["tw_name"]) ? null : Convert.ToString(record["tw_name"]),
                    DisplayName = Convert.IsDBNull(record["tw_dname"]) ? null : Convert.ToString(record["tw_dname"])
                },
                Discord = new DiscordDbModel
                {
                    Id = Convert.IsDBNull(record["discord_id"]) ? null : Convert.ToString(record["discord_id"]),
                    Token = Convert.IsDBNull(record["discord_token"]) ? null : Convert.ToString(record["discord_token"])
                }
            };
        }
    }
}
