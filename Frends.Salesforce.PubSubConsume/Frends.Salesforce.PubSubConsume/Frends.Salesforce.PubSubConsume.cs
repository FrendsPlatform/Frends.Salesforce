using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Eventbus.V1;
using Frends.Common.Toolkit.Handlers;
using Frends.Salesforce.PubSubConsume.Definitions;
using Frends.Salesforce.PubSubConsume.Helpers;
using Frends.Salesforce.Toolkit.Handlers;
using Google.Protobuf;

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
        try
        {
            ValidationHandler.Run(input, connection);

            var accessToken = await ConnectionHandler.GetAccessToken(connection, cancellationToken);
            var client = ConnectionHandler.GetPubSubClient(connection);
            var headers = ConnectionHandler.GetMetadata(connection, accessToken);
            var result = new Result
            {
                Success = true,
                TopicName = input.TopicName,
            };
            var schemaCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                var topicInfo = await client.GetTopicAsync(
                    new TopicRequest
                    {
                        TopicName = input.TopicName,
                    },
                    headers: headers,
                    cancellationToken: cancellationToken).ResponseAsync.ConfigureAwait(false);

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

                if (input.WaitTimeoutSeconds > 0)
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(input.WaitTimeoutSeconds));
                using var call = client.Subscribe(headers: headers, cancellationToken: timeoutCts.Token);
                await call.RequestStream.WriteAsync(initialRequest, cancellationToken).ConfigureAwait(false);

                while (await call.ResponseStream.MoveNext(timeoutCts.Token).ConfigureAwait(false))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var response = call.ResponseStream.Current;
                    result.PendingNumRequested = response.PendingNumRequested;
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
                            timeoutCts.Token).ConfigureAwait(false);
                        result.Events.Add(GrpcHandler.MapEvent(consumerEvent, schemaJson));

                        if (result.Events.Count >= input.NumberOfEvents) break;
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
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
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
