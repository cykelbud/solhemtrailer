using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Edument.CQRS;
using Events;

namespace Trailer
{
    public class TrailerAggregate : Aggregate,
        IHandleCommand<BookTrailerCommand>,
        IHandleCommand<CancelBookingCommand>,
        IApplyEvent<TrailerBookedEvent>,
        IApplyEvent<TrailerBookingCanceledEvent>

    {
        readonly List<Booking> _bookings = new List<Booking>();

        public IEnumerable Handle(BookTrailerCommand c)
        {
            // check trailer is not booked at that time
            if (_bookings.Any(b =>
                (b.Start.Ticks >= c.Start.Ticks && b.Start.Ticks <= c.End.Ticks) ||
                (b.End.Ticks >= c.Start.Ticks && b.End.Ticks <= c.End.Ticks) 
            ))
            {
                throw new BookingAlreadyExistsException(c.BookingId);
            }

            //call this
            yield return new TrailerBookedEvent
            {
                BookingId = c.BookingId,
                TrailerId = c.TrailerId,
                Start = c.Start,
                End = c.End
            };
        }

        public IEnumerable Handle(CancelBookingCommand c)
        {
            if (_bookings.All(b => b.BookingId != c.BookingId))
            {
                throw new BookingDoesNotExist(c.BookingId);
            }

            yield return new TrailerBookingCanceledEvent
            {
                BookingId = c.BookingId,
            };
        }

        public void Apply(TrailerBookedEvent e)
        {
            _bookings.Add(new Booking() { BookingId = e.BookingId, TrailerId = e.TrailerId, Start = e.Start, End = e.End });
        }
        public void Apply(TrailerBookingCanceledEvent e)
        {
            var booking = _bookings.Single(b => b.BookingId == e.BookingId);
            _bookings.Remove(booking);
        }
    }

    internal class Booking
    {
        public long BookingId { get; set; }
        public Guid TrailerId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
