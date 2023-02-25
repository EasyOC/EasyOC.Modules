using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Roles.Recipes;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace EasyOC.Deployment.Deployment
{
    public class RolesDeploymentSource : IDeploymentSource
    {
        private readonly RoleManager<IRole> _roleManager;
        private readonly IRoleService _roleService;

        public RolesDeploymentSource(RoleManager<IRole> roleManager, IRoleService roleService)
        {
            _roleManager = roleManager;
            _roleService = roleService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var rolesStep = step as RolesDeploymentStep;

            if (rolesStep == null)
            {
                return;
            }

            // Get all roles
            var roles = await _roleService.GetRolesAsync();
            var permissions = new JArray();
            var tasks = new List<Task>();
            roles = roles.WhereIf(!rolesStep.IncludeAll, x => rolesStep.Names.Contains(x.RoleName));
            foreach (var role in roles)
            {
                var currentRole = (Role)await _roleManager.FindByNameAsync(_roleManager.NormalizeKey(role.RoleName));

                if (currentRole != null)
                {
                    permissions.Add(JObject.FromObject(
                        new RolesStepRoleModel
                        {
                            Name = currentRole.RoleName,
                            Description = currentRole.RoleDescription,
                            Permissions = currentRole.RoleClaims.Where(x => x.ClaimType == Permission.ClaimType).Select(x => x.ClaimValue).ToArray()
                        }));
                }
            }

            result.Steps.Add(new JObject(
                new JProperty("name", "Roles"),
                new JProperty("Roles", permissions)
            ));
        }
    }
}
