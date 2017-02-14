using GraceBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;

namespace GraceBot
{
    internal interface IFactory
    {   
        IApp GetApp();
        IResponseManager GetResponseManager(string fileName);
        IFilter GetActivityFilter();
        IDbManager GetDbManager();
        ILuisManager GetLuisManager();
        ISlackManager GetQuestionSlackManager();
        ISlackManager GetExceptionSlackManager();
        IBotManager GetBotManager();
        ICommandManager GetCommandManager();
        IDialog<R> MakeIDialog<R>(DialogTypes dialogType);
        IAnswerManager GetAnswerManager();
        Dictionary<string, List<string>> GetResponseData(DialogTypes dialogType);
    }
}
