using System;

namespace Trailer
{
    public class BookingDoesNotExist : Exception
    {
        public long BookingId { get; }

        public BookingDoesNotExist(long bookingId)
        {
            BookingId = bookingId;
        }
    }
}