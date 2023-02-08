using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using System;

namespace EasyOC.ReplaceAction
{
    /// <summary>
    /// ¿ØÖÆÆ÷ÖØÐ´
    /// </summary>
    public class Startup : StartupBase
    {
        public override int Order => -900;
        public override int ConfigureOrder => 1100;
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
