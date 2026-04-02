using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eventbus.V1;
using Frends.Salesforce.PubSubPublish.Definitions;
using Google.Protobuf;
using Grpc.Core;

namespace Frends.Salesforce.PubSubPublish.Helpers;

internal static class GrpcHandler
{
    internal static async Task<PublishResult> PublishEventAsync(
        PubSub.PubSubClient client,
        Metadata headers,
        string topicName,
        string payload,
        List<Header> customHeaders,
        CancellationToken cancellationToken)
    {
        var producerEvent = new ProducerEvent
        {
            Id = Guid.NewGuid().ToString(),
            Payload = ByteString.CopyFromUtf8(payload),
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
        var response = await client.PublishAsync(request, headers: headers, cancellationToken: cancellationToken);

        return response.Results.FirstOrDefault();
    }
}
