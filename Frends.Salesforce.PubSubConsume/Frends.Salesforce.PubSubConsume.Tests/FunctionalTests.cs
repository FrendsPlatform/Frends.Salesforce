using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avro;
using Avro.Generic;
using Avro.IO;
using Eventbus.V1;
using Frends.Salesforce.PubSubConsume.Definitions;
using Frends.Salesforce.PubSubConsume.Helpers;
using Google.Protobuf;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.Salesforce.PubSubConsume.Tests;

[TestFixture]
public class FunctionalTests : TestBase
{
    private const string TestTopicName = "/event/Test_Event__e";
    private const string TestSchemaId = "6L0yfG1TjHj3IeYaje5uZQ";

    private const string TestSchemaJson = """
                                          {
                                            "type" : "record",
                                            "name" : "Test_Event__e",
                                            "namespace" : "com.sforce.eventbus",
                                            "fields" : [ {
                                              "name" : "CreatedDate",
                                              "type" : "long",
                                              "doc" : "CreatedDate:DateTime"
                                            }, {
                                              "name" : "CreatedById",
                                              "type" : "string",
                                              "doc" : "CreatedBy:EntityId"
                                            }, {
                                              "name" : "Message__c",
                                              "type" : [ "null", "string" ],
                                              "doc" : "Data:Text:00Nfj00003DOOJV",
                                              "default" : null
                                            } ]
                                          }
                                          """;

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

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        SetUp();
        await PublishEvents(3).ConfigureAwait(false);
    }

    [SetUp]
    public void SetUp()
    {
        input = new Input
        {
            TopicName = TestTopicName,
            NumberOfEvents = 1,
            ReplayPreset = ReplayPresetOption.Earliest,
            WaitTimeoutSeconds = 5,
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
            ResolveSchemas = true,
            ThrowErrorOnFailure = false,
            ErrorMessageOnFailure = null,
        };
    }

    [Test]
    public async Task ConsumeExactlyMaxMessages()
    {
        var result = await Salesforce.PubSubConsume(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.EventCount, Is.EqualTo(1), result.Error?.Message);
    }

    [Test]
    public async Task ConsumeStoppedAfterWaitTimeout_ReturnsSomeEvents()
    {
        input.NumberOfEvents = 100;
        input.WaitTimeoutSeconds = 3;
        var result = await Salesforce.PubSubConsume(input, connection, options, CancellationToken.None);

        Assert.That(result.TimedOut, Is.True);
        Assert.That(result.EventCount, Is.GreaterThan(0));
        Assert.That(result.EventCount, Is.LessThan(100));
    }

    [Test]
    public async Task ConsumedMessageCanBeCorrectlyParsed()
    {
        var result = await Salesforce.PubSubConsume(input, connection, options, CancellationToken.None);
        var messages = new List<string>();

        foreach (var ev in result.Events)
        {
            var bytes = Convert.FromBase64String(ev.PayloadBase64);
            using var stream = new MemoryStream(bytes);
            var schema = Schema.Parse(ev.SchemaJson);

            var reader = new GenericReader<GenericRecord>(schema, schema);
            var decoder = new BinaryDecoder(stream);

            try
            {
                var record = reader.Read(null, decoder);
                messages.Add(record.GetValue(2).ToString());
            }
            catch (Exception e)
            {
                TestContext.WriteLine($"Failed to decode event ({ev.EventId}) with error: {e}");
            }
        }

        Assert.That(messages.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task ConsumeReturnsWhenMaxEventAchieved()
    {
        input.NumberOfEvents = 1;
        input.WaitTimeoutSeconds = 0;

        var result = await Salesforce.PubSubConsume(input, connection, options, CancellationToken.None);

        Assert.That(result.EventCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ConsumeStoppedWithCancellationToken_ReturnsSomeEvents()
    {
        input.NumberOfEvents = 100;
        input.WaitTimeoutSeconds = 0;

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        var result = await Salesforce.PubSubConsume(input, connection, options, cts.Token);

        Assert.That(result.TimedOut, Is.False);
        Assert.That(result.EventCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task ConsumeReturnsEmptyListWhenReplayPresetIsLatest()
    {
        // It would require sending an event during the test to fetch any event, so nothing should be fetched
        input.ReplayPreset = ReplayPresetOption.Latest;
        var result = await Salesforce.PubSubConsume(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.EventCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ConsumeReusesExistingChannel()
    {
        connection.ShutdownChannel = false;
        var result = await Salesforce.PubSubConsume(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        connection.ShutdownChannel = true;
        result = await Salesforce.PubSubConsume(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task FailsWhenTopicNotFound()
    {
        input.TopicName = "invalid-topic-name";
        var result = await Salesforce.PubSubConsume(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("PermissionDenied"));
    }

    private static async Task PublishEvents(int count)
    {
        var accessToken = await ConnectionHandler.GetAccessToken(connection, CancellationToken.None);
        var client = ConnectionHandler.GetPubSubClient(connection);
        var headers = ConnectionHandler.GetMetadata(connection, accessToken);
        var payloadBytes = SerializeJsonToAvro(TestPayload, TestSchemaJson);
        var producerEvent = new ProducerEvent
        {
            Payload = ByteString.CopyFrom(payloadBytes),
            SchemaId = TestSchemaId,
        };
        var request = new PublishRequest
        {
            TopicName = TestTopicName,
            AuthRefresh = string.Empty,
        };

        for (int i = 0; i < count; i++)
        {
            request.Events.Add(producerEvent);
        }

        var response = await client.PublishAsync(request, headers, cancellationToken: CancellationToken.None)
            .ConfigureAwait(false);
        await ConnectionHandler.ShutdownChannel();
        Assert.That(response.Results.Count, Is.EqualTo(count), "Failed to publish some test events");
    }

    private static byte[] SerializeJsonToAvro(string jsonPayload, string schemaJson)
    {
        var schema = (RecordSchema)Schema.Parse(schemaJson);
        var record = new GenericRecord(schema);
        var jsonObject = JObject.Parse(jsonPayload);

        foreach (var field in schema.Fields)
        {
            var value = jsonObject.GetValue(field.Name);
            record.Add(field.Name, value!.ToObject<object>());
        }

        using var ms = new MemoryStream();
        var writer = new GenericDatumWriter<GenericRecord>(schema);
        var encoder = new BinaryEncoder(ms);
        writer.Write(record, encoder);

        return ms.ToArray();
    }
}
