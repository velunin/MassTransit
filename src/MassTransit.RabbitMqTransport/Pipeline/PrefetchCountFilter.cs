namespace MassTransit.RabbitMqTransport.Pipeline
{
    using System;
    using System.Threading.Tasks;
    using Context;
    using Contracts;
    using GreenPipes;
    using Management;
    using MassTransit.Pipeline;
    using Util;


    /// <summary>
    /// Prepares a queue for receiving messages using the ReceiveSettings specified.
    /// </summary>
    public class PrefetchCountFilter :
        IFilter<ModelContext>,
        ISetPrefetchCount
    {
        readonly IConsumePipeConnector _managementPipe;
        ushort _prefetchCount;

        public PrefetchCountFilter(IConsumePipeConnector managementPipe, ushort prefetchCount)
        {
            _prefetchCount = prefetchCount;
            _managementPipe = managementPipe;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("prefetchCount");
            scope.Add("prefetchCount", _prefetchCount);
        }

        async Task IFilter<ModelContext>.Send(ModelContext context, IPipe<ModelContext> next)
        {
            LogContext.Debug?.Log("Prefetch Count: {PrefetchCount}", _prefetchCount);

            await context.BasicQos(0, _prefetchCount, true).ConfigureAwait(false);

            using (new SetPrefetchCountConsumer(_managementPipe, context, this))
            {
                await next.Send(context).ConfigureAwait(false);
            }
        }

        public Task SetPrefetchCount(ushort prefetchCount)
        {
            _prefetchCount = prefetchCount;

            return TaskUtil.Completed;
        }


        class SetPrefetchCountConsumer :
            IConsumer<SetPrefetchCount>,
            IDisposable
        {
            readonly ISetPrefetchCount _filter;
            readonly ConnectHandle _handle;
            readonly ModelContext _modelContext;

            public SetPrefetchCountConsumer(IConsumePipeConnector managementPipe, ModelContext modelContext, ISetPrefetchCount filter)
            {
                _modelContext = modelContext;
                _filter = filter;

                _handle = managementPipe.ConnectInstance(this);
            }

            async Task IConsumer<SetPrefetchCount>.Consume(ConsumeContext<SetPrefetchCount> context)
            {
                var prefetchCount = context.Message.PrefetchCount;

                LogContext.Debug?.Log("Set Prefetch Count: (count: {PrefetchCount})", prefetchCount);

                await _modelContext.BasicQos(0, prefetchCount, true).ConfigureAwait(false);

                await _filter.SetPrefetchCount(prefetchCount).ConfigureAwait(false);
            }

            public void Dispose()
            {
                _handle.Dispose();
            }
        }
    }
}
