using Microsoft.AspNetCore.Hosting;
using NetModular.Lib.Host.Web;

namespace NetModular.Module.Admin.WebHost
{
    public class Startup : StartupAbstract
    {
        public Startup(IHostingEnvironment env) : base(env)
        {
        }
    }
}
