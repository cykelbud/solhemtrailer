using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Events;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Streamstone;

namespace Azure
{

    public class AzureEventStore : IEventStore
    {
        private readonly CloudTable _table;
        const string BookingsTableName = "bookings";

        public AzureEventStore(IAzureTableFactory azureTableFactory)
        {
            _table = azureTableFactory.GetTable(BookingsTableName);
        }

        public void SaveEventsFor<TAggregate>(Guid aggregateId, int eventsLoaded, IEnumerable<IEvent> newEvents)
        {
            var paritionKey = aggregateId.ToString("D");
            var partition = new Partition(_table, paritionKey);

            var existent = Stream.TryOpenAsync(partition).GetAwaiter().GetResult();
            var stream = existent.Found
                ? existent.Stream
                : new Stream(partition);

            if (stream.Version != eventsLoaded)
                throw new ConcurrencyException();

            try
            {
                var events = newEvents.Cast<IEvent>();
                Stream.WriteAsync(stream, events.Select(ToEventData).ToArray());
            }
            catch (ConcurrencyConflictException e)
            {
                throw new ConcurrencyException();
            }
        }

        public IEnumerable<IEvent> LoadEventsFor<TAggregate>(Guid aggregateId)
        {
            var paritionKey = aggregateId.ToString("D");
            var partition = new Partition(_table, paritionKey);

            if (!Stream.ExistsAsync(partition).GetAwaiter().GetResult())
            {
                var stream = Stream.ProvisionAsync(partition).GetAwaiter().GetResult();
                if (!Stream.ExistsAsync(partition).GetAwaiter().GetResult())
                {
                    throw new AggregateNotFoundException();
                }
            }

            return Stream.ReadAsync<EventEntity>(partition).GetAwaiter().GetResult().Events.Select(ToEvent).ToList();
        }


        static IEvent ToEvent(EventEntity e)
        {
            object json = null;

            if (e.Type == "Events.TrailerBookedEvent")
            {
                json = JsonConvert.DeserializeObject<TrailerBookedEvent>(e.Data);
            }
            if (e.Type == "Events.TrailerBookingCanceledEvent")
            {
                json = JsonConvert.DeserializeObject<TrailerBookingCanceledEvent>(e.Data);
            }
            if (e.Type == "Events.PhoneNumberAuthorizedEvent")
            {
                json = JsonConvert.DeserializeObject<PhoneNumberAuthorizedEvent>(e.Data);
            }
            if (e.Type == "Events.PhoneNumberRemovedEvent")
            {
                json = JsonConvert.DeserializeObject<PhoneNumberRemovedEvent>(e.Data);
            }

            return (IEvent)json;
        }

        static EventData ToEventData(IEvent e)
        {
            var id = Guid.NewGuid();

            var properties = new
            {
                Id = id,
                Type = e.GetType().FullName,
                Data = JsonConvert.SerializeObject(e)
            };

            return new EventData(EventId.From(id), EventProperties.From(properties));
        }

        class EventEntity : TableEntity
        {
            public string Type { get; set; }
            public string Data { get; set; }
        }
    }

    public class AggregateNotFoundException : Exception
    {
    }

    public class ConcurrencyException : Exception
    {
    }
}
