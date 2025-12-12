using System;

namespace Frends.Salesforce.ExecuteQuery.Helpers;

internal static class ErrorHandler
{
    internal static Result Handle(Exception exception, bool throwOnFailure, string token, string additionalMessage = "")
    {
        if (throwOnFailure)
        {
            if (string.IsNullOrEmpty(additionalMessage))
                throw new Exception(exception.Message, exception);

            throw new Exception(additionalMessage, exception);
        }

        var errorMessage = !string.IsNullOrEmpty(additionalMessage)
            ? $"{additionalMessage}: {exception.Message}"
            : exception.Message;

        return new Result(null, false, exception, errorMessage, token);
    }
}