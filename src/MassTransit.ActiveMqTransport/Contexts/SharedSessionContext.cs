﻿namespace MassTransit.ActiveMqTransport.Contexts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Apache.NMS;
    using GreenPipes;


    public class SharedSessionContext :
        ProxyPipeContext,
        SessionContext
    {
        readonly SessionContext _context;

        public SharedSessionContext(SessionContext context, CancellationToken cancellationToken)
            : base(context)
        {
            _context = context;
            CancellationToken = cancellationToken;
        }

        public override CancellationToken CancellationToken { get; }

        ISession SessionContext.Session => _context.Session;
        ConnectionContext SessionContext.ConnectionContext => _context.ConnectionContext;

        Task<ITopic> SessionContext.GetTopic(string topicName)
        {
            return _context.GetTopic(topicName);
        }

        Task<IQueue> SessionContext.GetQueue(string queueName)
        {
            return _context.GetQueue(queueName);
        }

        Task<IDestination> SessionContext.GetDestination(string destination, DestinationType destinationType)
        {
            return _context.GetDestination(destination, destinationType);
        }

        Task<IMessageProducer> SessionContext.CreateMessageProducer(IDestination destination)
        {
            return _context.CreateMessageProducer(destination);
        }

        Task<IMessageConsumer> SessionContext.CreateMessageConsumer(IDestination destination, string selector, bool noLocal)
        {
            return _context.CreateMessageConsumer(destination, selector, noLocal);
        }

        Task SessionContext.DeleteTopic(string topicName)
        {
            return _context.DeleteTopic(topicName);
        }

        Task SessionContext.DeleteQueue(string queueName)
        {
            return _context.DeleteQueue(queueName);
        }
    }
}
