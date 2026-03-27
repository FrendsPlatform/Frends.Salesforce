using System;
using System.IO;
using System.Threading;
using Avro;
using Avro.Generic;
using Avro.IO;
using Frends.Salesforce.PubSubConsume.Definitions;
using NUnit.Framework;

namespace Frends.Salesforce.PubSubConsume.Tests;

[TestFixture]
public class FunctionalTests : TestBase
{
    [Test]
    public void ConsumeSuccessful()
    {
        var input = new Input
        {
            TopicName = "/event/Test_Event__e",
            NumberOfEvents = 3,
            ReplayPreset = ReplayPresetOption.Earliest,
            WaitTimeoutSeconds = 0,
        };

        var connection = new Connection
        {
            AuthenticationMethod = AuthenticationMethod.UsernamePasswordOAuth,
            LoginUrl = LoginUrl,
            PubSubApiUrl = PubSubApiUrl,
            InstanceUrl = InstanceUrl,
            TenantId = TenantId,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            Username = Username,
            Password = Password,
            SecurityToken = SecurityToken,
        };

        var options = new Options
        {
            ResolveSchemas = true,
            ThrowErrorOnFailure = false,
            ErrorMessageOnFailure = null,
        };

        var result = Salesforce.PubSubConsume(input, connection, options, CancellationToken.None);

        var bytes = Convert.FromBase64String(result.Events[0].PayloadBase64);
        using var stream = new MemoryStream(bytes);
        var schema = Schema.Parse(result.Events[0].SchemaJson);

        var reader = new GenericReader<GenericRecord>(schema, schema);
        var decoder = new BinaryDecoder(stream);
        var record = reader.Read(null, decoder);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Error, Is.Null);
            Assert.That(record.GetValue(2), Does.Contain("Hello"));
        });
    }
}
