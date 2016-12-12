using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GraceBot
{
    internal class GraceHttpClient : IHttpClient
    {
        private readonly HttpClient _client;

        public GraceHttpClient(HttpClient client)
        {
            _client = client;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public Task<HttpResponseMessage> GetAsync(string uri)
        {
            return _client.GetAsync(uri);
        }

        public Task<HttpResponseMessage> PostMessageAsync(string uri, Payload payload)
        {
            var serializedPayload = JsonConvert.SerializeObject(payload);
            return _client.PostAsync(uri, new StringContent(serializedPayload, Encoding.UTF8, "application/json"));
        }
    }
}