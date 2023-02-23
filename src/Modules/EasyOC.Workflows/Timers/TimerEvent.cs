using Microsoft.Extensions.Localization;
using NCrontab;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace EasyOC.Workflows.Timers
{
    public class TimerEvent : EventActivity
    {
        public static string EventName => nameof(TimerEvent);
        private readonly IClock _clock;
        private readonly IStringLocalizer S;
        private readonly ISiteService _siteService;

        public TimerEvent(IClock clock, IStringLocalizer<TimerEvent> localizer, ISiteService siteService)
        {
            _clock = clock;
            S = localizer;
            _siteService = siteService;
        }

        public override string Name => EventName;

        public override LocalizedString DisplayText => S["Timer Event"];

        public override LocalizedString Category => S["Background"];

        public string CronExpression
        {
            get => GetProperty(() => "*/5 * * * *");
            set => SetProperty(value);
        }
        public bool UseSiteTimeZone
        {
            get => GetProperty(() => false);
            set => SetProperty(value);
        }

        private DateTime? StartedTime
        {
            get => GetProperty<DateTime?>();
            set => SetProperty(value);
        }

        public override async Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return StartedTime == null || await IsExpired();
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override async Task<ActivityExecutionResult> ResumeAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            if (await IsExpired())
            {
                workflowContext.LastResult = "TimerEvent";
                return Outcomes("Done");
            }

            return Halt();
        }

        private async Task<bool> IsExpired()
        {
            DateTime when;
            var now = _clock.UtcNow;
            if (UseSiteTimeZone)
            {
                var timeZoneId = (await _siteService.GetSiteSettingsAsync()).TimeZoneId;
                now = _clock.ConvertToTimeZone(new DateTimeOffset(now), _clock.GetTimeZone(timeZoneId)).DateTime;
            }
            StartedTime ??= now;

            var schedule = CrontabSchedule.Parse(CronExpression);
            when = schedule.GetNextOccurrence(StartedTime.Value);
            return now >= when;
        }
    }
}
