using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Queries;

namespace EasyOC.Deployment.Deployment
{
    public class QueriesDeploymentSource : IDeploymentSource
    {
        private readonly IQueryManager _queryManager;

        public QueriesDeploymentSource(IQueryManager queryManager)
        {
            _queryManager = queryManager;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var queriesStep = step as QueriesDeploymentStep;

            if (queriesStep == null)
            {
                return;
            }

            var queries = await _queryManager.ListQueriesAsync();

            result.Steps.Add(new JObject(
                new JProperty("name", "Queries"),
                new JProperty("Queries", queries.
                WhereIf(!queriesStep.IncludeAll, x => queriesStep.Queries.Contains(x.Name))
                .Select(JObject.FromObject))
            ));
        }
    }
}
