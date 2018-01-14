using System;

namespace Core
{
    public class ScheduleSlot
    {
        public Guid TrailerId { get; set; }
        public long BookingId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvaliable { get; set; }
    }
}