using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MvcApplication1.App_Start.StartUp))]
namespace MvcApplication1.App_Start

{
    public partial class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}