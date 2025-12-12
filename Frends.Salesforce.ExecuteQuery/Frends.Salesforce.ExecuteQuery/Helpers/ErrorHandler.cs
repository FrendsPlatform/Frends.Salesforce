using System;

namespace Frends.Salesforce.ExecuteQuery.Helpers;

internal static class ErrorHandler
{
    internal static Result Handle(Exception exception, bool throwOnFailure, string token)
    {
        return throwOnFailure
            ? throw new Exception(exception.Message, exception)
            : new Result(null, false, exception, exception.Message, token);
    }
}