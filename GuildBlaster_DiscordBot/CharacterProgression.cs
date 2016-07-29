using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuildBlaster_DiscordBot
{
    class CharacterProgression
    {
        private string Name { get; set; }
        private string Server { get; set; }
        private string URI { get; set;}
        private string jsonResult { get; set; }
        private RootObject obj { get; set; }
        public bool IsError { get; set; }

        public CharacterProgression(string charName)
        {
            Name = charName;
            Server = Properties.Settings.Default.TargetRealm;
            URI = @"https://us.api.battle.net/wow/character/" + Server + "/" + Name + "?fields=progression&locale=en_US&apikey=";

            GetCharacterProgressionData();
        }

        private void GetCharacterProgressionData()
        {
            WowApi api = new WowApi();
            try
            {
                obj = JsonConvert.DeserializeObject<RootObject>(api.GetJSONFromURI(URI));
            }
            catch(Exception e)
            {
                IsError = true;
            }

            // TODO: do something with char progression data

        }

        public string GetHonorableKills()
        {
            if (IsError) return "NULL";
            return obj.totalHonorableKills.ToString("N0");
        }

        public class Boss
        {
            public int id { get; set; }
            public string name { get; set; }
            public int normalKills { get; set; }
            public object normalTimestamp { get; set; }
            public int? heroicKills { get; set; }
            public long? heroicTimestamp { get; set; }
            public int? lfrKills { get; set; }
            public long? lfrTimestamp { get; set; }
            public int? mythicKills { get; set; }
            public int? mythicTimestamp { get; set; }
        }

        public class Raid
        {
            public string name { get; set; }
            public int lfr { get; set; }
            public int normal { get; set; }
            public int heroic { get; set; }
            public int mythic { get; set; }
            public int id { get; set; }
            public List<Boss> bosses { get; set; }
        }

        public class Progression
        {
            public List<Raid> raids { get; set; }
        }

        public class RootObject
        {
            public long lastModified { get; set; }
            public string name { get; set; }
            public string realm { get; set; }
            public string battlegroup { get; set; }
            public int @class { get; set; }
            public int race { get; set; }
            public int gender { get; set; }
            public int level { get; set; }
            public int achievementPoints { get; set; }
            public string thumbnail { get; set; }
            public string calcClass { get; set; }
            public int faction { get; set; }
            public Progression progression { get; set; }
            public int totalHonorableKills = 0;
        }

    }
}
