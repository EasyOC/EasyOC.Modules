using OrchardCore.Deployment;

namespace EasyOC.Deployment.Deployment
{
    /// <summary>
    /// Adds all queries to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class QueriesDeploymentStep : DeploymentStep
    {
        public QueriesDeploymentStep()
        {
            Name = "Queries";
        }

        public bool IncludeAll { get; set; }

        public string[] Queries { get; set; }
    }
}
