namespace Helios.Data;

public class ScheduledEvent
{
    public string GroupName { get; set; } = string.Empty;
    public string CronExpressionOn { get; set; } = string.Empty;
    public string CronExpressionOff { get; set; } = string.Empty;
    public DayOfWeek[] DayOfWeek { get; set; } = Array.Empty<DayOfWeek>();
    public TimeOnly ActivationTime { get; set; }
    public TimeOnly DeactivationTime { get; set; }
    public bool IsActive { get; set; }
}
