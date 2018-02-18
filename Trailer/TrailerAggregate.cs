using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Events;

namespace Trailer
{
    public class TrailerAggregate : Aggregate,
        IHandleCommand<BookTrailerCommand>,
        IHandleCommand<CancelBookingCommand>,
        IApplyEvent<TrailerBookedEvent>,
        IApplyEvent<TrailerBookingCanceledEvent>

    {
        private readonly List<Booking> _bookings = new List<Booking>();

        public IEnumerable Handle(BookTrailerCommand command)
        {
            // check trailer is not booked at that time
            if (_bookings.Any(booking =>
                (booking.Start >= command.Start && booking.Start < command.End) ||
                (booking.End > command.Start && booking.End <= command.End) 
            ))
            {
                throw new BookingAlreadyExistsException(command.BookingId);
            }

            //call this
            yield return new TrailerBookedEvent
            {
                Id = command.Id,
                BookingId = command.BookingId,
                Start = command.Start,
                End = command.End,
                Phone = command.Phone,
                Email = command.Email,
            };
        }

        public IEnumerable Handle(CancelBookingCommand command)
        {
            // Validate business rules
            if (_bookings.All(b => b.BookingId != command.BookingId))
            {
                throw new BookingDoesNotExist(command.BookingId);
            }

            var phone = _bookings.Single(b => b.BookingId == command.BookingId).Phone;
            yield return new TrailerBookingCanceledEvent
            {
                BookingId = command.BookingId,
                Phone = phone,
            };
        }

        public void Apply(TrailerBookedEvent @event)
        {
            _bookings.Add(new Booking() { BookingId = @event.BookingId,
                TrailerId = @event.Id,
                Start = @event.Start,
                End = @event.End,
                Email = @event.Email,
                Phone = @event.Phone
            });
        }
        public void Apply(TrailerBookingCanceledEvent @event)
        {
            var booking = _bookings.Single(b => b.BookingId == @event.BookingId);
            _bookings.Remove(booking);
        }
    }

    internal class Booking
    {
        public long BookingId { get; set; }
        public Guid TrailerId { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
