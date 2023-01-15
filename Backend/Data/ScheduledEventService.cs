namespace Helios.Data;

public class ScheduledEventService
{
    public Task<ScheduledEvent[]> GetScheduledAsync()
    {
        return Task.FromResult<ScheduledEvent[]>(new ScheduledEvent[]
        {
            new()
            {
                TriggerName = "test",
                JobType = "test",
                ActivationTime = TimeOnly.FromDateTime(DateTime.Now),
                CronExpression = "* * 2",
                DayOfWeek = new[] { DayOfWeek.Sunday, DayOfWeek.Saturday },
            }
        });
    }
}
