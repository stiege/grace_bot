using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace GraceBot
{
    internal interface IFactory
    {
        Task<Activity> RespondAsync(string replyText, Activity originalActivity);
        Task<T> GetUserDataPropertyAsync<T>(string property, Activity activity);
        Task SetUserDataPropertyAsync<T>(string property, T data, Activity activity);
        Task<string[]> DeleteStateForUserAsync(Activity activity);

        IApp GetApp();
        IDefinition GetActivityDefinition();
        IFilter GetActivityFilter();
        IHttpClient GetHttpClient();
        IDbManager GetDbManager();
    }
}
