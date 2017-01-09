using Microsoft.Bot.Connector;
using System.Threading.Tasks;

namespace GraceBot
{
    internal interface IFilter
    {
        Task<bool> FilterAsync(Activity activity);
    }
}
