using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avro;
using Avro.Generic;
using Avro.IO;
using Eventbus.V1;
using Frends.Salesforce.PubSubPublish.Definitions;
using Google.Protobuf;
using Grpc.Core;
using Newtonsoft.Json.Linq;

namespace Frends.Salesforce.PubSubPublish.Helpers;

internal static class GrpcHandler
{
    internal static async Task<PublishResult> PublishEventAsync(
        PubSub.PubSubClient client,
        Metadata headers,
        string topicName,
        string payloadJson,
        SchemaInfo schemaInfo,
        List<Header> customHeaders,
        CancellationToken cancellationToken)
    {
        var payloadBytes = SerializeJsonToAvro(payloadJson, schemaInfo.SchemaJson);

        var producerEvent = new ProducerEvent
        {
            Payload = ByteString.CopyFrom(payloadBytes),
            SchemaId = schemaInfo.SchemaId,
        };

        if (customHeaders != null)
        {
            foreach (var header in customHeaders)
            {
                producerEvent.Headers.Add(new EventHeader
                {
                    Key = header.Key,
                    Value = ByteString.CopyFromUtf8(header.Value ?? string.Empty),
                });
            }
        }

        var request = new PublishRequest
        {
            TopicName = topicName,
            AuthRefresh = string.Empty,
        };
        request.Events.Add(producerEvent);
        var response = await client.PublishAsync(request, headers, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return response.Results.FirstOrDefault();
    }

    private static byte[] SerializeJsonToAvro(string jsonPayload, string schemaJson)
    {
        var schema = (RecordSchema)Schema.Parse(schemaJson);
        var record = new GenericRecord(schema);
        var jsonObject = JObject.Parse(jsonPayload);

        foreach (var field in schema.Fields)
        {
            if (jsonObject.TryGetValue(field.Name, out var value))
            {
                record.Add(field.Name, value.ToObject<object>());
            }
            else
            {
                throw new Exception($"Missing field '{field.Name}' in JSON payload.");
            }
        }

        using var ms = new MemoryStream();
        var writer = new GenericDatumWriter<GenericRecord>(schema);
        var encoder = new BinaryEncoder(ms);
        writer.Write(record, encoder);

        return ms.ToArray();
    }
}
