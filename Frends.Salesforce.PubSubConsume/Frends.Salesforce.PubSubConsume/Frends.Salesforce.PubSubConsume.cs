using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Eventbus.V1;
using Frends.Salesforce.PubSubConsume.Definitions;
using Frends.Salesforce.PubSubConsume.Helpers;
using Google.Protobuf;
using Grpc.Core;
using ValidationHandler = Frends.Salesforce.PubSubConsume.Helpers.ValidationHandler;

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
    public static async Task<Result> PubSubConsume(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var result = new Result
        {
            Success = true,
            TopicName = input.TopicName,
            EventCount = 0,
            Events = [],
        };

        try
        {
            ValidationHandler.Run(input, connection);

            var accessToken = await ConnectionHandler.GetAccessToken(connection, cancellationToken);
            var client = ConnectionHandler.GetPubSubClient(connection);
            var headers = ConnectionHandler.GetMetadata(connection, accessToken);

            var schemaCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using var call = client.Subscribe(headers: headers, cancellationToken: timeoutCts.Token);

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

                var initialRequest = new FetchRequest
                {
                    TopicName = input.TopicName,
                    NumRequested = input.NumberOfEvents,
                    ReplayPreset = GrpcHandler.MapReplayPreset(input.ReplayPreset),
                };

                var replayId = GrpcHandler.ParseReplayId(input);
                if (replayId != null) initialRequest.ReplayId = ByteString.CopyFrom(replayId);

                await call.RequestStream.WriteAsync(initialRequest, timeoutCts.Token).ConfigureAwait(false);

                if (input.WaitTimeoutSeconds > 0)
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(input.WaitTimeoutSeconds));

                while (await call.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                {
                    timeoutCts.Token.ThrowIfCancellationRequested();
                    var response = call.ResponseStream.Current;
                    GrpcHandler.SetLatestReplayId(response.LatestReplayId, result);

                    if (response.Events.Count == 0) break;

                    foreach (var consumerEvent in response.Events)
                    {
                        var schemaJson = await GrpcHandler.ResolveSchemaJsonAsync(
                            client,
                            headers,
                            consumerEvent.Event?.SchemaId,
                            options.ResolveSchemas,
                            schemaCache,
                            cancellationToken).ConfigureAwait(false);
                        result.Events.Add(GrpcHandler.MapEvent(consumerEvent, schemaJson));

                        if (result.Events.Count >= input.NumberOfEvents) break;
                    }

                    if (result.Events.Count >= input.NumberOfEvents)
                        break;
                }
            }
            catch (RpcException ex)
            {
                if (ex.InnerException is OperationCanceledException)
                {
                    if (!cancellationToken.IsCancellationRequested &&
                        timeoutCts.Token.IsCancellationRequested)
                    {
                        result.TimedOut = true;
                    }
                }
                else
                {
                    throw;
                }
            }

            await call.RequestStream.CompleteAsync().ConfigureAwait(false);
            result.EventCount = result.Events.Count;
            return result;
        }
        catch (Exception ex)
        {
            return result.TimedOut
                ? result
                : ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure, result);
        }
        finally
        {
            if (connection.ShutdownChannel)
            {
                await ConnectionHandler.ShutdownChannel().ConfigureAwait(false);
            }
        }
    }
}
