using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GuildBlaster_DiscordBot
{
    class WowApi
    {
        private string WowApiKey { get; set; }
        
        public WowApi()
        {
            WowApiKey = Properties.Settings.Default.WowApiKey;
        }

        public string GetJSONFromURI(string URI)
        {
            string jsonResult = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URI+WowApiKey);
            request.Method = WebRequestMethods.Http.Get;
            request.Accept = "application/json";
            
                var response = (HttpWebResponse)request.GetResponse();

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    jsonResult = sr.ReadToEnd();
                }
            
            
            
            return jsonResult;
        }
    }
}
