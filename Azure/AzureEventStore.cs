using System;
using System.Collections;
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

        public AzureEventStore(IAzureTableFactory azureTableFactory)
        {
            _table = azureTableFactory.GetTable();
        }

        private void checkStream(Partition p)
        {
            if (!Stream.ExistsAsync(p).GetAwaiter().GetResult())
            {
                var stream = Stream.ProvisionAsync(p).GetAwaiter().GetResult();
                Console.WriteLine("Provisioned new empty stream in partition '{0}'", stream.Partition);
                Console.WriteLine("Etag: {0}", stream.ETag);
                Console.WriteLine("Version: {0}", stream.Version);
            }

           

        }

        public void SaveEventsFor<TAggregate>(Guid aggregateId, int eventsLoaded, ArrayList newEvents)
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

        public IEnumerable LoadEventsFor<TAggregate>(Guid aggregateId)
        {
            var paritionKey = aggregateId.ToString("D");
            var partition = new Partition(_table, paritionKey);

            //var existent = Stream.TryOpenAsync(partition).GetAwaiter().GetResult();
            //var stream = existent.Found
            //    ? existent.Stream
            //    : new Stream(partition);
            //var stream = Stream.ProvisionAsync(partition).GetAwaiter().GetResult();

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
            var eeee = new TrailerBookedEvent();
            var ee = new TrailerBookingCanceledEvent();

            object json = null;

            if (e.Type == "Events.TrailerBookedEvent")
            {
                json = JsonConvert.DeserializeObject<TrailerBookedEvent>(e.Data);

            }
            if (e.Type == "Events.TrailerBookingCanceledEvent")
            {
                json = JsonConvert.DeserializeObject<TrailerBookingCanceledEvent>(e.Data);
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
