using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebAssignment.Startup))]
namespace WebAssignment
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
