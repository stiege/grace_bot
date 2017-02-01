using System.Net.Http;
using System.Threading.Tasks;

namespace GraceBot
{
    public interface ISlackManager
    {
        /// <summary>
        /// Forward a text message to a specified slack channel.
        /// </summary>
        /// 
        /// <param name="msg">The message to be forwarded</param>
        /// 
        /// <returns>True if the message is forwarded successfully</returns>
        Task<bool> ForwardMessageAsync(string msg);
    }
}