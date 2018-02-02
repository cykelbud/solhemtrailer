using System;

namespace Core
{
    public class ScheduleSlot
    {
        public Guid TrailerId { get; set; }
        public string Date { get; set; }
        public long StartTime { get; set; }
        public string StartUtc { get; set; }
        public long EndTime { get; set; }
        public string EndUtc { get; set; }
        public bool IsAvaliable { get; set; }
    }
}