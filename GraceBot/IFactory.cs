using GraceBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using System;
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

        [Obsolete("Please use IDialog<IDialogResult> MakeIDialog(DialogTypes dialogType) or" + 
            "MakeRootDialog() instead.")]
        IDialog<R> MakeIDialog<R>(DialogTypes dialogTypes);
        IDialog<IDialogResult> MakeIDialog(DialogTypes dialogType);
        IDialog<object> MakeRootDialog();

        IAnswerManager GetAnswerManager();
        Dictionary<string, List<string>> GetResponseData(DialogTypes dialogType);
    }
}
