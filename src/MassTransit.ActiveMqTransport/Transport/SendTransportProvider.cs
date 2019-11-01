﻿namespace MassTransit.ActiveMqTransport.Transport
{
    using System;
    using System.Threading.Tasks;
    using Transports;


    public class SendTransportProvider :
        ISendTransportProvider
    {
        readonly IActiveMqHostControl _host;

        public SendTransportProvider(IActiveMqHostControl host)
        {
            _host = host;
        }

        Task<ISendTransport> ISendTransportProvider.GetSendTransport(Uri address)
        {
            var endpointAddress = new ActiveMqEndpointAddress(_host.Address, address);

            return _host.CreateSendTransport(endpointAddress);
        }

        public Uri NormalizeAddress(Uri address)
        {
            return new ActiveMqEndpointAddress(_host.Address, address);
        }
    }
}
