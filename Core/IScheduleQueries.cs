using System;
using System.Collections.Generic;

namespace Core
{
    public interface IScheduleQueries
    {
        IEnumerable<ScheduleSlot> GetBookingSchedule(Guid trailerId, DateTime startDate, DateTime endDate);
    }
}
