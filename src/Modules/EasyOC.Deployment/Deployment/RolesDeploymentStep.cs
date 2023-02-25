using OrchardCore.Deployment;

namespace EasyOC.Deployment.Deployment
{
    /// <summary>
    /// Adds roles to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class RolesDeploymentStep : DeploymentStep
    {
        public RolesDeploymentStep()
        {
            Name = "Roles";
        }
        public bool IncludeAll { get; set; }

        public string[] Names { get; set; }
    }
}
