using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using Core;

namespace Edument.CQRS
{
    /// <summary>
    /// This implements a basic message dispatcher, driving the overall command handling
    /// and event application/distribution process. It is suitable for a simple, single
    /// node application that can safely build its subscriber list at startup and keep
    /// it in memory. Depends on some kind of event storage mechanism.
    /// </summary>
    public class MessageDispatcher : IMessageDispatcher
    {
        private Dictionary<Type, Action<object>> commandHandlers =
            new Dictionary<Type, Action<object>>();
        private Dictionary<Type, List<Action<object>>> eventSubscribers =
            new Dictionary<Type, List<Action<object>>>();
        private IEventStore eventStore;

        /// <summary>
        /// Initializes a message dispatcher, which will use the specified event store
        /// implementation.
        /// </summary>
        /// <param name="es"></param>
        public MessageDispatcher(IEventStore es)
        {
            eventStore = es;
        }

        /// <summary>
        /// Tries to send the specified command to its handler. Throws an exception
        /// if there is no handler registered for the command.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="command"></param>
        public void SendCommand<TCommand>(TCommand command)
        {
            if (commandHandlers.ContainsKey(typeof(TCommand)))
            {
                // call commandhandler
                commandHandlers[typeof(TCommand)](command);
            }
            else
                throw new Exception("No command handler registered for " + typeof(TCommand).Name);
        }

        /// <summary>
        /// Publishes the specified event to all of its subscribers.
        /// </summary>
        /// <param name="e"></param>
        private void PublishEvent(object e)
        {
            var eventType = e.GetType();
            if (eventSubscribers.ContainsKey(eventType))
                foreach (var sub in eventSubscribers[eventType])
                    sub(e);
        }

        public void PublishEvents(IEnumerable<IEvent> @events)
        {
            foreach (var @event in @events)
            {
                PublishEvent(@event);
            }
        }

        /// <summary>
        /// Registers an aggregate as being the handler for a particular
        /// command.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="handler"></param>
        public void AddHandlerFor<TCommand, TAggregate>()
            where TAggregate : Aggregate, new()
        {
            if (commandHandlers.ContainsKey(typeof(TCommand)))
                throw new Exception("Command handler already registered for " + typeof(TCommand).Name);

            commandHandlers.Add(typeof(TCommand), command =>
                {
                    // Create an empty aggregate.
                    var aggregate = new TAggregate();
                    aggregate.Id = ((dynamic)command).Id;

                    // Get all events for this aggregate and apply to aggregate 
                    // to get to current state.
                    var allEvents = eventStore.LoadEventsFor<TAggregate>(aggregate.Id);
                    aggregate.ApplyEvents(allEvents);

                    // Call Handle(command) on all aggregates to yield the event (to store).
                    var resultEvents = new List<IEvent>();
                    foreach (var @event in ((IHandleCommand<TCommand>)aggregate).Handle((TCommand)command))
                    {
                        resultEvents.Add((IEvent)@event);
                    }

                    if (resultEvents.Count == 0)
                    {
                        return;
                    }

                    // Store the events in the event store.
                    eventStore.SaveEventsFor<TAggregate>(aggregate.Id, aggregate.EventsLoaded, resultEvents);

                    // We are done! trailer booked... but...

                    // ...publish events to subscribers.
                    foreach (var @event in resultEvents)
                    {
                        PublishEvent(@event);
                    }
                });
        }

        /// <summary>
        /// Adds an object that subscribes to the specified event, by virtue of implementing
        /// the ISubscribeTo interface.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="subscriber"></param>
        public void AddSubscriberFor<TEvent>(ISubscribeTo<TEvent> subscriber)
        {
            if (!eventSubscribers.ContainsKey(typeof(TEvent)))
                eventSubscribers.Add(typeof(TEvent), new List<Action<object>>());
            eventSubscribers[typeof(TEvent)].Add(e =>
                subscriber.Handle((TEvent)e));
        }

        /// <summary>
        /// Looks thorugh the specified assembly for all public types that implement
        /// the IHandleCommand or ISubscribeTo generic interfaces. Registers each of
        /// the implementations as a command handler or event subscriber.
        /// </summary>
        /// <param name="ass"></param>
        public void ScanAssembly(Assembly ass)
        {
            // Scan for and register handlers.
            var handlers =
                from t in ass.GetTypes()
                from i in t.GetInterfaces()
                where i.IsGenericType
                where i.GetGenericTypeDefinition() == typeof(IHandleCommand<>)
                let args = i.GetGenericArguments()
                select new
                {
                    CommandType = args[0],
                    AggregateType = t
                };
            foreach (var h in handlers)
                this.GetType().GetMethod("AddHandlerFor")
                    .MakeGenericMethod(h.CommandType, h.AggregateType)
                    .Invoke(this, new object[] { });

            // Scan for and register subscribers.
            var subscriber =
                from t in ass.GetTypes()
                from i in t.GetInterfaces()
                where i.IsGenericType
                where i.GetGenericTypeDefinition() == typeof(ISubscribeTo<>)
                select new
                {
                    Type = t,
                    EventType = i.GetGenericArguments()[0]
                };
            foreach (var s in subscriber)
                this.GetType().GetMethod("AddSubscriberFor")
                    .MakeGenericMethod(s.EventType)
                    .Invoke(this, new object[] { CreateInstanceOf(s.Type) });
        }

        /// <summary>
        /// Looks at the specified object instance, examples what commands it handles
        /// or events it subscribes to, and registers it as a receiver/subscriber.
        /// </summary>
        /// <param name="instance"></param>
        public void ScanInstance(object instance)
        {
            // Scan for and register handlers.
            var handlers =
                from i in instance.GetType().GetInterfaces()
                where i.IsGenericType
                where i.GetGenericTypeDefinition() == typeof(IHandleCommand<>)
                let args = i.GetGenericArguments()
                select new
                {
                    CommandType = args[0],
                    AggregateType = instance.GetType()
                };
            foreach (var h in handlers)
                this.GetType().GetMethod("AddHandlerFor")
                    .MakeGenericMethod(h.CommandType, h.AggregateType)
                    .Invoke(this, new object[] { });

            // Scan for and register subscribers.
            var subscriber =
                from i in instance.GetType().GetInterfaces()
                where i.IsGenericType
                where i.GetGenericTypeDefinition() == typeof(ISubscribeTo<>)
                select i.GetGenericArguments()[0];
            foreach (var s in subscriber)
                this.GetType().GetMethod("AddSubscriberFor")
                    .MakeGenericMethod(s)
                    .Invoke(this, new object[] { instance });
        }

        /// <summary>
        /// Creates an instance of the specified type. If you are using some kind
        /// of DI container, and want to use it to create instances of the handler
        /// or subscriber, you can plug it in here.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private object CreateInstanceOf(Type t)
        {
            return Activator.CreateInstance(t);
        }
    }
}
