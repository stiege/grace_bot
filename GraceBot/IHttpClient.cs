using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GraceBot
{
    public interface IHttpClient : IDisposable
    {
        Task<HttpResponseMessage> GetAsync(string uri);
    }
}