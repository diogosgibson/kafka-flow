namespace KafkaFlow.Client.Protocol
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using KafkaFlow.Client.Protocol.Messages;
    using KafkaFlow.Client.Protocol.Streams;

    public class Request : IRequest
    {
        public int CorrelationId { get; }
        public string ClientId { get; }
        public TaggedField[] TaggedFields => Array.Empty<TaggedField>();
        public IRequestMessage Message { get; }

        public Request(
            int correlationId,
            string clientId,
            IRequestMessage message)
        {
            this.CorrelationId = correlationId;
            this.ClientId = clientId;
            this.Message = message;
        }

        public void Write(Stream destination)
        {
            using var tmp = new DynamicMemoryStream(MemoryManager.Instance);

            tmp.WriteInt16((short) this.Message.ApiKey);
            tmp.WriteInt16(this.Message.ApiVersion);
            tmp.WriteInt32(this.CorrelationId);
            tmp.WriteString(this.ClientId);

            if (this.Message is ITaggedFields)
                tmp.WriteTaggedFields(this.TaggedFields);

            tmp.WriteMessage(this.Message);

            destination.WriteInt32(Convert.ToInt32(tmp.Length));

            Debug.WriteLine($"Sending {this.Message.GetType().Name} with {tmp.Length:N0}b");

            tmp.Position = 0;
            tmp.CopyTo(destination);
        }
    }
}