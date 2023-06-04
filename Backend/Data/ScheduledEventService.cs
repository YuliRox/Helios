using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using System.Text.RegularExpressions;

namespace Helios.Data;

public class ScheduledEventService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly Regex CronEx = new Regex(@"(?<Seconds>\d{1,2}) (?<Minutes>\d{1,2}) (?<Hours>\d{1,2}) \? \* (?<WeekDays>.*)", RegexOptions.Compiled);
    private const string SchedName = "Job Scheduler";

    public ScheduledEventService(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }
    public async Task<ScheduledEvent[]> GetScheduledAsync()
    {
        var scheduler = await _schedulerFactory.GetScheduler(SchedName);

        if (scheduler == null)
            return Array.Empty<ScheduledEvent>();

        var scheduledEvents = new List<ScheduledEvent>();
        var pausedGroups = await scheduler.GetPausedTriggerGroups();

        foreach (var triggerGroupName in await scheduler.GetTriggerGroupNames())
        {
            var triggerKeys = await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(triggerGroupName));
            if (triggerKeys == null || !triggerKeys.Any() || triggerKeys.Count != 2)
                continue;

            var triggers = triggerKeys
                .Select(triggerKey => scheduler.GetTrigger(triggerKey).Result)
                .Where(x => x is CronTriggerImpl)
                .Cast<CronTriggerImpl>()
                .ToArray();

            var startTrigger = triggers.Single(t => t.Name.EndsWith("On"));
            var stopTrigger = triggers.Single(t => t.Name.EndsWith("Off"));
            var (startTime, startWeekdays) = CronToType(startTrigger.CronExpressionString);
            var (stopTime, stopweekdays) = CronToType(stopTrigger.CronExpressionString);

            var isActive = !pausedGroups.Contains(triggerGroupName);

            scheduledEvents.Add(new ScheduledEvent()
            {
                GroupName = triggerGroupName,
                ActivationTime = startTime,
                DeactivationTime = stopTime,
                DayOfWeek = startWeekdays,
                CronExpressionOn = startTrigger.CronExpressionString,
                CronExpressionOff = stopTrigger.CronExpressionString,
                IsActive = isActive
            });
        }

        return scheduledEvents.ToArray();
    }

    public async Task TogglePauseStatus(ScheduledEvent scheduledEvent)
    {
        var scheduler = await _schedulerFactory.GetScheduler(SchedName);
        if (scheduler == null)
            return;

        var pausedGroups = await scheduler.GetPausedTriggerGroups();

        if (!pausedGroups.Contains(scheduledEvent.GroupName))
        {
            await scheduler.PauseTriggers(GroupMatcher<TriggerKey>.GroupEquals(scheduledEvent.GroupName));
            scheduledEvent.IsActive = false;
        }
        else
        {
            await scheduler.ResumeTriggers(GroupMatcher<TriggerKey>.GroupEquals(scheduledEvent.GroupName));
            scheduledEvent.IsActive = true;
        }
    }

    public async Task UpdateTimes(ScheduledEvent scheduledEvent)
    {
        var scheduler = await _schedulerFactory.GetScheduler(SchedName);
        if (scheduler == null)
            return;

        var triggerKeys = await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(scheduledEvent.GroupName));
        if (triggerKeys == null || !triggerKeys.Any() || triggerKeys.Count != 2)
            return;

        var triggers = triggerKeys
                .Select(triggerKey => scheduler.GetTrigger(triggerKey).Result)
                .Where(x => x is CronTriggerImpl)
                .Cast<CronTriggerImpl>()
                .ToArray();

        var startTrigger = triggers.Single(t => t.Name.EndsWith("On"));
        var stopTrigger = triggers.Single(t => t.Name.EndsWith("Off"));

        startTrigger.CronExpressionString = ReplaceExpressionParts(startTrigger.CronExpressionString, scheduledEvent.ActivationTime);
        stopTrigger.CronExpressionString = ReplaceExpressionParts(stopTrigger.CronExpressionString, scheduledEvent.DeactivationTime);

        await scheduler.RescheduleJob(startTrigger.Key, startTrigger);
        await scheduler.RescheduleJob(stopTrigger.Key, stopTrigger);
    }

    private string ReplaceExpressionParts(string? existingExpression, TimeOnly time)
    {
        if (existingExpression == null)
            throw new ArgumentNullException(nameof(existingExpression));
        return CronEx.Replace(existingExpression, m =>
            $"{time.Second:d2} {time.Minute:d2} {time.Hour:d2} ? * {m.Groups["WeekDays"].Value}"
        );
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
