using EasyOC.Deployment.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace EasyOC.Deployment.Deployment
{
    public class QueriesDeploymentStepDriver : DisplayDriver<DeploymentStep, QueriesDeploymentStep>
    {
        public override IDisplayResult Display(QueriesDeploymentStep step)
        {
            return
                Combine(
                    View("QueriesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("QueriesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(QueriesDeploymentStep step)
        {
            return Initialize<QueriesDeploymentStepViewModel>("QueriesDeploymentStep_Fields_Edit", model =>
            {
                model.Queries = step.Queries;
                model.IncludeAll = step.IncludeAll;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(QueriesDeploymentStep step, IUpdateModel updater)
        {
            // Initializes the value to empty otherwise the model is not updated if no type is selected.
            step.Queries = Array.Empty<string>();
            step.IncludeAll = false;

            await updater.TryUpdateModelAsync(
                step,
                Prefix,
                x => x.Queries, 
                x => x.IncludeAll);

            // don't have the selected option if include all
            if (step.IncludeAll)
            {
                step.Queries = Array.Empty<string>();                
            }
            else
            {
                step.Queries = step.Queries.Distinct().ToArray();
            }

            return Edit(step);
        }
    }
}
