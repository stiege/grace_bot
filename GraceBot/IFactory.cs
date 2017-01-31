using GraceBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;

namespace GraceBot
{
    internal interface IFactory
    {   
        IApp GetApp();
        IResponseManager GetResponseManager(string fileName);
        IDefinition GetActivityDefinition();
        IFilter GetActivityFilter();
        IDbManager GetDbManager();
        ILuisManager GetLuisManager();
        ISlackManager GetSlackManager();
        IBotManager GetBotManager();
        ICommandManager GetCommandManager();
        IDialog<R> MakeIDialog<R>(DialogTypes dialogType);
        Dictionary<string, List<string>> GetResponseData(DialogTypes dialogType);
    }
}
