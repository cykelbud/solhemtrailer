using System;

namespace Events
{
    public class TrailerBookingCanceledEvent : IEvent
    {
        public long BookingId { get; set; }
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}