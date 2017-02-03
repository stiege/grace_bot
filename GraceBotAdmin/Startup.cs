using System;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GraceBotAdmin.Startup))]
namespace GraceBotAdmin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Custom datadirectory
            var path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            path=path.Replace("GraceBotAdmin", "GraceBot");
            AppDomain.CurrentDomain.SetData("DataDirectory", path);
        }
    }
}
