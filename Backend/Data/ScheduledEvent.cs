namespace Helios.Data;

public class ScheduledEvent
{
    public string TriggerName { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public DayOfWeek[] DayOfWeek { get; set; } = Array.Empty<DayOfWeek>();
    public TimeOnly? ActivationTime { get; set; }
}
