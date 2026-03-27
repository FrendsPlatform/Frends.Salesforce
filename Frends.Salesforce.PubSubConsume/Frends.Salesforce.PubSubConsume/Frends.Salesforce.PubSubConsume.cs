using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eventbus.V1;
using Frends.Salesforce.PubSubConsume.Definitions;
using Frends.Salesforce.PubSubConsume.Helpers;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;

namespace Frends.Salesforce.PubSubConsume;

/// <summary>
/// Task Class for Salesforce operations.
/// </summary>
public static class Salesforce
{
    /// <summary>
    /// Task to consume salesforce messages with a Pub/Sub API
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Salesforce-PubSubConsume)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string TopicName, int EventCount, bool TimedOut, string LatestReplayIdBase64, string LatestReplayIdHex, int PendingNumRequested, string LastRpcId, object[] Events, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result PubSubConsume(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            return PubSubConsumeInternalAsync(input, connection, options, cancellationToken).GetAwaiter().GetResult();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }

    private static async Task<Result> PubSubConsumeInternalAsync(
        Input input,
        Connection connection,
        Options options,
        CancellationToken cancellationToken)
    {
        ValidateArguments(input, connection, options);
        cancellationToken.ThrowIfCancellationRequested();

        var session = await SalesforceAuthentication.CreateSessionAsync(connection, cancellationToken)
            .ConfigureAwait(false);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (input.WaitTimeoutSeconds > 0)
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(input.WaitTimeoutSeconds));

        var endpointUri =
            SalesforceAuthentication.NormalizeUri(connection.PubSubApiUrl, nameof(connection.PubSubApiUrl));
        using var channel = GrpcChannel.ForAddress(endpointUri, new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true,
            },
        });

        var client = new PubSub.PubSubClient(channel);
        var headers = new Metadata
        {
            {
                "accesstoken", session.AccessToken
            },
            {
                "instanceurl", session.InstanceUrl
            },
            {
                "tenantid", connection.TenantId.Trim()
            },
        };

        var result = new Result
        {
            Success = true,
            TopicName = input.TopicName,
        };

        var schemaCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var topicInfo = await client.GetTopicAsync(
                new TopicRequest
                {
                    TopicName = input.TopicName,
                },
                headers: headers,
                cancellationToken: timeoutCts.Token).ResponseAsync.ConfigureAwait(false);

            if (!topicInfo.CanSubscribe)
            {
                throw new InvalidOperationException(
                    $"Salesforce topic '{input.TopicName}' is not subscribable for the current credentials.");
            }

            using var call = client.Subscribe(headers: headers, cancellationToken: timeoutCts.Token);

            var initialRequest = new FetchRequest
            {
                TopicName = input.TopicName,
                NumRequested = input.NumberOfEvents,
                ReplayPreset = MapReplayPreset(input.ReplayPreset),
            };

            var replayId = ParseReplayId(input);
            if (replayId != null)
                initialRequest.ReplayId = ByteString.CopyFrom(replayId);

            await call.RequestStream.WriteAsync(initialRequest, cancellationToken).ConfigureAwait(false);

            while (await call.ResponseStream.MoveNext(timeoutCts.Token).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var response = call.ResponseStream.Current;
                result.PendingNumRequested = response.PendingNumRequested;
                SetLatestReplayId(response.LatestReplayId, result);

                if (response.Events.Count == 0)
                    break;

                foreach (var consumerEvent in response.Events)
                {
                    var schemaJson = await ResolveSchemaJsonAsync(
                        client,
                        headers,
                        consumerEvent.Event?.SchemaId,
                        options.ResolveSchemas,
                        schemaCache,
                        timeoutCts.Token).ConfigureAwait(false);
                    result.Events.Add(MapEvent(consumerEvent, schemaJson));

                    if (result.Events.Count >= input.NumberOfEvents)
                        break;
                }

                if (result.Events.Count >= input.NumberOfEvents || response.PendingNumRequested <= 0)
                    break;
            }

            await call.RequestStream.CompleteAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested &&
                                                 timeoutCts.IsCancellationRequested)
        {
            result.TimedOut = true;
        }

        result.EventCount = result.Events.Count;

        return result;
    }

    private static void ValidateArguments(Input input, Connection connection, Options options)
    {
        ArgumentNullException.ThrowIfNull(input);

        ArgumentNullException.ThrowIfNull(connection);

        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(input.TopicName))
            throw new ArgumentException("'TopicName' must be provided.", nameof(input));

        if (input.NumberOfEvents < 1)
            throw new ArgumentException("'NumberOfEvents' must be greater than zero.", nameof(input));

        if (input.WaitTimeoutSeconds < 0)
            throw new ArgumentException("'WaitTimeoutSeconds' cannot be negative.", nameof(input));

        if (string.IsNullOrWhiteSpace(connection.TenantId))
            throw new ArgumentException("'TenantId' must be provided.", nameof(connection));

        if (input.ReplayPreset == ReplayPresetOption.Custom && string.IsNullOrWhiteSpace(input.ReplayIdBase64))
        {
            throw new ArgumentException(
                "'ReplayIdBase64' must be provided when ReplayPreset is Custom.",
                nameof(input));
        }

        if (input.ReplayPreset != ReplayPresetOption.Custom && !string.IsNullOrWhiteSpace(input.ReplayIdBase64))
        {
            throw new ArgumentException(
                "'ReplayIdBase64' can be used only when ReplayPreset is Custom.",
                nameof(input));
        }
    }

    private static ReplayPreset MapReplayPreset(ReplayPresetOption replayPreset)
    {
        return replayPreset switch
        {
            ReplayPresetOption.Latest => ReplayPreset.Latest,
            ReplayPresetOption.Earliest => ReplayPreset.Earliest,
            ReplayPresetOption.Custom => ReplayPreset.Custom,
            _ => throw new InvalidOperationException($"Unsupported replay preset '{replayPreset}'."),
        };
    }

    private static byte[] ParseReplayId(Input input)
    {
        if (input.ReplayPreset != ReplayPresetOption.Custom)
            return null;

        try
        {
            return Convert.FromBase64String(input.ReplayIdBase64.Trim());
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("'ReplayIdBase64' is not valid Base64.", nameof(input), ex);
        }
    }

    private static async Task<string> ResolveSchemaJsonAsync(
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

        schemaCache[schemaId] = schemaInfo.SchemaJson;

        return schemaInfo.SchemaJson;
    }

    private static ConsumedEvent MapEvent(ConsumerEvent consumerEvent, string schemaJson)
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

    private static void SetLatestReplayId(ByteString replayId, Result result)
    {
        var replayIdBytes = replayId?.ToByteArray();

        if (replayIdBytes == null || replayIdBytes.Length == 0)
            return;

        result.LatestReplayIdBase64 = Convert.ToBase64String(replayIdBytes);
    }
}
