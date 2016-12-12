using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GraceBot
{
    internal interface IHttpClient : IDisposable
    {
        Task<HttpResponseMessage> GetAsync(string uri);
        Task<HttpResponseMessage> PostMessageAsync(string uri, Payload payload);
    }
}