using DiscordDotNet = Discord.DiscordClient;
using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using DiscordSharp_Starter;
using NAudio.Wave;
using System.Linq;

namespace GuildBlaster_DiscordBot
{
    class Program
    {
        #region Class Vars
        private static DiscordDotNet _client;
        private static Realm realm;
        #endregion

        #region Main
        static void Main(string[] args) => new Program().Start();

        public void Start()
        {

            #region Bot Startup
            Console.WriteLine("Starting..");
            Console.WriteLine("Create client..");
            _client = new DiscordDotNet();
            Console.WriteLine("done..");

            Console.WriteLine("SetRealm..");
            realm = new Realm();
            Console.WriteLine("done..");

            // Get Realm Info
            Console.WriteLine("Load realm status from blizzard api..");
            LoadRealmStatus();
            Console.WriteLine("done..");
            #endregion
            
            #region Events - General
            Console.WriteLine("Waiting on input..");
            _client.MessageReceived += async (s, e) =>
             {
                 if (!e.Message.IsAuthor)
                 {
                     string[] messageText = e.Message.Text.Split(' ');

                     switch (messageText[0].ToLower())
                     {
                         // TODO: method needs fixing
                         //case "!admin":
                         //    CheckAdminStatus(e);
                         //    break;
                         case "!help":
                             await e.Channel.SendMessage(ShowHelp());
                             break;
                         case "!cat":
                             ShowRandomCatPic(e);
                             break;
                         case "!updategame":
                             SetRandomPlayingName();
                             break;
                         case "!status":
                             await e.Channel.SendMessage(AnnounceStatus());
                             break;
                         case "!updatestatus":
                             LoadRealmStatus(1);
                             break;
                         case "!realminfo":
                             await e.Channel.SendMessage(ShowRealmInfo());
                             break;
                         case "!news":
                             await e.Channel.SendMessage(ShowGuildNews());
                             break;
                         case "!char":
                             await e.Channel.SendMessage(GetCharacterInfo(messageText));
                             break;
                         case "!mrbigglesworth":
                             await e.Channel.SendMessage("*You enter Naxxramas and immediately strike down Mr. Bigglesworth.*");
                             SendSoundToVoice();
                             Console.WriteLine("Mr. Bigglesworth Played");
                             break;
                     }
                 }

             };

            _client.ExecuteAndWait(async () => {
                await _client.Connect(Properties.Settings.Default.DiscordBotToken);
            });
            
            #endregion
            
            #region Events - Environment
            //  This sends a message to every new channel on the server
            //client.ChannelCreated += (sender, e) =>
            //    {
            //        if (e.ChannelCreated.Type == ChannelType.Text)
            //        {
                        
            //            e.ChannelCreated.SendMessage("Nice! a new channel has been created!");
            //        }
            //    };

            ////  When a user joins the server, send a message to them.
            //client.UserAddedToServer += (sender, e) =>
            //    {
            //        e.AddedMember.SendMessage("Welcome to my server! rules:");
            //        e.AddedMember.SendMessage("1. be nice!");
            //        e.AddedMember.SendMessage("- Guild Crier!");
            //    };

            ////  Don't want messages to be removed? this piece of code will
            ////  Keep messages for you. Remove if unused :)
            //client.MessageDeleted += (sender, e) =>
            //    {
            //        e.Channel.SendMessage("Removing messages has been disabled on this server!");
            //        e.Channel.SendMessage("<@" + e.DeletedMessage.Author.ID + "> sent: " + e.DeletedMessage.Content.ToString());
            //    };
            #endregion
        
            #region Connect to discord
            //try {
            //    // Make sure that IF something goes wrong, the user will be notified.
            //    // The SendLoginRequest should be called after the events are defined, to prevent issues.
            //    Console.WriteLine("Sending login request");
            //    client.SendLoginRequest();
            //    Console.WriteLine("Connecting client in separate thread");
            //    // Cannot convert from 'method group' to 'ThreadStart', so i removed threading
            //    // Pass argument 'true' to use .Net sockets.
            //    client.Connect();
            //    // Login request, and then connect using the discordclient i just made.
            //    Console.WriteLine("Client connected!");
            //} catch (Exception e) {
            //    Console.WriteLine("Something went wrong!\n" + e.Message + "\nPress any key to close this window.");
            //}

            // Done! your very own Discord bot is online!
            #endregion

            #region Console state
            // Load Timer
            //Timer t = new Timer(TimerCallback, null, 0, 600000);

            //Console.ReadKey(); // If the user presses a key, the bot will shut down.
            //Environment.Exit(0); // Make sure all threads are closed.
            #endregion

        }
        // TODO: rearrange methods
        private static void SendSoundToVoice(/*string soundName*/)
        {
            string fileName = Properties.Settings.Default.SoundsPath + @"creature\KelThuzad\" + @"NA_KelThuzad_BigglesworthDies.mp3";

            SendVoice(fileName, _client);

        }

