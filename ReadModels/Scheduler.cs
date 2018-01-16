using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Edument.CQRS;
using Events;

namespace ReadModels
{
    public class Scheduler : IScheduleQueries,
        ISubscribeTo<TrailerBookedEvent>,
        ISubscribeTo<TrailerBookingCanceledEvent>
    {
        private List<Booking> _bookings = new List<Booking>();

        public IEnumerable<ScheduleSlot> GetBookingSchedule(Guid trailerId, DateTime startDate, DateTime endDate)
        {
            // clear old bookings
            _bookings = _bookings.Where(b => b.Start.Day >= DateTime.Now.Day).ToList();
            var slotsPerDay = 5;
            var slotLength = 3;
            var startTime = 8;
            // generate schedule
            var scheduleSlots = new List<ScheduleSlot>();
            for (var day = startDate; day < endDate; day = day.AddDays(1))
            {
                for (int slot = 1; slot <= slotsPerDay; slot++)
                {
                    var startTime = 

                    var scheduleSlot = new ScheduleSlot()
                    {
                        BookingId = day.Ticks, // startticks
                        TrailerId = trailerId,
                        StartTime = day,
                        EndTime = day.AddDays(1).AddTicks(-1),
                        IsAvaliable = true,
                    };
                    scheduleSlots.Add(scheduleSlot);
                }
            }
            
            // set already booked items to not available
            scheduleSlots.ForEach(s =>
            {
                if (_bookings.Any(b =>
                    (b.Start.Ticks > s.StartTime.Ticks && b.Start.Ticks < s.EndTime.Ticks) ||
                    (b.End.Ticks > s.StartTime.Ticks && b.End.Ticks < s.EndTime.Ticks)
                ))
                {
                    s.IsAvaliable = false;
                }
            });
            
            return scheduleSlots;
        }

        public void Handle(TrailerBookedEvent @event)
        {
            _bookings.Add(new Booking() { BookingId = @event.BookingId, TrailerId = @event.Id, Start = @event.Start, End = @event.End });
        }

        public void Handle(TrailerBookingCanceledEvent @event)
        {
            var booking = _bookings.SingleOrDefault(b => b.BookingId == @event.BookingId);
            if (booking == null)
            {
                return;
            }
            _bookings.Remove(booking);
        }
    }
}