using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace EasyOC.ReplaceAction
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddActionReplaceService(this IServiceCollection services)
        {
            services.AddOptions<ActionReplaceOption>();

            return services;
        }

        public static IServiceCollection ReplaceAction(this IServiceCollection services, Action<ActionReplaceOption> action)
        {
            services.Configure<ActionReplaceOption>(opt => action(opt));

            return services;
        }
        public static IServiceCollection ReplaceAction<TTarget, TNew>(this IServiceCollection services, string actionName, string newActionName = default)
        {
            if (newActionName is null)
            {
                newActionName = actionName;
            }
            return services.ReplaceAction(opt =>
             {
                 var type = typeof(TNew);

                 opt.Items.Add(new ActionReplaceOptionItem
                 {
                     TargetControllerFullName = typeof(TTarget).FullName,
                     NewController = type,
                     ActionMapping = new Dictionary<string, MethodInfo> { [actionName] = type.GetMethod(newActionName) }
                 });
             });
        }
        public static IServiceCollection ReplaceAction<TNew>(this IServiceCollection services, string targetControllerFullName, string actionName, string newActionName = default)
        {
            if (newActionName is null)
            {
                newActionName = actionName;
            }
            return services.ReplaceAction(opt =>
            {
                var type = typeof(TNew);

                opt.Items.Add(new ActionReplaceOptionItem
                {
                    TargetControllerFullName = targetControllerFullName,
                    NewController = type,
                    ActionMapping = new Dictionary<string, MethodInfo> { [actionName] = type.GetMethod(newActionName) }
                });
            });
        }
        public static IServiceProvider UseReplaceAction(this IServiceProvider serviceProvider)
        {
            var rOptions = serviceProvider.GetRequiredService<IOptions<ActionReplaceOption>>();
            var config = rOptions.Value;

            var descriptors = serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>()
              .ActionDescriptors.Items
              .OfType<ControllerActionDescriptor>()
              .ToArray();

            foreach (var item in config.Items.OrderBy(x => x.Order))
            {
                foreach (var descriptor in descriptors)
                {
                    if (descriptor.ControllerTypeInfo.FullName == item.TargetControllerFullName &&
                       item.ActionMapping.ContainsKey(descriptor.ActionName)
                        )
                    {
                        descriptor.ControllerTypeInfo = item.NewController.GetTypeInfo();
                        descriptor.MethodInfo = item.ActionMapping[descriptor.ActionName];
                    }
                }
            }
            return serviceProvider;
        }
    }
}
