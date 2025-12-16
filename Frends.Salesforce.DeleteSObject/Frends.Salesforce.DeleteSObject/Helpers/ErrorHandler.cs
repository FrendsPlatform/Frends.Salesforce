using System;
using Frends.Salesforce.DeleteSObject.Definitions;

namespace Frends.Salesforce.DeleteSObject.Helpers;

internal static class ErrorHandler
{
    internal static Result Handle(Exception exception, string additionalMessage = "")
    {
        if (string.IsNullOrEmpty(additionalMessage))
            throw new Exception(exception.Message, exception);

        throw new Exception(additionalMessage, exception);
    }
}