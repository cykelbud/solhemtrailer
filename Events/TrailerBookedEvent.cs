using System;
using Core;

namespace Events 
{
    public class TrailerBookedEvent : IEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }


        public long BookingId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
