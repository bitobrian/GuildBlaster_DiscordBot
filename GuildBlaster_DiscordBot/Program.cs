using DiscordSharp;
using DiscordSharp.Objects;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using DiscordSharp.Events;
using System.IO;
using Newtonsoft.Json;
using DiscordSharp_Starter;
using System.Text.RegularExpressions;

namespace GuildBlaster_DiscordBot
{
    class Program
    {
        #region Class Vars
        public static bool isbot = true;
        private static DiscordClient client;
        private static Realm realm;
        #endregion

        #region Main
        static void Main(string[] args)
        {

            #region Bot Startup
            // First of all, a DiscordClient will be created, and the email and password will be defined.
            Console.WriteLine("Starting GuildBlast Bot");
            Console.WriteLine("Reading config");

            // Fill in token and change isbot to true
            client = new DiscordClient(Properties.Settings.Default.DiscordBotToken, true);
            client.ClientPrivateInformation.Email = Properties.Settings.Default.ClientEmail;
            client.ClientPrivateInformation.Password = Properties.Settings.Default.ClientPassword;
            realm = new Realm();

            // Get Realm Info
            LoadRealmStatus();
                        
            #endregion

            #region Events - General
            Console.WriteLine("Defining Events");
            // find that one you interested in 

            client.Connected += (sender, e) => // Client is connected to Discord
            {
                Console.WriteLine("Connected! User: " + e.User.Username);
                // If the bot is connected, this message will show.
                // Changes to client, like playing game should be called when the client is connected,
                // just to make sure nothing goes wrong.
                client.UpdateCurrentGame("Guild Crier"); // This will display at "Playing: "
                //Whoops! i messed up here. (original: Bot online!\nPress any key to close this window.)
            };


            client.PrivateMessageReceived += (sender, e) => // Private message has been received
            {
                if (e.Message == "!help")
                {
                    e.Author.SendMessage("This is a private message!");
                    // Because this is a private message, the bot should send a private message back
                    // A private message does NOT have a channel
                }
                if (e.Message.StartsWith("join"))
                {
                    if (!isbot) {
                        string inviteID = e.Message.Substring(e.Message.LastIndexOf('/') + 1);
                        // Thanks to LuigiFan (Developer of DiscordSharp) for this line of code!
                        client.AcceptInvite(inviteID);
                        e.Author.SendMessage("Joined your discord server!");
                        Console.WriteLine("Got join request from " + inviteID);
                    } else
                    {
                        e.Author.SendMessage("Please use this url instead!" +
                            "https://discordapp.com/oauth2/authorize?client_id=208042164234485771&scope=bot&permissions=0");
                    }
                }
            };


            client.MessageReceived += (sender, e) => // Channel message has been received
            {
                string[] messageText = e.MessageText.Split(' ');
                              
                switch (messageText[0].ToLower())
                {
                    case "!admin": CheckAdminStatus(e);
                        break;
                    case "!help": ShowHelp(e);
                        break;
                    case "!cat": ShowRandomCatPic(e);
                        break;
                    case "!updategame": SetRandomPlayingName(e);
                        break;
                    case "!status": AnnounceStatus();
                        break;
                    case "!updatestatus": LoadRealmStatus(1);
                        break;
                    case "!realminfo": ShowRealmInfo();
                        break;
                    case "!news": ShowGuildNews();
                        break;
                    case "!char": GetCharacterInfo(messageText);
                        break;
                }
            };
            #endregion
            
            #region Events - Environment
            //  This sends a message to every new channel on the server
            client.ChannelCreated += (sender, e) =>
                {
                    if (e.ChannelCreated.Type == ChannelType.Text)
                    {
                        e.ChannelCreated.SendMessage("Nice! a new channel has been created!");
                    }
                };

            //  When a user joins the server, send a message to them.
            client.UserAddedToServer += (sender, e) =>
                {
                    e.AddedMember.SendMessage("Welcome to my server! rules:");
                    e.AddedMember.SendMessage("1. be nice!");
                    e.AddedMember.SendMessage("- Guild Crier!");
                };

            //  Don't want messages to be removed? this piece of code will
            //  Keep messages for you. Remove if unused :)
            client.MessageDeleted += (sender, e) =>
                {
                    e.Channel.SendMessage("Removing messages has been disabled on this server!");
                    e.Channel.SendMessage("<@" + e.DeletedMessage.Author.ID + "> sent: " + e.DeletedMessage.Content.ToString());
                };
            #endregion
        
            #region Connect to discord
            try {
                // Make sure that IF something goes wrong, the user will be notified.
                // The SendLoginRequest should be called after the events are defined, to prevent issues.
                Console.WriteLine("Sending login request");
                client.SendLoginRequest();
                Console.WriteLine("Connecting client in separate thread");
                // Cannot convert from 'method group' to 'ThreadStart', so i removed threading
                // Pass argument 'true' to use .Net sockets.
                client.Connect();
                // Login request, and then connect using the discordclient i just made.
                Console.WriteLine("Client connected!");
            } catch (Exception e) {
                Console.WriteLine("Something went wrong!\n" + e.Message + "\nPress any key to close this window.");
            }

            // Done! your very own Discord bot is online!
            #endregion

            #region Console state
            // Load Timer
            //Timer t = new Timer(TimerCallback, null, 0, 600000);

            Console.ReadKey(); // If the user presses a key, the bot will shut down.
            Environment.Exit(0); // Make sure all threads are closed.
            #endregion

        }

