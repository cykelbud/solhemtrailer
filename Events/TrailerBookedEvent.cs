using System;
using Core;

namespace Events 
{
    public class TrailerBookedEvent : IEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }


        public long BookingId { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
    }
}
