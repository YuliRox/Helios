using System;
using Helios.Enum;

namespace Helios.DTO{
    public class ScheduledEventDTO
    {
        public string TriggerName { get; set; }
        public string CronExpression { get; set; }
        public string JobType { get; set; }
        public TimeOnly ActivationTime { get; set; }
        public Weekday[] DayOfWeek { get; set; }
    }
}