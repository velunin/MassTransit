﻿namespace MassTransit.Context
{
    using System.Threading;


    public class FaultedPublishContext<TMessage> :
        BaseSendContext<TMessage>
        where TMessage : class
    {
        public FaultedPublishContext(TMessage message, CancellationToken cancellationToken)
            : base(message, cancellationToken)
        {
        }
    }
}