        private static void GetCharacterInfo(string [] value)
        {
            CharacterProgression character = new CharacterProgression(value[1]);
            
            // TODO: get char info
            if(character.IsError)
            {
                client.SendMessageToChannel(value[1] + " does not exist.", client.GetChannelByName("general"));
            }
            else if (value[2].Equals("hks"))
            {
                client.SendMessageToChannel(value[1] + " has " + character.GetHonorableKills() + " honorable kills.", client.GetChannelByName("general"));
            }
        }
        #endregion

        #region Timers
        private static void TimerCallback(Object o)
        {
            // TODO: Function to allow automated status updates WIP
            //LoadRealmStatus();
            //AnnounceStatus();
            //// Display the date/time when this method got called.
            //Console.WriteLine("Status Annouced: " + DateTime.Now);
            //// Force a garbage collection to occur for this demo.
            //GC.Collect();
        }
        #endregion

        #region !commands methods
        private static void ShowGuildNews()
        {
            GuildNews news = new GuildNews();
            client.SendMessageToChannel(news.LatestGuildNews, client.GetChannelByName("general"));
        }

        private static void ShowRealmInfo()
        {
            client.SendMessageToChannel("Realm information: \n" +
                "Realm: " + realm.name + "\n" +
                "Locale: " + realm.locale + "\n" +
                "Population: " + realm.population + "\n" +
                "Queue: " + realm.queue + "\n" +
                "Status: " + realm.status + "\n" +
                "Timezone: " + realm.timezone + "\n" +
                "Type: " + realm.type
                , client.GetChannelByName("general"));
        }

        private static void LoadRealmStatus(int v)
        {
            LoadRealmStatus();
            client.SendMessageToChannel("Realm status updated.", client.GetChannelByName("general"));
        }

        private static void LoadRealmStatus()
        {
            WowApi api = new WowApi();

            RootObject obj = JsonConvert.DeserializeObject<RootObject>(api.GetJSONFromURI("https://us.api.battle.net/wow/realm/status?realms=" + Properties.Settings.Default.TargetRealm + "&locale=en_US&apikey="));

            foreach (var kvp in obj.realms)
            {
                realm.name = kvp.name;
                realm.status = kvp.status;
                realm.queue = kvp.queue;
                realm.battlegroup = kvp.battlegroup;
                realm.connected_realms = kvp.connected_realms;
                realm.locale = kvp.locale;
                realm.population = kvp.population;
                realm.timezone = kvp.timezone;
                realm.type = kvp.type;
                realm.lastUpdated = DateTime.Now;
            }
        }

