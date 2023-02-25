using EasyOC.Deployment.Deployment;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;

namespace EasyOC.Deployment
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, QueriesDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<QueriesDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, QueriesDeploymentStepDriver>();

            services.AddTransient<IDeploymentSource, RolesDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<RolesDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, RolesDeploymentStepDriver>();
        }

    }
}
