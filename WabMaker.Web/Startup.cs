using Microsoft.Owin;
using Owin;
using WabMaker.Web.Helpers;

[assembly: OwinStartupAttribute(typeof(WabMaker.Web.Startup))]
namespace WabMaker.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
