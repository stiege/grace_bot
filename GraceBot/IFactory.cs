using GraceBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;

namespace GraceBot
{
    internal interface IFactory
    {   
        IApp GetApp();
        ILocalJsonManager GetDefinitionManager();
        IFilter GetActivityFilter();
        IDbManager GetDbManager();
        ILuisManager GetLuisManager();
        ISlackManager GetSlackManager();
        IBotManager GetBotManager();
        ICommandManager GetCommandManager();
        ILocalJsonManager GetAutoReplyHomeManager();
        IDialog<R> MakeIDialog<R>(DialogTypes dialogType);
        Dictionary<string, List<string>> GetResponseData(DialogTypes dialogType);
    }
}
