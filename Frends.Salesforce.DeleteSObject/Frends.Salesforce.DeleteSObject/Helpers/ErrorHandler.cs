using System;
using Frends.Salesforce.DeleteSObject.Definitions;

namespace Frends.Salesforce.DeleteSObject.Helpers;

internal static class ErrorHandler
{
    internal static Result Handle(Exception exception, bool throwOnFailure, string token)
    {
        return throwOnFailure
            ? throw new Exception(exception.Message, exception)
            : new Result(null, false, exception, exception.Message, token);
    }
}