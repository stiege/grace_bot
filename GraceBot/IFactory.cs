using System;
using System.Threading.Tasks;

namespace GraceBot
{
    internal interface IFactory
    {
        IFilter GetActivityFilter();
        IHttpClient GetHttpClient();
        Task RespondAsync(string s, IExtendedActivity activity);
        IApp GetApp();
        IDefinition GetActivityDefinition();
        Action<IExtendedActivity> GetActivityPersistor();
    }
}