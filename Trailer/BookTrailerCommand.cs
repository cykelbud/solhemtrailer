using System;

namespace Trailer
{
    public class BookTrailerCommand
    {
        public long BookingId { get; set; }
        public Guid TrailerId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
