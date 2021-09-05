using System;
using Helios.Enum;

namespace Helios.DTO{
    public class ScheduledEventDTO
    {
        public Weekday Weekday {get; set;}
        public DateTime Start {get; set;}
        public DateTime End {get; set;}

    }
}