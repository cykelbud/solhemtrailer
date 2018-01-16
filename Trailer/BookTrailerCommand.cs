using System;
using Core;

namespace Trailer
{
    public class BookTrailerCommand : ICommand
    {
        // Aggregate Id
        public Guid Id { get; set; }

        public long BookingId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
