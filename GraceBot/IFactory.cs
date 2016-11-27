using System;

namespace GraceBot
{
    internal interface IFactory
    {
        IFilter GetActivityFilter();
        IHttpClient GetHttpClient();
    }
}