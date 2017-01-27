using GraceBot.Dialogs;
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
        GraceDialog<R> MakeGraceDialog<R>(DialogTypes dialogType);
        Dictionary<DialogTypes, List<string>> GetResponseData(DialogTypes dialogType);
    }
}
