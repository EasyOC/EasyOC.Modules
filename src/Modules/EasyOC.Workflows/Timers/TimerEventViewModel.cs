using System.ComponentModel.DataAnnotations;

namespace EasyOC.Workflows.Timers
{
    public class TimerEventViewModel
    {
        [Required]
        public string CronExpression { get; set; }

        public bool UseSiteTimeZone { get; set; }
    }
}
