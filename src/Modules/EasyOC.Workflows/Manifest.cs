using OrchardCore.Modules.Manifest;
using static EasyOC.Constants.ManifestConstants;
[assembly: Module(
    Author = Author,
    Website = Website,
    Version = CurrentVersion,
    Description = "EasyOC.Workflows",
    Category = "Workflows"
)]
[assembly: Feature(
    Id = EasyOC.Workflows.Constants.TimersFeautreId,
    //Name = "Timer Workflow Activty (Support site time zone)",
    Dependencies = new[] {
        "OrchardCore.Workflows.Timers" },
    Description = "Make Timer Workflow Activty support site time zone.",
    Category = "Workflows"
)]