        private static void SendAudio(string fileName)
        {     
            
        }
               
        private static async void SendVoice(string filePath, DiscordDotNet _client)
        {
            _client.UsingAudio(x => // Opens an AudioConfigBuilder so we can configure our AudioService
            {
                x.Mode = AudioMode.Outgoing; // Tells the AudioService that we will only be sending audio
            });

            var voiceChannel = _client.FindServers("proper villains").FirstOrDefault().VoiceChannels.FirstOrDefault(); // Finds the first VoiceChannel on the server 'Music Bot Server'

            var _vClient = await _client.GetService<AudioService>() // We use GetService to find the AudioService that we installed earlier. In previous versions, this was equivelent to _client.Audio()
                    .Join(voiceChannel); // Join the Voice Channel, and return the IAudioClient.

            var channelCount = _client.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
            var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
            using (var MP3Reader = new Mp3FileReader(filePath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
            using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
            {
                resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                byte[] buffer = new byte[blockSize];
                int byteCount;

                while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                {
                    if (byteCount < blockSize)
                    {
                        // Incomplete Frame
                        for (int i = byteCount; i < blockSize; i++)
                            buffer[i] = 0;
                    }
                    _vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                }
            }

            //await _vClient.VoiceSocket.Disconnect();            
        }

        private static byte[] ReadFromFile(string fileName)
        {
            MemoryStream ms = new MemoryStream();
            using (FileStream source = File.Open(fileName, FileMode.Open))
            {
                source.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static string GetCharacterInfo(string [] command)
        {
            CharacterProgression character = new CharacterProgression(command[1]);
                        
            if(character.IsError)
            {                
                return command[1] + " does not exist.";
            }
            else /*if (value[2].Equals("hks"))*/
            {
                return command[1] + " has " + character.GetHonorableKills() + " honorable kills.";
            }
        }
        #endregion
        
        #region !commands methods
        private static string ShowGuildNews()
        {
            GuildNews news = new GuildNews();
            return news.LatestGuildNews;
        }

        private static string ShowRealmInfo()
        {
            return "Realm information: \n" +
                "Realm: " + realm.name + "\n" +
                "Locale: " + realm.locale + "\n" +
                "Population: " + realm.population + "\n" +
                "Queue: " + realm.queue + "\n" +
                "Status: " + realm.status + "\n" +
                "Timezone: " + realm.timezone + "\n" +
                "Type: " + realm.type
                ;
        }

        private static void LoadRealmStatus(int v)
        {
            LoadRealmStatus();
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

        private static string AnnounceStatus()
        {
            string s = "offline";
            if (realm.status) s = "online";
            string q = "does not";
            if (realm.queue) q = "does";

            try
            {
                return Properties.Settings.Default.GuildName + ", " + realm.name + " is currently " + s + " and " + q + " have a wait. - Updated " + realm.lastUpdated.ToShortTimeString()+"\n"+
                "Type !help for a list of commands.";
            }
            catch (Exception error) { Console.WriteLine(error.StackTrace); }

            // TODO: Handle better
            return "error";
        }

        private static void SetRandomPlayingName()
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
            _client.SetGame(new Discord.Game(value));
        }
               
        private static string ShowHelp()
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

            return help;
        }

        private static void ShowRandomCatPic(Discord.MessageEventArgs e)
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
                e.Channel.SendMessage("Meow!");
                e.Channel.SendFile("cat.png");
            }
        }

        private static void CheckAdminStatus(Discord.MessageEventArgs e)
        {
            // Example from https://github.com/NaamloosDT/DiscordSharp_Starter
            //List<DiscordRole> roles = e.Server.Roles.ToList<DiscordRole>;

            foreach (Discord.Role role in e.Server.Roles)
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
