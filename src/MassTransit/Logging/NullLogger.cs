namespace MassTransit.Logging
{
    using System;
    using Microsoft.Extensions.Logging;


    public class NullLogger :
        ILogger
    {
        NullLogger()
        {
        }

        public static NullLogger Instance { get; } = new NullLogger();

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return false;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }
    }
}
