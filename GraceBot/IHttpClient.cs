using GraceBot.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GraceBot
{
    internal interface IHttpClient : IDisposable
    {
        /// <summary>
        /// Send a GET request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <param name="uri">A specified Uri to send a GET request.</param>
        /// <returns></returns>
        Task<HttpResponseMessage> GetAsync(string uri);

        /// <summary>
        /// Send a POST request to the specified Uri as an asynchronous operation. 
        /// </summary>
        /// <param name="uri">A specified Uri to send a POST request.</param>
        /// <param name="payload"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> PostMessageAsync(string uri, Payload payload);
    }
}
