namespace MassTransit.Tests.MessageData
{
    using System;
    using System.Threading.Tasks;
    using GreenPipes;
    using MassTransit.MessageData;
    using NUnit.Framework;
    using TestFramework;


    [TestFixture]
    public class Sending_a_request_with_response_message_data :
        InMemoryTestFixture
    {
        [Test]
        public async Task Should_load_the_data_from_the_repository()
        {
            IRequestClient<Request> client = CreateRequestClient<Request>();

            Response<Response> response = await client.GetResponse<Response>(new {Key = "Hello"});

            Assert.That(await response.Message.Value.Value, Is.Not.Empty);
        }

        [Test]
        public async Task Should_load_the_data_from_the_repository_via_connect()
        {
            IRequestClient<Request> client = await ConnectRequestClient<Request>();

            Response<Response> response = await client.GetResponse<Response>(new {Key = "Hello"});

            Assert.That(await response.Message.Value.Value, Is.Not.Empty);
        }

        readonly IMessageDataRepository _repository = new InMemoryMessageDataRepository();
        Task<ConsumeContext<Request>> _received;

        protected override void ConfigureInMemoryBus(IInMemoryBusFactoryConfigurator configurator)
        {
            configurator.UseRetry<Response>(r => r.Immediate(1));

            configurator.UseMessageData<Response>(_repository);
        }

        protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
            configurator.UseMessageRetry(r => r.None());

            _received = Handler<Request>(configurator, async context =>
            {
                await context.RespondAsync<Response>(new
                {
                    context.Message.Key,
                    Value = await _repository.PutString("This is a huge string, and it is just too big to fit.")
                });
            });
        }


        public interface Request
        {
            string Key { get; }
        }


        public interface Response
        {
            string Key { get; }
            MessageData<string> Value { get; }
        }
    }
}
