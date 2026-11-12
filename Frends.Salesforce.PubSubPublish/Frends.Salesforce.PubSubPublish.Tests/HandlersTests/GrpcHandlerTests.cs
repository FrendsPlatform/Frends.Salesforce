using System;
using System.IO;
using Avro;
using Avro.Generic;
using Avro.IO;
using Frends.Salesforce.PubSubPublish.Helpers;
using NUnit.Framework;

namespace Frends.Salesforce.PubSubPublish.Tests.HandlersTests;

[TestFixture]
public class GrpcHandlerTests
{
    [Test]
    public void ShouldUseValueFromPayload()
    {
        const string schema = """
            {"type":"record","name":"Evt","fields":[{"name":"Name","type":"string"}]}
            """;

        var bytes = GrpcHandler.SerializeJsonToAvro("""{"Name":"foo"}""", schema);

        Assert.That(Deserialize(bytes, schema)["Name"], Is.EqualTo("foo"));
    }

    [Test]
    public void ShouldUseDefaultValueWhenFieldIsMissing()
    {
        const string schema = """
            {"type":"record","name":"Evt","fields":[
                {"name":"Name","type":"string","default":"fallback"}]}
            """;

        var bytes = GrpcHandler.SerializeJsonToAvro("{}", schema);

        Assert.That(Deserialize(bytes, schema)["Name"], Is.EqualTo("fallback"));
    }

    [Test]
    public void ShouldUseNullWhenFieldIsMissingAndSchemaIsNullType()
    {
        const string schema = """
            {"type":"record","name":"Evt","fields":[{"name":"Nothing","type":"null"}]}
            """;

        var bytes = GrpcHandler.SerializeJsonToAvro("{}", schema);

        Assert.That(Deserialize(bytes, schema)["Nothing"], Is.Null);
    }

    [TestCase("string")]
    [TestCase("int")]
    public void ShouldUseNullWhenFieldIsMissingAndUnionContainsNull(string innerType)
    {
        var schema = $$"""
            {"type":"record","name":"Evt","fields":[{"name":"Opt","type":["null","{{innerType}}"]}]}
            """;

        var bytes = GrpcHandler.SerializeJsonToAvro("{}", schema);

        Assert.That(Deserialize(bytes, schema)["Opt"], Is.Null);
    }

    [Test]
    public void ShouldThrowListingAllMissingFieldsAndSchema()
    {
        const string schema = """
            {"type":"record","name":"Evt","fields":[
                {"name":"A","type":"string"},
                {"name":"B","type":"int"},
                {"name":"C","type":["null","string"]},
                {"name":"D","type":"string","default":"x"},
                {"name":"E","type":"string"}]}
            """;

        Action act = () => GrpcHandler.SerializeJsonToAvro("""{"E":"present"}""", schema);
        var ex = Assert.Throws<Exception>(act);

        Assert.That(ex.Message, Does.Contain(" - A"));
        Assert.That(ex.Message, Does.Contain(" - B"));
        Assert.That(ex.Message, Does.Not.Contain(" - C"));
        Assert.That(ex.Message, Does.Not.Contain(" - D"));
        Assert.That(ex.Message, Does.Not.Contain(" - E"));
        Assert.That(ex.Message, Does.Contain(schema));
    }

    private static GenericRecord Deserialize(byte[] bytes, string schemaJson)
    {
        var schema = (RecordSchema)Schema.Parse(schemaJson);
        var reader = new GenericDatumReader<GenericRecord>(schema, schema);
        using var ms = new MemoryStream(bytes);
        return reader.Read(null, new BinaryDecoder(ms));
    }
}
