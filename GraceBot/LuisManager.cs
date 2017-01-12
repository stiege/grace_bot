using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using GraceBot.Models;
using Newtonsoft.Json;

namespace GraceBot
{
    public class LuisManager:ILuisManager
    {
        public async Task<LuisResponse> GetResponse(string activityText)
        {
            var strEscaped = Uri.EscapeUriString(activityText);
            var uri =
                "https://api.projectoxford.ai/luis/v2.0/apps/" + Environment.GetEnvironmentVariable("LUIS_ID") +
                "?subscription-key=" +
                Environment.GetEnvironmentVariable("LUIS_KEY") + "&q=" +
                strEscaped + "&verbose=true";

            using (var client = new HttpClient())
            {
                var msg = await client.GetAsync(uri);
                if (msg.IsSuccessStatusCode)
                {
                    var response = JsonConvert.DeserializeObject<LuisResponse>(
                        await msg.Content.ReadAsStringAsync());
                    return response;
                }
                return null;
            }
           
        }
    }
}