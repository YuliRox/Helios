using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Helios.DTO;
using Quartz;
using System.Linq;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using System.Text.RegularExpressions;

namespace Helios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulingController : ControllerBase
    {
        public ISchedulerFactory SchedulerFactory { get; }

        public SchedulingController(ISchedulerFactory schedulerFactory)
        {
            SchedulerFactory = schedulerFactory;
        }

        // GET: api/Scheduling
        [HttpGet("GetScheduledEvents")]
        public async Task<IEnumerable<ScheduledEventDTO>> GetScheduledEvents()
        {
            var schedulers = await SchedulerFactory.GetAllSchedulers();
            var scheduler = schedulers.FirstOrDefault();
            if (scheduler == null)
                return Enumerable.Empty<ScheduledEventDTO>();

            //var groups = await scheduler.GetJobGroupNames();
            var keys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            var jobKey = keys.FirstOrDefault();
            if (jobKey == null)
                return Enumerable.Empty<ScheduledEventDTO>();

            var triggers = await scheduler.GetTriggersOfJob(jobKey);

            var scheduledEvents = triggers
                .Where(x => x is CronTriggerImpl)
                .Cast<CronTriggerImpl>()
                .Select(t =>
            {
                var (time, weekdays) = CronToType(t.CronExpressionString);
                return new ScheduledEventDTO()
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
                .Select(x => new ScheduledEventDTO()
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

        [HttpPost("RunNow")]
        public async Task RunNow(string jobName)
        {
            var schedulers = await SchedulerFactory.GetAllSchedulers();
            var scheduler = schedulers.FirstOrDefault();
            if (scheduler == null)
                return;

            var jobKey = new JobKey(jobName);
            await scheduler.TriggerJob(jobKey);
        }

        public async Task CreateTrigger(ScheduledEventDTO scheduledEventDTO)
        {
            

            throw new NotImplementedException();
        }

        public async Task UpdateTrigger(ScheduledEventDTO scheduledEventDTO)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteTrigger(ScheduledEventDTO scheduledEventDTO)
        {
            if (string.IsNullOrWhiteSpace(scheduledEventDTO.TriggerName))
                throw new ArgumentException("Scheduled Event TriggerName is empty");
            if (string.IsNullOrWhiteSpace(scheduledEventDTO.JobType))
                throw new ArgumentException("Scheduled Event TriggerName is empty");

            throw new NotImplementedException();
        }
    }
}