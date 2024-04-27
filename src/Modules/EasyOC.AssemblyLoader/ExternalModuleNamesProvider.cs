using Microsoft.Extensions.Hosting;
using OrchardCore.Modules;
using System.Reflection;

namespace EasyOC.AssemblyLoader
{
    public class ExternalModuleNamesProvider : IModuleNamesProvider
    {
        private IEnumerable<string> _moduleNames;
        private static List<Assembly> _assemblies = [];
        public static List<Assembly> ExternalAssemblies => _assemblies;
        public ExternalModuleNamesProvider()
        {
            //_moduleNames = _assemblies.Select(x=>x.;
            _moduleNames = _assemblies.SelectMany(x =>
            {
                var modules = x.GetCustomAttributes<OrchardCore.Modules.Manifest.ModuleAttribute>();
                var names = modules.Select(modules => modules.Id).ToList();
                //var features = x.GetCustomAttributes<OrchardCore.Modules.Manifest.FeatureAttribute>();
                //names.AddRange(features.Select(f => f.Name).ToList());
                return names;
            }).Distinct().Where(x => !string.IsNullOrEmpty(x));
        }

        public IEnumerable<string> GetModuleNames()
        {
            return _moduleNames;
        }
    }
}
