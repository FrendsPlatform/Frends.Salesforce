using System;
using Frends.Salesforce.CreateSObject.Definitions;
using Frends.Salesforce.CreateSObject.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.Salesforce.CreateSObject.Tests;

[TestClass]
public class ResultTests
{
    [TestMethod]
    public void SuccessfulResultExposesSuccessAndNoError()
    {
        var result = new Result(new { id = "001xx000003DGbY" }, true, null, null, null);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.RequestIsSuccessful);
        Assert.IsNull(result.Error);
    }

    [TestMethod]
    public void FailureHandlerReturnsResultWhenThrowingIsDisabled()
    {
        var options = new Options
        {
            ThrowErrorOnFailure = false,
            ErrorMessageOnFailure = "Record creation failed",
        };

        var result = new InvalidOperationException("Salesforce rejected the request").Handle(options);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.AreEqual("Record creation failed: Salesforce rejected the request", result.Error.Message);
        Assert.IsTrue(result.Error.AdditionalInfo.Contains("Salesforce rejected the request"));
        Assert.IsInstanceOfType<string>(result.ErrorException);
    }
}
