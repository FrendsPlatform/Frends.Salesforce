using System.Threading;
using System.Threading.Tasks;
using Frends.Salesforce.PubSubPublish.Definitions;
using NUnit.Framework;

namespace Frends.Salesforce.PubSubPublish.Tests;

[TestFixture]
public class FunctionalTests : TestBase
{
    private const string TestTopicName = "/event/Test_Event__e";
    private const string TestPayload = """
                                       {
                                         "CreatedDate": 1775086025169,
                                         "CreatedById": "005fj000009DcVJAA0",
                                         "Message__c": "Hello from test!"
                                       }
                                       """;

    private static Connection connection;
    private static Input input;
    private static Options options;

    [SetUp]
    public void SetUp()
    {
        input = new Input
        {
            TopicName = TestTopicName,
            Payload = TestPayload,
        };

        connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.OAuth2WithPassword,
            PubSubApiUrl = PubSubApiUrl,
            InstanceUrl = InstanceUrl,
            TenantId = TenantId,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            Username = Username,
            Password = Password,
            SecurityToken = SecurityToken,
        };

        options = new Options
        {
            ThrowErrorOnFailure = false,
            ErrorMessageOnFailure = null,
        };
    }

    [Test]
    public async Task PublishSuccessfully()
    {
        var result = await Salesforce.PubSubPublish(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True, "Publish should succeed");
        Assert.That(result.MessageId, Is.Not.Null.And.Not.Empty, "MessageId should be returned");
        Assert.That(result.Error, Is.Null, "Error should be null");
    }

    [Test]
    public async Task PublishFailedWhenTopicDoesNotExist()
    {
        input.TopicName = "invalid-topic-name";
        var result = await Salesforce.PubSubPublish(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("PermissionDenied"));
    }

    [Test]
    public async Task PublishFailedWhenPayloadIsInvalid()
    {
        input.Payload = """
                        {
                          "CreatedById": "005fj000009DcVJAA0",
                          "Message__c": "Hello from test!"
                        }
                        """;
        var result = await Salesforce.PubSubPublish(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("Missing field 'CreatedDate' in JSON payload."));
    }
}
