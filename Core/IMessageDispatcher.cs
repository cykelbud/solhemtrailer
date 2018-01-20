using System.Reflection;

namespace Core
{
    public interface IMessageDispatcher
    {
        /// <summary>
        /// Tries to send the specified command to its handler. Throws an exception
        /// if there is no handler registered for the command.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="c"></param>
        void SendCommand<TCommand>(TCommand c);

        /// <summary>
        /// Registers an aggregate as being the handler for a particular
        /// command.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="handler"></param>
        void AddHandlerFor<TCommand, TAggregate>()
            where TAggregate : Aggregate, new();

        /// <summary>
        /// Adds an object that subscribes to the specified event, by virtue of implementing
        /// the ISubscribeTo interface.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="subscriber"></param>
        void AddSubscriberFor<TEvent>(ISubscribeTo<TEvent> subscriber);

        /// <summary>
        /// Looks thorugh the specified assembly for all public types that implement
        /// the IHandleCommand or ISubscribeTo generic interfaces. Registers each of
        /// the implementations as a command handler or event subscriber.
        /// </summary>
        /// <param name="ass"></param>
        void ScanAssembly(Assembly ass);

        /// <summary>
        /// Looks at the specified object instance, examples what commands it handles
        /// or events it subscribes to, and registers it as a receiver/subscriber.
        /// </summary>
        /// <param name="instance"></param>
        void ScanInstance(object instance);

    }
}