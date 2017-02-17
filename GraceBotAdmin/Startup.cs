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


        }
    }
}
