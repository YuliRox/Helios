using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using System.Text.RegularExpressions;

namespace Helios.Data;

public class ScheduledEventService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly Regex CronEx = new Regex(@"(?<Seconds>\d{2}) (?<Minutes>\d{2}) (?<Hours>\d{2}) ? * (?<WeekDays>.*)", RegexOptions.Compiled);


    public ScheduledEventService(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }
    public async Task<ScheduledEvent[]> GetScheduledAsync()
    {
        var scheduler = await _schedulerFactory.GetScheduler("Job Scheduler");

        if (scheduler == null)
            return Array.Empty<ScheduledEvent>();

        var scheduledEvents = new List<ScheduledEvent>();
        foreach (var triggerGroupName in await scheduler.GetTriggerGroupNames())
        {
            var triggerKeys = await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(triggerGroupName));
            if (triggerKeys == null || !triggerKeys.Any() || triggerKeys.Count != 2)
                continue;

            var triggers = triggerKeys
                .Select(triggerKey => scheduler.GetTrigger(triggerKey).Result)
                .Where(x => x is CronTriggerImpl)
                .Cast<CronTriggerImpl>()
                .ToArray()
                ;

            var startTrigger = triggers.Single(t => t.Name.EndsWith("On"));
            var stopTrigger = triggers.Single(t => t.Name.EndsWith("Off"));
            var (startTime, startWeekdays) = CronToType(startTrigger.CronExpressionString);
            var (stopTime, stopweekdays) = CronToType(stopTrigger.CronExpressionString);

            scheduledEvents.Add(new ScheduledEvent()
            {
                GroupName = triggerGroupName,
                ActivationTime = startTime,
                DeactivationTime = stopTime,
                DayOfWeek = startWeekdays,
                CronExpressionOn = startTrigger.CronExpressionString,
                CronExpressionOff = stopTrigger.CronExpressionString
            });
        }

        return scheduledEvents.ToArray();
    }

    private (TimeOnly, DayOfWeek[]) CronToType(string cronExpressionString)
    {
        var cronParts = CronEx.Match(cronExpressionString);

        var seconds = int.Parse(cronParts.Groups["Seconds"].Value);
        var minutes = int.Parse(cronParts.Groups["Minutes"].Value);
        var hours = int.Parse(cronParts.Groups["Hours"].Value);
        var time = new TimeOnly(hours, minutes, seconds);

        var weekDays = cronParts.Groups["WeekDays"].Value;

        return (time, new[] { DayOfWeek.Monday });
    }
}
