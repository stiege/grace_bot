using Microsoft.Bot.Connector;
using System.Threading.Tasks;

namespace GraceBot
{
    internal interface IFilter
    {
        /// <summary>
        /// Analyse whether an activity satisfies a certain condition as an asynchronous operation.
        /// </summary>
        /// <param name="activity">An activity to be analysed.</param>
        /// <returns></returns>
        Task<string> FilterAsync(Activity activity);
    }
}
