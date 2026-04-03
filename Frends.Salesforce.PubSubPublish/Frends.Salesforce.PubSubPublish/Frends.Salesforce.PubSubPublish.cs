using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Eventbus.V1;
using Frends.Salesforce.PubSubPublish.Definitions;
using Frends.Salesforce.PubSubPublish.Helpers;

namespace Frends.Salesforce.PubSubPublish;

/// <summary>
/// Task Class for Salesforce operations.
/// </summary>
public static class Salesforce
{
    /// <summary>
    /// Task to publish salesforce messages with a Pub/Sub API
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Salesforce-PubSubPublish)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string MessageId, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> PubSubPublish(
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

            var topicInfo = await client.GetTopicAsync(
                new TopicRequest
                {
                    TopicName = input.TopicName,
                },
                headers: headers,
                cancellationToken: cancellationToken).ResponseAsync.ConfigureAwait(false);

            var schemaInfo = await client.GetSchemaAsync(
                new SchemaRequest
                {
                    SchemaId = topicInfo.SchemaId,
                },
                headers,
                cancellationToken: cancellationToken).ResponseAsync.ConfigureAwait(false);

            if (!topicInfo.CanSubscribe)
            {
                throw new InvalidOperationException(
                    $"Salesforce topic '{input.TopicName}' is not subscribable for the current credentials.");
            }

            var publishTaskResult = await GrpcHandler.PublishEventAsync(
                client,
                headers,
                input.TopicName,
                input.Payload,
                schemaInfo,
                input.Headers,
                cancellationToken).ConfigureAwait(false);

            if (publishTaskResult?.Error != null && !string.IsNullOrEmpty(publishTaskResult.Error.Msg))
                throw new Exception(publishTaskResult.Error.Msg);

            return new Result
            {
                Success = true,
                MessageId = publishTaskResult?.ReplayId != null
                    ? Convert.ToBase64String(publishTaskResult.ReplayId.ToByteArray())
                    : null,
                Error = null,
            };
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
