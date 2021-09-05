using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Helios.DTO;

namespace Helios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulingController : ControllerBase
    {

        // GET: api/Scheduling
        [HttpGet]
        public async Task<IEnumerable<ScheduledEventDTO>> GetScheduledEvents()
        {
            var scheduledEvents = new ScheduledEventDTO()
            {
                Weekday = Enum.Weekday.Monday,
                Start = new DateTime(),
                End = new DateTime()
            };

            return new[] { scheduledEvents };
        }
    }
}