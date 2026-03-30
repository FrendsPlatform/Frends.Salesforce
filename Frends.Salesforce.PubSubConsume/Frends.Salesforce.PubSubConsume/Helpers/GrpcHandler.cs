using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eventbus.V1;
using Frends.Salesforce.PubSubConsume.Definitions;
using Google.Protobuf;
using Grpc.Core;

namespace Frends.Salesforce.PubSubConsume.Helpers;

internal static class GrpcHandler
{
    internal static ReplayPreset MapReplayPreset(ReplayPresetOption replayPreset)
    {
        return replayPreset switch
        {
            ReplayPresetOption.Latest => ReplayPreset.Latest,
            ReplayPresetOption.Earliest => ReplayPreset.Earliest,
            ReplayPresetOption.Custom => ReplayPreset.Custom,
            _ => throw new InvalidOperationException($"Unsupported replay preset '{replayPreset}'."),
        };
    }

    internal static byte[] ParseReplayId(Input input)
    {
        if (input.ReplayPreset != ReplayPresetOption.Custom)
            return null;

        try
        {
            return Convert.FromBase64String(input.ReplayIdBase64);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("'ReplayIdBase64' is not valid Base64.", nameof(input), ex);
        }
    }

    internal static async Task<string> ResolveSchemaJsonAsync(
        PubSub.PubSubClient client,
        Metadata headers,
        string schemaId,
        bool resolveSchemas,
        Dictionary<string, string> schemaCache,
        CancellationToken cancellationToken)
    {
        if (!resolveSchemas || string.IsNullOrWhiteSpace(schemaId))
            return null;

        if (schemaCache.TryGetValue(schemaId, out var cachedSchema))
            return cachedSchema;

        var schemaInfo = await client.GetSchemaAsync(
            new SchemaRequest
            {
                SchemaId = schemaId,
            },
            headers: headers,
            cancellationToken: cancellationToken).ResponseAsync.ConfigureAwait(false);

        schemaCache.Add(schemaId, schemaInfo.SchemaJson);

        return schemaInfo.SchemaJson;
    }

    internal static ConsumedEvent MapEvent(ConsumerEvent consumerEvent, string schemaJson)
    {
        var replayIdBytes = consumerEvent.ReplayId?.ToByteArray() ?? [];
        var payloadBytes = consumerEvent.Event?.Payload?.ToByteArray() ?? [];

        return new ConsumedEvent
        {
            EventId = consumerEvent.Event?.Id,
            SchemaId = consumerEvent.Event?.SchemaId,
            ReplayIdBase64 = Convert.ToBase64String(replayIdBytes),
            PayloadBase64 = Convert.ToBase64String(payloadBytes),
            SchemaJson = schemaJson,
            Headers = consumerEvent.Event?.Headers?.Select(header => new ConsumedEventHeader
            {
                Key = header.Key,
                ValueBase64 = Convert.ToBase64String(header.Value?.ToByteArray() ?? []),
            }).ToList() ?? [],
        };
    }

    internal static void SetLatestReplayId(ByteString replayId, Result result)
    {
        var replayIdBytes = replayId?.ToByteArray();

        if (replayIdBytes == null || replayIdBytes.Length == 0)
            return;

        result.LatestReplayIdBase64 = Convert.ToBase64String(replayIdBytes);
    }
}
