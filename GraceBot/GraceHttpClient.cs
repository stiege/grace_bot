using GraceBot.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GraceBot
{
    internal class GraceHttpClient : IHttpClient
    {
        private readonly HttpClient _client;

        // constructor
        public GraceHttpClient(HttpClient client)
        {
            _client = client;
        }

        // Releases the unmanaged resources and disposes of the managed resources used by
        // the System.Net.Http.HttpMessageInvoker.
        public void Dispose()
        {
            _client.Dispose();
        }

        // Send a GET request to the specified Uri as an asynchronous operation.
        public Task<HttpResponseMessage> GetAsync(string uri)
        {
            return _client.GetAsync(uri);
        }
        
        // Send a POST request to the specified Uri as an asynchronous operation.
        public Task<HttpResponseMessage> PostMessageAsync(string uri, Payload payload)
        {
            var serializedPayload = JsonConvert.SerializeObject(payload);
            return _client.PostAsync(uri, new StringContent(serializedPayload, Encoding.UTF8, "application/json"));
        }
    }
}
