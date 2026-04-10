using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.PubSubConsume.Definitions;
using NUnit.Framework;

namespace Frends.Salesforce.PubSubConsume.Tests;

[TestFixture]
public class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.ThrowsAsync<Exception>(() =>
            Salesforce.PubSubConsume(new Input(), new Connection(), new Options(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = new Options
        {
            ThrowErrorOnFailure = false,
        };
        var result =
            await Salesforce.PubSubConsume(new Input(), new Connection(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = new Options
        {
            ErrorMessageOnFailure = CustomErrorMessage,
        };
        var ex = Assert.ThrowsAsync<Exception>(() =>
            Salesforce.PubSubConsume(new Input(), new Connection(), options, CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }
}
