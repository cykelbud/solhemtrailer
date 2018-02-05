using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Events;

namespace ReadModels
{
    public class Scheduler : IScheduleQueries,
        ISubscribeTo<TrailerBookedEvent>,
        ISubscribeTo<TrailerBookingCanceledEvent>
    {
        private readonly List<Booking> _bookings = new List<Booking>();

        public IEnumerable<ScheduleSlot> GetBookingSchedule(Guid trailerId, DateTime startDate, DateTime endDate)
        {
            var slotsPerDay = 3;
            var slotLength = 4;
            var startHrs = 8;
            //var slotsPerDay = 5;
            //var slotLength = 3;
            //var startHrs = 6;
            // generate schedule
            var scheduleSlots = new List<ScheduleSlot>();
            for (var day = startDate; day < endDate; day = day.AddDays(1))
            {
                for (int slot = 0; slot < slotsPerDay; slot++)
                {
                    var hrs = startHrs + slot * slotLength;
                    var startTime = day.AddHours(hrs);
                    var endTime = startTime.AddHours(slotLength);
                    if (slot == slotsPerDay - 1)
                    {
                        endTime = day.AddDays(1).AddHours(startHrs);
                    }

                    var scheduleSlot = new ScheduleSlot()
                    {
                        TrailerId = trailerId,
                        StartTime = startTime.Ticks,
                        StartUtc = startTime.ToString("s"),
                        EndTime = endTime.Ticks,
                        EndUtc = endTime.ToString("s"),
                        IsAvailable = endTime >= DateTime.Now,
                        Date = day.ToString("yyyy-MM-dd")
                    };
                    scheduleSlots.Add(scheduleSlot);
                }
            }
            
            // set already booked items to not available
            scheduleSlots.ForEach(s =>
            {
                if (_bookings.Any(booking =>
                    (booking.Start >= s.StartTime && booking.Start < s.EndTime) ||
                    (booking.End > s.StartTime && booking.End <= s.EndTime)
                ))
                {
                    s.IsAvailable = false;
                }
            });
            
            return scheduleSlots;
        }

        public IEnumerable<Booking> GetAll()
        {
            return _bookings;
        }

        public IEnumerable<Booking> GetBookings(string phone)
        {
            return _bookings.Where(b => b.Phone == phone);
        }

        public void Handle(TrailerBookedEvent @event)
        {
            _bookings.Add(new Booking() { BookingId = @event.BookingId, TrailerId = @event.Id, Start = @event.Start, End = @event.End, Phone = @event.Phone });
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