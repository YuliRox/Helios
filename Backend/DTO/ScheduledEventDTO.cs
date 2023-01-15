using System;

namespace Helios.DTO;
public class ScheduledEventDTO
{
    public string TriggerName { get; set; }
    public string CronExpression { get; set; }
    public string JobType { get; set; }
    public DayOfWeek[] DayOfWeek { get; set; }
    public TimeOnly ActivationTime { get; set; }
}
