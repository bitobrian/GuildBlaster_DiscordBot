using GuildBlaster_DiscordBot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp_Starter
{
    class GuildNews
    {        
        public string LatestGuildNews { get; set; }
        private object props = GuildBlaster_DiscordBot.Properties.Settings.Default;

        public GuildNews()
        {
            LoadGuildNews();
        }

        private void LoadGuildNews()
        {
            string requestUrl = @"https://us.api.battle.net/wow/guild/"+
                WebUtility.HtmlEncode(GuildBlaster_DiscordBot.Properties.Settings.Default.TargetRealm) +
                "/"+ WebUtility.HtmlEncode(GuildBlaster_DiscordBot.Properties.Settings.Default.GuildName) +
                "?fields=news&locale=en_US&apikey=";
            
            WowApi api = new WowApi();

            RootObject obj = JsonConvert.DeserializeObject<RootObject>(api.GetJSONFromURI(requestUrl));

            string latestNewsString = "";

            latestNewsString += GuildBlaster_DiscordBot.Properties.Settings.Default.GuildName+" latest news: \n";

            int i = 0;

            foreach (var kvp in obj.news)
            {        
                if (i > 4) break;
                if(kvp.type.Equals("playerAchievement"))
                {
                    latestNewsString += kvp.character.ToString() +"'s achievement: " + kvp.achievement.description.ToString()+"\n";
                }
                else if(kvp.type.Equals("itemLoot"))
                {
                    latestNewsString += kvp.character.ToString() + "'s loot: " + "http://www.wowhead.com/item="+kvp.itemId.ToString()/*GetItemById(kvp.itemId.ToString())*/+ "\n";
                }
                i++;
            }

            LatestGuildNews = latestNewsString.ToString();
        }

        private string GetItemById(string v)
        {
            Item item = new Item(v);
            return item.ItemName;
        }

        public class Emblem
        {
            public int icon { get; set; }
            public string iconColor { get; set; }
            public int iconColorId { get; set; }
            public int border { get; set; }
            public string borderColor { get; set; }
            public int borderColorId { get; set; }
            public string backgroundColor { get; set; }
            public int backgroundColorId { get; set; }
        }

        public class Criterion
        {
            public int id { get; set; }
            public string description { get; set; }
            public int orderIndex { get; set; }
            public int max { get; set; }
        }

        public class Achievement
        {
            public int id { get; set; }
            public string title { get; set; }
            public int points { get; set; }
            public string description { get; set; }
            public List<object> rewardItems { get; set; }
            public string icon { get; set; }
            public List<Criterion> criteria { get; set; }
            public bool accountWide { get; set; }
            public int factionId { get; set; }
        }

        public class News
        {
            public string type { get; set; }
            public string character { get; set; }
            public object timestamp { get; set; }
            public string context { get; set; }
            public List<object> bonusLists { get; set; }
            public Achievement achievement { get; set; }
            public int? itemId { get; set; }
        }

        public class RootObject
        {
            public long lastModified { get; set; }
            public string name { get; set; }
            public string realm { get; set; }
            public string battlegroup { get; set; }
            public int level { get; set; }
            public int side { get; set; }
            public int achievementPoints { get; set; }
            public Emblem emblem { get; set; }
            public List<News> news { get; set; }
        }

    }
}