        private static void AnnounceStatus()
        {
            string s = "offline";
            if (realm.status) s = "online";
            string q = "does not";
            if (realm.queue) q = "does";

            try
            {
                client.SendMessageToChannel(Properties.Settings.Default.GuildName + ", " + realm.name + " is currently " + s + " and " + q + " have a wait. - Updated " + realm.lastUpdated.ToShortTimeString(), client.GetChannelByName("general"));
                client.SendMessageToChannel("Type !help for a list of commands.", client.GetChannelByName("general"));
            }
            catch (Exception error) { Console.WriteLine(error.StackTrace); }
        }

        private static void SetRandomPlayingName(DiscordMessageEventArgs e)
        {
            Random r = new Random();
            string value = string.Empty;
            switch(new Random().Next(1,10))
            {
                case 1:
                    value = "World of Starcraft";
                    break;
                case 2:
                    value = "Diablo 4";
                    break;
                case 3:
                    value = "Hearthstone CCG";
                    break;
                case 4:
                    value = "Warcraft 4";
                    break;
                case 5:
                    value = "Overwatch";
                    break;
                case 6:
                    value = "America Online";
                    break;
                case 7:
                    value = "Pokemon Go";
                    break;
                case 8:
                    value = "League of Legends";
                    break;
                case 9:
                    value = "Hit 80s Records";
                    break;
                case 10:
                    value = "the Game";
                    break;
            }
            client.UpdateCurrentGame(value);
        }
               
        private static void ShowHelp(DiscordMessageEventArgs e)
        {
            string help = "";
            string[] helpList = { "To enter a command type an ! then the keyword.",
                "admin - check your chat admin status",
                "help - help derp", "cat - cat derp",
                "updategame - randomize my playing: tag",
                "status - check realm status",
                "updatestatus - force update realm status",
            "realminfo - info about "+Properties.Settings.Default.TargetRealm,
            "news - the top 5 guild news"};
            foreach(string s in helpList)
            {
                help += s+"\n";    
            }

            e.Channel.SendMessage(help);
        }

        private static void ShowRandomCatPic(DiscordMessageEventArgs e)
        {
            // People love this
            // All credit goes to https://github.com/NaamloosDT/DiscordSharp_Starter
            Thread t = new Thread(new ParameterizedThreadStart(randomcat));
            t.Start(e.Channel);
            string s;
            using (WebClient webclient = new WebClient())
            {
                s = webclient.DownloadString("http://random.cat/meow");
                int pFrom = s.IndexOf("\\/i\\/") + "\\/i\\/".Length;
                int pTo = s.LastIndexOf("\"}");
                string cat = s.Substring(pFrom, pTo - pFrom);
                webclient.DownloadFile("http://random.cat/i/" + cat, "cat.png");
                client.AttachFile(e.Channel, "meow!", "cat.png");
            }
        }

        private static void CheckAdminStatus(DiscordMessageEventArgs e)
        {
            // Example from https://github.com/NaamloosDT/DiscordSharp_Starter
            List<DiscordRole> roles = e.Author.Roles;

            foreach (DiscordRole role in roles)
            {
                if (role.Name.Contains("bosses"))
                {
                    e.Channel.SendMessage("Yes, you are an admin.");
                }
                else e.Channel.SendMessage("Sorry, not yet!");
            }      
        }

        public static void randomcat(object channel)
        {

        }
        #endregion

        #region JSON Realm Object
        public class Realm
        {
            public string type { get; set; }
            public string population { get; set; }
            public bool queue { get; set; }
            public bool status { get; set; }
            public string name { get; set; }
            public string slug { get; set; }
            public string battlegroup { get; set; }
            public string locale { get; set; }
            public string timezone { get; set; }
            public List<string> connected_realms { get; set; }
            public DateTime lastUpdated { get; set; }
        }

        public class RootObject
        {
            public List<Realm> realms { get; set; }
        }
        #endregion
    }
}
