using System;

namespace Events
{
    public interface IEvent
    {
        Guid Id { get; set; }
        int Version { get; set; }
    }
}
