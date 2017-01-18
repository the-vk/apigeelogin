using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ApigeeLogin.Startup))]
namespace ApigeeLogin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
