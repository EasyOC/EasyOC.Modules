using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace EasyOC.ReplaceAction
{
    /// <summary>
    /// ¿ØÖÆÆ÷ÖØÐ´
    /// </summary>
    public class Startup : StartupBase
    {
        public override int ConfigureOrder => 10000;
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActionReplaceService();
        }
        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            serviceProvider.UseReplaceAction();
        }

    }

}
