using System;

namespace ReadModels
{
    internal class Booking
    {
        public long BookingId { get; set; }
        public Guid TrailerId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}