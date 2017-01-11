using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal interface IApp
    {
        /// <summary>
        /// To process an activity as an asynchronous operation.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        Task RunAsync(Activity activity);
    }
}
