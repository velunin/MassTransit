﻿namespace MassTransit.AmazonSqsTransport.Configuration.Configurators
{
    using System;
    using System.Collections.Generic;
    using BusConfigurators;
    using Configuration;
    using GreenPipes;
    using MassTransit.Builders;
    using Topology.Configuration;
    using Topology.Settings;


    public class AmazonSqsBusFactoryConfigurator :
        BusFactoryConfigurator,
        IAmazonSqsBusFactoryConfigurator,
        IBusFactory
    {
        readonly IAmazonSqsBusConfiguration _busConfiguration;
        readonly IAmazonSqsHostConfiguration _hostConfiguration;
        readonly QueueReceiveSettings _settings;

        public AmazonSqsBusFactoryConfigurator(IAmazonSqsBusConfiguration busConfiguration)
            : base(busConfiguration)
        {
            _busConfiguration = busConfiguration;
            _hostConfiguration = busConfiguration.HostConfiguration;

            var queueName = _busConfiguration.Topology.Consume.CreateTemporaryQueueName("bus");
            _settings = new QueueReceiveSettings(queueName, false, true);
        }

        public IBusControl CreateBus()
        {
            void ConfigureBusEndpoint(IAmazonSqsReceiveEndpointConfigurator configurator)
            {
                configurator.SubscribeMessageTopics = false;
            }

            var busReceiveEndpointConfiguration = _busConfiguration.HostConfiguration
                .CreateReceiveEndpointConfiguration(_settings, _busConfiguration.BusEndpointConfiguration, ConfigureBusEndpoint);

            var builder = new ConfigurationBusBuilder(_busConfiguration, busReceiveEndpointConfiguration);

            ApplySpecifications(builder);

            return builder.Build();
        }

        public override IEnumerable<ValidationResult> Validate()
        {
            foreach (var result in base.Validate())
                yield return result;

            if (string.IsNullOrWhiteSpace(_settings.EntityName))
                yield return this.Failure("Bus", "The bus queue name must not be null or empty");
        }

        public ushort PrefetchCount
        {
            set => _settings.PrefetchCount = value;
        }

        public ushort WaitTimeSeconds
        {
            set => _settings.WaitTimeSeconds = value;
        }

        public bool Durable
        {
            set => _settings.Durable = value;
        }

        public bool AutoDelete
        {
            set => _settings.AutoDelete = value;
        }

        public bool PurgeOnStartup
        {
            set => _settings.PurgeOnStartup = value;
        }

        public void OverrideDefaultBusEndpointQueueName(string value)
        {
            _settings.EntityName = value;
        }

        public IDictionary<string, object> QueueAttributes => _settings.QueueAttributes;
        public IDictionary<string, object> QueueSubscriptionAttributes => _settings.QueueSubscriptionAttributes;
        public IDictionary<string, string> QueueTags => _settings.QueueTags;

        public bool DeployTopologyOnly
        {
            set => _hostConfiguration.DeployTopologyOnly = value;
        }

        public IAmazonSqsHost Host(AmazonSqsHostSettings settings)
        {
            _busConfiguration.HostConfiguration.Settings = settings;

            return _busConfiguration.HostConfiguration.Proxy;
        }

        void IAmazonSqsBusFactoryConfigurator.Send<T>(Action<IAmazonSqsMessageSendTopologyConfigurator<T>> configureTopology)
        {
            IAmazonSqsMessageSendTopologyConfigurator<T> configurator = _busConfiguration.Topology.Send.GetMessageTopology<T>();

            configureTopology?.Invoke(configurator);
        }

        void IAmazonSqsBusFactoryConfigurator.Publish<T>(Action<IAmazonSqsMessagePublishTopologyConfigurator<T>> configureTopology)
        {
            IAmazonSqsMessagePublishTopologyConfigurator<T> configurator = _busConfiguration.Topology.Publish.GetMessageTopology<T>();

            configureTopology?.Invoke(configurator);
        }

        public new IAmazonSqsSendTopologyConfigurator SendTopology => _busConfiguration.Topology.Send;
        public new IAmazonSqsPublishTopologyConfigurator PublishTopology => _busConfiguration.Topology.Publish;

        public void ReceiveEndpoint(IEndpointDefinition definition, IEndpointNameFormatter endpointNameFormatter,
            Action<IAmazonSqsReceiveEndpointConfigurator> configureEndpoint = null)
        {
            _hostConfiguration.ReceiveEndpoint(definition, endpointNameFormatter, configureEndpoint);
        }

        public void ReceiveEndpoint(IEndpointDefinition definition, IEndpointNameFormatter endpointNameFormatter,
            Action<IReceiveEndpointConfigurator> configureEndpoint = null)
        {
            _hostConfiguration.ReceiveEndpoint(definition, endpointNameFormatter, configureEndpoint);
        }

        public void ReceiveEndpoint(IAmazonSqsHost host, IEndpointDefinition definition, IEndpointNameFormatter endpointNameFormatter,
            Action<IAmazonSqsReceiveEndpointConfigurator> configureEndpoint = null)
        {
            _hostConfiguration.ReceiveEndpoint(definition, endpointNameFormatter, configureEndpoint);
        }

        public void ReceiveEndpoint(string queueName, Action<IAmazonSqsReceiveEndpointConfigurator> configureEndpoint)
        {
            _hostConfiguration.ReceiveEndpoint(queueName, configureEndpoint);
        }

        public void ReceiveEndpoint(string queueName, Action<IReceiveEndpointConfigurator> configureEndpoint)
        {
            _hostConfiguration.ReceiveEndpoint(queueName, configureEndpoint);
        }

        public void ReceiveEndpoint(IAmazonSqsHost host, string queueName, Action<IAmazonSqsReceiveEndpointConfigurator> configureEndpoint)
        {
            _hostConfiguration.ReceiveEndpoint(queueName, configureEndpoint);
        }
    }
}
