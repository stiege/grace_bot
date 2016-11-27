using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    internal interface IApp
    {
        Task Run();
    }
}