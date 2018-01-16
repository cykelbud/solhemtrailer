using System;
using Core;

namespace Events
{
    public class TrailerBookingCanceledEvent : IEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public long BookingId { get; set; }
    }
}