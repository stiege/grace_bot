using Microsoft.Bot.Builder.Dialogs;
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
        GraceDialog<T> GetGraceDialog<T>(string dialogName);
        Dictionary<string, List<string>> GetResponseData(string contextOrDialogName);
    }
}
