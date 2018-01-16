using System;

namespace Core
{
    public interface IEvent
    {
        Guid Id { get; set; }
        int Version { get; set; }
    }
}
