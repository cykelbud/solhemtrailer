using System;

namespace Core
{
    public class ScheduleSlot
    {
        public Guid TrailerId { get; set; }
        public long BookingId { get; set; }
        public string Date { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public bool IsAvaliable { get; set; }
    }
}