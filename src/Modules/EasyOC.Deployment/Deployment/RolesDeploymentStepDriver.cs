using EasyOC.Deployment.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace EasyOC.Deployment.Deployment
{
    public class RolesDeploymentStepDriver : DisplayDriver<DeploymentStep, RolesDeploymentStep>
    {
        public override IDisplayResult Display(RolesDeploymentStep step)
        {
            return
                Combine(
                    View("RolesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("RolesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }
        public override IDisplayResult Edit(RolesDeploymentStep step)
        {
            return Initialize<RolesDeploymentStepViewModel>("RolesDeploymentStep_Fields_Edit", model =>
            {
                model.Names = step.Names;
                model.IncludeAll = step.IncludeAll;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(RolesDeploymentStep step, IUpdateModel updater)
        {
            // Initializes the value to empty otherwise the model is not updated if no type is selected.
            step.Names = Array.Empty<string>();
            step.IncludeAll = false;

            await updater.TryUpdateModelAsync(
                step,
                Prefix,
                x => x.Names,
                x => x.IncludeAll);

            // don't have the selected option if include all
            if (step.IncludeAll)
            {
                step.Names = Array.Empty<string>();
            }
            else
            {
                step.Names = step.Names.Distinct().ToArray();
            }

            return Edit(step);
        }
    }
}
