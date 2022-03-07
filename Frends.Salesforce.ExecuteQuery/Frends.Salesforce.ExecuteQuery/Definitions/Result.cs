using System;

namespace Frends.Salesforce.ExecuteQuery
{
    /// <summary>
    /// Result-class for ExecuteQuery-task.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Body of the response.
        /// </summary>
        public object Body { get; set; }

        /// <summary>
        /// Was the request successful?
        /// </summary>
        public bool RequestIsSuccessful { get; set; }

        /// <summary>
        /// Exception that was thrown by the server.
        /// </summary>
        public Exception ErrorException { get; set; }

        /// <summary>
        /// Error message from the server.
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Extended Result-class with access token.
    /// </summary>
    public class ResultWithToken : Result
    {
        /// <summary>
        /// OAuth2 access token.
        /// </summary>
        public string Token { get; set; }
    }
}
