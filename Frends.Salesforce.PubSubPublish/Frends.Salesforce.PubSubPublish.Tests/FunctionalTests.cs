using System.Threading;
using Frends.Salesforce.PubSubPublish.Definitions;
using NUnit.Framework;

namespace Frends.Salesforce.PubSubPublish.Tests;

[TestFixture]
public class FunctionalTests : TestBase
{
    [Test]
    public void ShouldRepeatContentWithDelimiter()
    {
        var input = new Input
        {
            Content = "foobar",
            Repeat = 3,
        };

        var connection = new Connection
        {
            ConnectionString = "Host=127.0.0.1;Port=12345",
        };

        var options = new Options
        {
            Delimiter = ", ",
            ThrowErrorOnFailure = true,
            ErrorMessageOnFailure = null,
        };

        var result = Salesforce.PubSubPublish(input, connection, options, CancellationToken.None);

        Assert.That(result.Output, Is.EqualTo("foobar, foobar, foobar"));
    }
}
