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
    internal class SlackManager : ISlackManager
    {
        private string _uri;
        private string _channel;
        private string _userName;

        private SlackManager() { }
        internal SlackManager(string uri, string channel, string userName)
        {
            _uri = uri;
            _channel = channel;
            _userName = userName;
        }
        
        public async Task<bool> ForwardMessageAsync(string msg)
        {
            var payload = new Payload()
            {
                Text = msg,
                Channel = _channel,
                Username = _userName,
            };

            var serializedPayload = JsonConvert.SerializeObject(payload);
            using (var client = new HttpClient())
            {
                var rsponse = await client.PostAsync(_uri, 
                    new StringContent(serializedPayload, Encoding.UTF8, "application/json"));
                if (rsponse.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }

        }
    }
}