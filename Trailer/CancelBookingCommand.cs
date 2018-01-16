using System;
using Core;

namespace Trailer
{
    public class CancelBookingCommand : ICommand
    {
        // trailer id
        public Guid Id { get; set; }

        public long BookingId { get; set; }
    }
}