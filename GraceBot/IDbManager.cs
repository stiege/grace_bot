using GraceBot.Models;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraceBot
{
    internal interface IDbManager
    {
        Task AddActivity(Activity activity, ProcessStatus processStatus = ProcessStatus.BotMessage);
        Task UpdateActivity(Activity activity);
        Task UpdateActivity(Activity activity, ProcessStatus processStatus);

        List<Activity> FindUnprocessedQuestions();
        Activity FindActivity(string id);
    }
}
