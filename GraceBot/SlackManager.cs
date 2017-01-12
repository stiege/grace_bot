using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using GraceBot.Models;
using Newtonsoft.Json;

namespace GraceBot
{
    public class SlackManager : ISlackManager
    {
        public async Task<bool> Forward(string msg)
        {
            var uri = Environment.GetEnvironmentVariable("WEBHOOK_URL");

            var payload = new Payload()
            {
                Text = msg,
                Channel = "#5-grace-questions",
                Username = "GraceBot_UserEnquiry",
            };

            var serializedPayload = JsonConvert.SerializeObject(payload);

            using (var client = new HttpClient())
            {
                var rsponse = await client.PostAsync(uri, new StringContent(serializedPayload, Encoding.UTF8, "application/json"));
                if (rsponse.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }

        }
    }
}