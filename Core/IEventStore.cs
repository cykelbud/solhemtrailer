using System;
using System.Collections;
using System.Collections.Generic;

namespace Core
{
    public interface IEventStore
    {
        IEnumerable<Core.IEvent> LoadEventsFor<TAggregate>(Guid id);
        void SaveEventsFor<TAggregate>(Guid id, int eventsLoaded, IEnumerable<IEvent> newEvents);
    }
}
