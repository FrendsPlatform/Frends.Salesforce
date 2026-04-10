using System;
using Frends.Salesforce.PubSubConsume.Definitions;

namespace Frends.Salesforce.PubSubConsume.Helpers;

internal static class ErrorHandler
{
    internal static Result Handle(
        Exception exception,
        bool throwOnFailure,
        string errorMessageOnFailure,
        Result result = null)
    {
        if (throwOnFailure)
        {
            if (string.IsNullOrEmpty(errorMessageOnFailure))
                throw new Exception(exception.Message, exception);

            throw new Exception(errorMessageOnFailure, exception);
        }

        var errorMessage = !string.IsNullOrEmpty(errorMessageOnFailure)
            ? $"{errorMessageOnFailure}: {exception.Message}"
            : exception.Message;

        if (result is null)
        {
            return new Result
            {
                Success = false,
                Error = new Error
                {
                    Message = errorMessage,
                    AdditionalInfo = exception,
                },
            };
        }

        result.Success = false;
        result.Error = new Error
        {
            Message = errorMessage,
            AdditionalInfo = exception,
        };

        return result;
    }
}
