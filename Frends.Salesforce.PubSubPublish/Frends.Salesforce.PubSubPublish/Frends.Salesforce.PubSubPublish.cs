using System;
using System.ComponentModel;
using System.Threading;
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
    public static Result PubSubPublish(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, connection);
            var accessTokenTask = ConnectionHandler.GetAccessToken(connection, cancellationToken);
            accessTokenTask.Wait(cancellationToken);
            var accessToken = accessTokenTask.Result;
            var client = ConnectionHandler.GetPubSubClient(connection);
            var metadata = ConnectionHandler.GetMetadata(connection, accessToken);
            var publishTask = GrpcHandler.PublishEventAsync(
                client,
                metadata,
                input.TopicName,
                input.Payload,
                input.Headers,
                cancellationToken);
            publishTask.Wait(cancellationToken);
            var result = publishTask.Result;

            if (result?.Error != null && !string.IsNullOrEmpty(result.Error.Msg))
            {
                return new Result
                {
                    Success = false,
                    MessageId = null,
                    Error = new Error
                    {
                        Message = result.Error.Msg,
                        AdditionalInfo = null,
                    },
                };
            }

            return new Result
            {
                Success = true,
                MessageId = result?.ReplayId != null ? Convert.ToBase64String(result.ReplayId.ToByteArray()) : null,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}
