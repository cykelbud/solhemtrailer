using System;
using Core;

namespace Trailer
{
    public class BookTrailerCommand : ICommand
    {
        // Aggregate Id
        public Guid Id { get; set; }

        public long BookingId { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
