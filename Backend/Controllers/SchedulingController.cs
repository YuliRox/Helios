using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Helios.DTO;
using Quartz;
using System.Linq;
using Quartz.Impl.Matchers;

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

            var scheduledEvents = new ScheduledEventDTO()
            {
                Weekday = Enum.Weekday.Monday,
                Start = new DateTime(),
                End = new DateTime()
            };

            return new[] { scheduledEvents };
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
    }
}