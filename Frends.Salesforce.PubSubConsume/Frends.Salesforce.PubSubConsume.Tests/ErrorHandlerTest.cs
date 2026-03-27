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
            Salesforce.PubSubConsume(DefaultInput(), DefaultConnection(), DefaultOptions(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result =
            await Salesforce.PubSubConsume(DefaultInput(), DefaultConnection(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;
        var ex = Assert.ThrowsAsync<Exception>(() =>
            Salesforce.PubSubConsume(DefaultInput(), DefaultConnection(), options, CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }

    private static Input DefaultInput() => new()
    {
        TopicName = "/event/My_Event__e",
        NumberOfEvents = 0,
    };

    private static Connection DefaultConnection() => new()
    {
        TenantId = "00Dxx0000000001",
    };

    private static Options DefaultOptions() => new()
    {
        ThrowErrorOnFailure = true,
        ErrorMessageOnFailure = string.Empty,
    };
}
