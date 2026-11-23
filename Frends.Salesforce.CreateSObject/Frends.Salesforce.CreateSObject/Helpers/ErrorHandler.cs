namespace Frends.Salesforce.CreateSObject.Helpers;

using System;
using Definitions;

internal static class ErrorHandler
{
    internal static Result Handle(this Exception exception, Definitions.Options options)
    {
        if (options.ThrowErrorOnFailure)
        {
            if (string.IsNullOrEmpty(options.ErrorMessageOnFailure))
                throw new Exception(exception.Message, exception);

            throw new Exception(options.ErrorMessageOnFailure, exception);
        }

        var errorMessage = !string.IsNullOrEmpty(options.ErrorMessageOnFailure)
            ? $"{options.ErrorMessageOnFailure}: {exception.Message}"
            : exception.Message;
        return new Result(null, false, exception.ToString(), errorMessage, null);
    }
}
