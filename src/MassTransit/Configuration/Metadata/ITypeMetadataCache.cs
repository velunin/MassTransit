namespace MassTransit.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;


    public interface ITypeMetadataCache<out T>
    {
        string ShortName { get; }

        /// <summary>
        /// True if the type implements any known saga interfaces
        /// </summary>
        bool HasConsumerInterfaces { get; }

        /// <summary>
        /// True if the type implements any known saga interfaces
        /// </summary>
        bool HasSagaInterfaces { get; }

        /// <summary>
        /// True if the message type is a valid message type
        /// </summary>
        bool IsValidMessageType { get; }

        /// <summary>
        /// Once checked, the reason why the message type is invalid
        /// </summary>
        string InvalidMessageTypeReason { get; }

        /// <summary>
        /// True if this message is not a public type
        /// </summary>
        bool IsTemporaryMessageType { get; }

        /// <summary>
        /// Returns all valid message types that are contained within the s
        /// </summary>
        Type[] MessageTypes { get; }

        /// <summary>
        /// The names of all the message types supported by the message type
        /// </summary>
        string[] MessageTypeNames { get; }

        IEnumerable<PropertyInfo> Properties { get; }

        /// <summary>
        /// The implementation type for the type, if it's an interface
        /// </summary>
        Type ImplementationType { get; }
    }
}
