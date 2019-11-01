namespace MassTransit.Courier
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contexts;
    using Contracts;
    using InternalMessages;
    using MassTransit.Serialization;
    using Metadata;
    using Newtonsoft.Json;
    using Serialization;


    public class RoutingSlipEventPublisher :
        IRoutingSlipEventPublisher
    {
        static IDictionary<string, object> _emptyObject;
        readonly CourierContext _context;
        readonly HostInfo _host;
        readonly IPublishEndpoint _publishEndpoint;
        readonly RoutingSlip _routingSlip;
        readonly ISendEndpointProvider _sendEndpointProvider;

        static RoutingSlipEventPublisher()
        {
            RoutingSlipEventCorrelation.ConfigureCorrelationIds();
        }

        public RoutingSlipEventPublisher(CourierContext context, RoutingSlip routingSlip)
        {
            _sendEndpointProvider = context;
            _publishEndpoint = context;
            _routingSlip = routingSlip;
            _host = context.Host;
            _context = context;
        }

        public RoutingSlipEventPublisher(ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint, RoutingSlip routingSlip)
        {
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
            _routingSlip = routingSlip;
            _host = HostMetadataCache.Host;
        }

        static IDictionary<string, object> EmptyObject => _emptyObject ?? (_emptyObject = JsonConvert.DeserializeObject<IDictionary<string, object>>("{}"));

        public Task PublishRoutingSlipCompleted(DateTime timestamp, TimeSpan duration, IDictionary<string, object> variables)
        {
            return PublishEvent<RoutingSlipCompleted>(RoutingSlipEvents.Completed, contents => new RoutingSlipCompletedMessage(
                _routingSlip.TrackingNumber,
                timestamp,
                duration,
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Variables)
                    ? variables
                    : EmptyObject
            ));
        }

        public Task PublishRoutingSlipFaulted(DateTime timestamp, TimeSpan duration, IDictionary<string, object> variables,
            params ActivityException[] exceptions)
        {
            return PublishEvent<RoutingSlipFaulted>(RoutingSlipEvents.Faulted, contents => new RoutingSlipFaultedMessage(
                _routingSlip.TrackingNumber,
                timestamp,
                duration,
                exceptions,
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Variables)
                    ? variables
                    : EmptyObject
            ));
        }

        public Task PublishRoutingSlipActivityCompleted(string activityName, Guid executionId,
            DateTime timestamp, TimeSpan duration, IDictionary<string, object> variables, IDictionary<string, object> arguments,
            IDictionary<string, object> data)
        {
            return PublishEvent<RoutingSlipActivityCompleted>(RoutingSlipEvents.ActivityCompleted, contents => new RoutingSlipActivityCompletedMessage(
                _host,
                _routingSlip.TrackingNumber,
                activityName,
                executionId,
                timestamp,
                duration,
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Variables)
                    ? variables
                    : EmptyObject,
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Arguments)
                    ? arguments
                    : EmptyObject,
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Data)
                    ? data
                    : EmptyObject));
        }

        public Task PublishRoutingSlipActivityFaulted(string activityName, Guid executionId, DateTime timestamp, TimeSpan duration, ExceptionInfo exceptionInfo,
            IDictionary<string, object> variables, IDictionary<string, object> arguments)
        {
            return PublishEvent<RoutingSlipActivityFaulted>(RoutingSlipEvents.ActivityFaulted, contents => new RoutingSlipActivityFaultedMessage(
                _host,
                _routingSlip.TrackingNumber,
                activityName,
                executionId,
                timestamp,
                duration,
                exceptionInfo,
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Variables)
                    ? variables
                    : EmptyObject,
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Arguments)
                    ? arguments
                    : EmptyObject));
        }

        public Task PublishRoutingSlipActivityCompensated(string activityName, Guid executionId, DateTime timestamp, TimeSpan duration,
            IDictionary<string, object> variables, IDictionary<string, object> data)
        {
            return PublishEvent<RoutingSlipActivityCompensated>(RoutingSlipEvents.ActivityCompensated, contents => new RoutingSlipActivityCompensatedMessage(
                _host,
                _routingSlip.TrackingNumber,
                activityName,
                executionId,
                timestamp,
                duration,
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Variables)
                    ? variables
                    : EmptyObject,
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Arguments)
                    ? data
                    : EmptyObject));
        }

        public Task PublishRoutingSlipRevised(string activityName, Guid executionId, DateTime timestamp, TimeSpan duration,
            IDictionary<string, object> variables,
            IList<Activity> itinerary, IList<Activity> previousItinerary)
        {
            return PublishEvent<RoutingSlipRevised>(RoutingSlipEvents.Revised, contents => new RoutingSlipRevisedMessage(
                _host,
                _routingSlip.TrackingNumber,
                activityName,
                executionId,
                timestamp,
                duration,
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Variables)
                    ? variables
                    : EmptyObject,
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Itinerary)
                    ? itinerary
                    : Enumerable.Empty<Activity>(),
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Itinerary)
                    ? previousItinerary
                    : Enumerable.Empty<Activity>()));
        }

        public Task PublishRoutingSlipTerminated(string activityName, Guid executionId, DateTime timestamp, TimeSpan duration,
            IDictionary<string, object> variables,
            IList<Activity> previousItinerary)
        {
            return PublishEvent<RoutingSlipTerminated>(RoutingSlipEvents.Terminated, contents => new RoutingSlipTerminatedMessage(
                _host,
                _routingSlip.TrackingNumber,
                activityName,
                executionId,
                timestamp,
                duration,
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Variables)
                    ? variables
                    : EmptyObject,
                contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Itinerary)
                    ? previousItinerary
                    : Enumerable.Empty<Activity>()));
        }

        public Task PublishRoutingSlipActivityCompensationFailed(string activityName, Guid executionId,
            DateTime timestamp, TimeSpan duration, DateTime failureTimestamp, TimeSpan routingSlipDuration,
            ExceptionInfo exceptionInfo, IDictionary<string, object> variables, IDictionary<string, object> data)
        {
            var activityTask = PublishEvent<RoutingSlipActivityCompensationFailed>(RoutingSlipEvents.ActivityCompensationFailed,
                contents => new ActivityCompensationFailed(
                    _host,
                    _routingSlip.TrackingNumber,
                    activityName,
                    executionId,
                    timestamp,
                    duration,
                    exceptionInfo,
                    contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Variables)
                        ? variables
                        : EmptyObject,
                    contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Data)
                        ? data
                        : EmptyObject));

            var slipTask = PublishEvent<RoutingSlipCompensationFailed>(RoutingSlipEvents.CompensationFailed,
                contents => new CompensationFailed(
                    _host,
                    _routingSlip.TrackingNumber,
                    failureTimestamp,
                    routingSlipDuration,
                    exceptionInfo,
                    contents == RoutingSlipEventContents.All || contents.HasFlag(RoutingSlipEventContents.Variables)
                        ? variables
                        : EmptyObject));

            return Task.WhenAll(activityTask, slipTask);
        }

        async Task PublishEvent<T>(RoutingSlipEvents eventFlag, Func<RoutingSlipEventContents, T> messageFactory)
            where T : class
        {
            foreach (var subscription in _routingSlip.Subscriptions)
                await PublishSubscriptionEvent(eventFlag, messageFactory, subscription).ConfigureAwait(false);

            if (_routingSlip.Subscriptions.All(sub => sub.Events.HasFlag(RoutingSlipEvents.Supplemental)))
                await _publishEndpoint.Publish(messageFactory(RoutingSlipEventContents.All)).ConfigureAwait(false);
        }

        async Task PublishSubscriptionEvent<T>(RoutingSlipEvents eventFlag, Func<RoutingSlipEventContents, T> messageFactory, Subscription subscription)
            where T : class
        {
            if ((subscription.Events & RoutingSlipEvents.EventMask) == RoutingSlipEvents.All || subscription.Events.HasFlag(eventFlag))
            {
                var activityName = _context?.ActivityName;
                if (string.IsNullOrWhiteSpace(activityName) || string.IsNullOrWhiteSpace(subscription.ActivityName)
                    || activityName.Equals(subscription.ActivityName, StringComparison.OrdinalIgnoreCase))
                {
                    var endpoint = await _sendEndpointProvider.GetSendEndpoint(subscription.Address).ConfigureAwait(false);

                    var message = messageFactory(subscription.Include);

                    if (subscription.Message != null)
                    {
                        var adapter = new MessageEnvelopeContextAdapter(null, subscription.Message, JsonMessageSerializer.ContentTypeHeaderValue, message);

                        await endpoint.Send(message, adapter).ConfigureAwait(false);
                    }
                    else
                        await endpoint.Send(message).ConfigureAwait(false);
                }
            }
        }
    }
}
