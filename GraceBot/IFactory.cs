using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace GraceBot
{
    internal interface IFactory
    {   

        IApp GetApp();

        IDefinition GetActivityDefinition();

        IFilter GetActivityFilter();

        IDbManager GetDbManager();

        ILuisManager GetLuisManager();

        ISlackManager GetSlackManager();

        IBotManager GetBotManager();
        ICommandManager GetCommandManager();
    }
}
