﻿using System;
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
            var slotsPerDay = 5;
            var slotLength = 3;
            var startHrs = 6;
            // generate schedule
            var scheduleSlots = new List<ScheduleSlot>();
            for (var day = startDate; day < endDate; day = day.AddDays(1))
            {
                for (int slot = 1; slot <= slotsPerDay; slot++)
                {
                    var hrs = startHrs + slot * slotLength;
                    var startTime = day.AddHours(hrs);
                    var scheduleSlot = new ScheduleSlot()
                    {
                        BookingId = day.Ticks, // startticks
                        TrailerId = trailerId,
                        StartTime = startTime.Ticks,
                        EndTime = startTime.AddHours(slotLength).Ticks,
                        IsAvaliable = true,
                        Date = startDate.ToShortDateString()
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
                    s.IsAvaliable = false;
                }
            });
            
            return scheduleSlots;
        }

        public IEnumerable<Booking> GetAll()
        {
            return _bookings;
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