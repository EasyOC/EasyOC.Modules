using EasyOC.Workflows.Timers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Workflows.Options;
using OCTimerEvent = OrchardCore.Workflows.Timers.TimerEvent;
namespace EasyOC.Workflows
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {

        }

    }
    [Feature(Constants.TimersFeautreId)]
    [RequireFeatures("OrchardCore.Workflows.Timers")]
    public class TimerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<WorkflowOptions>(options =>
            {
                if (options.IsActivityRegistered<OCTimerEvent>())
                {
                    options.UnregisterActivityType<OCTimerEvent>();
                }
                options.RegisterActivity<TimerEvent, TimerEventDisplayDriver>();
            });

        }
    }
}
