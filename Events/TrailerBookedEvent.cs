using System;
using Edument.CQRS;

namespace Events 
{
    public class TrailerBookedEvent : IEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }


        public long BookingId { get; set; }
        public Guid TrailerId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
