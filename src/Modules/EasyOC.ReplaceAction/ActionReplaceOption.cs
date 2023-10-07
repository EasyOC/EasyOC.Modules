using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace EasyOC.ReplaceAction
{

    public class ActionReplaceOption
    {
        public List<ActionReplaceOptionItem> Items { get; set; } = new List<ActionReplaceOptionItem>();


        public List<ActionReplaceOptionItem> AddReplaceOption<TargetNew>(string targetControllerName, IDictionary<string, string> actionMapping, int order = 0)
            where TargetNew : class
        {
            var type = typeof(TargetNew);
            var typeInfo = type.GetTypeInfo();

            var _actionMapping = actionMapping.ToDictionary(k => k.Key, v => type.GetMethod(actionMapping[v.Key]));

            Items.Add(new ActionReplaceOptionItem
            {
                TargetControllerFullName = targetControllerName,
                NewController = type,
                ActionMapping = _actionMapping
            });
            return Items;
        }


        public List<ActionReplaceOptionItem> AddReplaceOption<TargetNew>(string targetControllerName, IDictionary<MethodDescription, MethodDescription> actionMapping)
         where TargetNew : class
        {
            var type = typeof(TargetNew);
            var typeInfo = type.GetTypeInfo();
            foreach (var kv in actionMapping)
            {
                var newMethod = kv.Value.Parameters != null ? typeInfo.GetMethod(kv.Value.Name, kv.Value.Parameters) :
                    typeInfo.GetMethod(kv.Value.Name);
                if (newMethod != null)
                {
                    var item = new ActionReplaceOptionItem
                    {
                        TargetControllerFullName = targetControllerName,
                        NewController = type,
                        TargetMethodDescriptions = new KeyValuePair<MethodDescription, MethodInfo>(kv.Key, newMethod)
                    };
                    Items.Add(item);
                }
            }

            return Items;
        }

        public List<ActionReplaceOptionItem> AddReplaceOption<SourceOld, TargetNew>(IDictionary<string, string> ActionMapping, int order = 0)
            where TargetNew : class
        {
            return AddReplaceOption<TargetNew>(typeof(SourceOld).FullName, ActionMapping, order);
        }
    }

    public class MethodDescription
    {
        public string Name { get; set; }
        /// <summary>
        /// 如果未指定参数则不检查
        /// </summary>
        public Type[] Parameters { get; set; }
    }
    public class ActionReplaceOptionItem
    {
        public int Order { get; set; } = 0;
        public Dictionary<string, MethodInfo> ActionMapping { get; set; }

        public KeyValuePair<MethodDescription, MethodInfo>? TargetMethodDescriptions { get; set; }
        public Type NewController { get; set; }
        public string TargetControllerFullName { get; set; }

        public Action<ControllerActionDescriptor> CustomAction { get; set; }
    }

}
