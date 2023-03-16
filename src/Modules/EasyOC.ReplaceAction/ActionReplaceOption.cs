using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace EasyOC.ReplaceAction
{

    public class ActionReplaceOption
    {
        public List<ActionReplaceOptionItem> Items { get; set; } = new List<ActionReplaceOptionItem>();

        /// <summary>
        /// Find first Match
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public ActionReplaceOptionItem FindOption(MethodInfo method)
        {
            return Items.OrderBy(x => x.Order).FirstOrDefault(x => x.TargetControllerFullName.Equals(method.DeclaringType.FullName)
            && x.ActionMapping.ContainsKey(method.Name));
        }


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

        public List<ActionReplaceOptionItem> AddReplaceOption<SourceOld, TargetNew>(IDictionary<string, string> ActionMapping, int order = 0)
            where TargetNew : class
        {
            return AddReplaceOption<TargetNew>(typeof(SourceOld).FullName, ActionMapping, order);
        }
    }
    public class ActionReplaceOptionItem
    {
        public int Order { get; set; } = 0;
        public Dictionary<string, MethodInfo> ActionMapping { get; set; }
        public Type NewController { get; set; }
        public string TargetControllerFullName { get; set; }

        public Action<ControllerActionDescriptor> CustomAction { get; set; }
    }

}
