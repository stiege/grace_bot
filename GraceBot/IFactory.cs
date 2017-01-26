using GraceBot.Dialogs;
using System.Collections.Generic;

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
        GraceDialog<R> MakeGraceDialog<R>(DialogTypes dialogType);
        Dictionary<DialogTypes, List<string>> GetResponseData(DialogTypes dialogType);
    }
}
