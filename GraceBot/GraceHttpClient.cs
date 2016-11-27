using System.Net.Http;
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
    }
}