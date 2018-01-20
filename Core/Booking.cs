using System;

namespace Core
{
    public class Booking
    {
        public long BookingId { get; set; }
        public Guid TrailerId { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
    }
}