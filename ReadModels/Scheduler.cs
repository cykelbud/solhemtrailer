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

            // generate schedule
            var scheduleSlots = new List<ScheduleSlot>();
            for (var day = startDate; day < endDate; day = day.AddDays(1))
            {
                scheduleSlots.Add(new ScheduleSlot()
                {
                    BookingId = day.Ticks, // startticks
                    TrailerId = trailerId,
                    StartTime = day,
                    EndTime = day.AddDays(1).AddTicks(-1),
                    IsAvaliable = true,
                });
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

        public void Handle(TrailerBookedEvent e)
        {
            _bookings.Add(new Booking() { BookingId = e.BookingId, TrailerId = e.TrailerId, Start = e.Start, End = e.End });
        }

        public void Handle(TrailerBookingCanceledEvent e)
        {
            var booking = _bookings.SingleOrDefault(b => b.BookingId == e.BookingId);
            if (booking == null)
            {
                return;
            }
            _bookings.Remove(booking);
        }
    }
}