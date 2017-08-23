using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MT.Startup))]
namespace MT
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
