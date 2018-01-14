using System;

namespace Events 
{
    public class TrailerBookedEvent
    {
        public Guid EventId { get; set; }
        public long BookingId { get; set; }
        public Guid TrailerId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
