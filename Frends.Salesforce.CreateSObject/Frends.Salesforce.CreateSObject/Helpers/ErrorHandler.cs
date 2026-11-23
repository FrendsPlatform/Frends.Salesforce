namespace Frends.Salesforce.CreateSObject.Helpers;

using System;
using Frends.Salesforce.CreateSObject.Definitions;

internal static class ErrorHandler
{
    internal static Result Handle(
        this Exception exception,
        Options options,
        Result responseResult = null,
        string additionalMessage = null)
    {
        var message = additionalMessage ?? exception.Message;

        if (options.ThrowErrorOnFailure)
        {
            if (string.IsNullOrEmpty(options.ErrorMessageOnFailure))
                throw new Exception(message, exception);

            throw new Exception(options.ErrorMessageOnFailure, exception);
        }

        var errorMessage = !string.IsNullOrEmpty(options.ErrorMessageOnFailure)
            ? $"{options.ErrorMessageOnFailure}: {message}"
            : message;
        return new Result(
            responseResult?.Body,
            false,
            exception.ToString(),
            errorMessage,
            responseResult?.Token);
    }
}
