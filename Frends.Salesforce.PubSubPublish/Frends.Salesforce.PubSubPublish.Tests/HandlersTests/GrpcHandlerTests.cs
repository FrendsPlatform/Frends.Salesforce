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

    [TestCase("int", "0", 0)]
    [TestCase("long", "5", 5L)]
    [TestCase("float", "1", 1.0f)]
    [TestCase("double", "2", 2.0d)]
    [TestCase("double", "2.5", 2.5d)]
    [TestCase("boolean", "true", true)]
    public void ShouldUseNumericDefaultConvertedToSchemaType(string type, string defaultJson, object expected)
    {
        var schema = $$"""
            {"type":"record","name":"Evt","fields":[{"name":"Count","type":"{{type}}","default":{{defaultJson}}}]}
            """;

        var bytes = GrpcHandler.SerializeJsonToAvro("{}", schema);

        Assert.That(Deserialize(bytes, schema)["Count"], Is.EqualTo(expected));
    }

    [TestCase("int", "7", 7)]
    [TestCase("long", "7", 7L)]
    [TestCase("float", "7", 7.0f)]
    [TestCase("double", "7", 7.0d)]
    [TestCase("double", "7.5", 7.5d)]
    public void ShouldConvertPayloadNumbersToSchemaType(string type, string valueJson, object expected)
    {
        var schema = $$"""
            {"type":"record","name":"Evt","fields":[{"name":"Count","type":"{{type}}"}]}
            """;

        var bytes = GrpcHandler.SerializeJsonToAvro($$"""{"Count":{{valueJson}}}""", schema);

        Assert.That(Deserialize(bytes, schema)["Count"], Is.EqualTo(expected));
    }

    [Test]
    public void ShouldConvertPayloadValueInNullableUnion()
    {
        const string schema = """
            {"type":"record","name":"Evt","fields":[{"name":"Opt","type":["null","int"]}]}
            """;

        var bytes = GrpcHandler.SerializeJsonToAvro("""{"Opt":3}""", schema);

        Assert.That(Deserialize(bytes, schema)["Opt"], Is.EqualTo(3));
    }

    [Test]
    public void ShouldWriteNullForExplicitJsonNullInNullableUnion()
    {
        const string schema = """
            {"type":"record","name":"Evt","fields":[{"name":"Opt","type":["null","int"]}]}
            """;

        var bytes = GrpcHandler.SerializeJsonToAvro("""{"Opt":null}""", schema);

        Assert.That(Deserialize(bytes, schema)["Opt"], Is.Null);
    }

    [Test]
    public void ShouldUseDefaultOfNonNullUnionBranch()
    {
        const string schema = """
            {"type":"record","name":"Evt","fields":[{"name":"Opt","type":["int","null"],"default":4}]}
            """;

        var bytes = GrpcHandler.SerializeJsonToAvro("{}", schema);

        Assert.That(Deserialize(bytes, schema)["Opt"], Is.EqualTo(4));
    }

    [Test]
    public void ShouldConvertNestedRecordArrayAndMapValues()
    {
        const string schema = """
            {"type":"record","name":"Evt","fields":[
                {"name":"Inner","type":{"type":"record","name":"Inner","fields":[
                    {"name":"N","type":"int"},
                    {"name":"L","type":"long","default":9}]}},
                {"name":"Arr","type":{"type":"array","items":"int"}},
                {"name":"Map","type":{"type":"map","values":"float"}}]}
            """;

        var bytes = GrpcHandler.SerializeJsonToAvro(
            """{"Inner":{"N":1},"Arr":[1,2,3],"Map":{"a":1,"b":2.5}}""",
            schema);

        var result = Deserialize(bytes, schema);
        var inner = (GenericRecord)result["Inner"];
        Assert.That(inner["N"], Is.EqualTo(1));
        Assert.That(inner["L"], Is.EqualTo(9L));
        Assert.That((object[])result["Arr"], Is.EqualTo(new object[] { 1, 2, 3 }));
        var map = (System.Collections.IDictionary)result["Map"];
        Assert.That(map["a"], Is.EqualTo(1.0f));
        Assert.That(map["b"], Is.EqualTo(2.5f));
    }

    [Test]
    public void ShouldConvertEnumAndBytesValues()
    {
        const string schema = """
            {"type":"record","name":"Evt","fields":[
                {"name":"E","type":{"type":"enum","name":"Color","symbols":["RED","BLUE"]},"default":"BLUE"},
                {"name":"B","type":"bytes"}]}
            """;

        var bytes = GrpcHandler.SerializeJsonToAvro("""{"B":"abc"}""", schema);

        var result = Deserialize(bytes, schema);
        Assert.That(((GenericEnum)result["E"]).Value, Is.EqualTo("BLUE"));
        Assert.That((byte[])result["B"], Is.EqualTo(new byte[] { 97, 98, 99 }));
    }

    private static GenericRecord Deserialize(byte[] bytes, string schemaJson)
    {
        var schema = (RecordSchema)Schema.Parse(schemaJson);
        var reader = new GenericDatumReader<GenericRecord>(schema, schema);
        using var ms = new MemoryStream(bytes);
        return reader.Read(null, new BinaryDecoder(ms));
    }
}
