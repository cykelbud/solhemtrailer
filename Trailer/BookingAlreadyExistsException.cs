using System;

namespace Trailer
{
    public class BookingAlreadyExistsException : Exception
    {
        public long BookingId { get; }

        public BookingAlreadyExistsException(long bookingId)
        {
            BookingId = bookingId;
        }
    }
}