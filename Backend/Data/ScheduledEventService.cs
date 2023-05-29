using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using System.Text.RegularExpressions;

namespace Helios.Data;

public class ScheduledEventService
{
    private readonly ISchedulerFactory _schedulerFactory;

        public ScheduledEventService(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }
    public async Task<ScheduledEvent[]> GetScheduledAsync()
    {
        //return Task.FromResult<ScheduledEvent[]>(new ScheduledEvent[]
        //{
        //    new()
        //    {
        //        TriggerName = "test",
        //        JobType = "test",
        //        ActivationTime = TimeOnly.FromDateTime(DateTime.Now),
        //        CronExpression = "* * 2",
        //        DayOfWeek = new[] { DayOfWeek.Sunday, DayOfWeek.Saturday },
        //    }
        //});
        var scheduler = await _schedulerFactory.GetScheduler("Job Scheduler");

        if (scheduler == null)
            return Array.Empty<ScheduledEvent>();

        //var groups = await scheduler.GetJobGroupNames();
        var keys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        var jobKey = keys.FirstOrDefault();
        if (jobKey == null)
            return Array.Empty<ScheduledEvent>();

        var triggers = await scheduler.GetTriggersOfJob(jobKey);

        var scheduledEvents = triggers
            .Where(x => x is CronTriggerImpl)
            .Cast<CronTriggerImpl>()
            .Select(t =>
        {
            var (time, weekdays) = CronToType(t.CronExpressionString);
            return new ScheduledEvent()
            {
                TriggerName = t.Name,
                JobType = t.JobName,

                ActivationTime = time,
                DayOfWeek = weekdays,

                CronExpression = t.CronExpressionString,
            };
        }).ToArray();

        triggers.Where(x => x is DailyTimeIntervalTriggerImpl)
            .Cast<DailyTimeIntervalTriggerImpl>()
            .Select(x => new ScheduledEvent()
            {
                TriggerName = x.Name,
                JobType = x.JobName,

                ActivationTime = new TimeOnly(x.StartTimeOfDay.Hour, x.StartTimeOfDay.Minute, x.StartTimeOfDay.Second),
                DayOfWeek = x.DaysOfWeek.ToArray()
            });

        return scheduledEvents;
    }

    private Regex CronEx = new Regex(@"(?<Seconds>\d{2}) (?<Minutes>\d{2}) (?<Hours>\d{2}) ? * (?<WeekDays>.*)", RegexOptions.Compiled);
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
