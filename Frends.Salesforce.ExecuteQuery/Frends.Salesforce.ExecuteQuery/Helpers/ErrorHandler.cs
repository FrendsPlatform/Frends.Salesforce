using System;
using Frends.Salesforce.ExecuteQuery.Definitions;

namespace Frends.Salesforce.ExecuteQuery.Helpers;

internal static class ErrorHandler
{
    internal static Result Handle(Exception exception, string additionalMessage = "")
    {
        if (string.IsNullOrEmpty(additionalMessage))
            throw new Exception(exception.Message, exception);

        throw new Exception(additionalMessage, exception);
    }
}