using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avro;
using Avro.Generic;
using Avro.IO;
using Eventbus.V1;
using Frends.Salesforce.PubSubPublish.Definitions;
using Google.Protobuf;
using Grpc.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frends.Salesforce.PubSubPublish.Helpers;

internal static class GrpcHandler
{
    internal static async Task<PublishResult> PublishEventAsync(
        PubSub.PubSubClient client,
        Metadata headers,
        string topicName,
        string payloadJson,
        SchemaInfo schemaInfo,
        List<Header> customHeaders,
        CancellationToken cancellationToken)
    {
        var payloadBytes = SerializeJsonToAvro(payloadJson, schemaInfo.SchemaJson);

        var producerEvent = new ProducerEvent
        {
            Payload = ByteString.CopyFrom(payloadBytes),
            SchemaId = schemaInfo.SchemaId,
        };

        if (customHeaders != null)
        {
            foreach (var header in customHeaders)
            {
                producerEvent.Headers.Add(new EventHeader
                {
                    Key = header.Key,
                    Value = ByteString.CopyFromUtf8(header.Value ?? string.Empty),
                });
            }
        }

        var request = new PublishRequest
        {
            TopicName = topicName,
            AuthRefresh = string.Empty,
        };
        request.Events.Add(producerEvent);
        var response = await client.PublishAsync(request, headers, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return response.Results.FirstOrDefault();
    }

    internal static byte[] SerializeJsonToAvro(string jsonPayload, string schemaJson)
    {
        var schema = (RecordSchema)Schema.Parse(schemaJson);
        var jsonObject = JObject.Parse(jsonPayload);

        var missingFields = new List<string>();
        var record = BuildRecord(schema, jsonObject, string.Empty, missingFields);

        if (missingFields.Count > 0)
        {
            var fieldList = string.Join(Environment.NewLine, missingFields.Select(name => $" - {name}"));
            throw new Exception(
                $"JSON payload is missing required fields that have no default value and do not allow null:{Environment.NewLine}{fieldList}{Environment.NewLine}Schema used for validation:{Environment.NewLine}{schemaJson}");
        }

        using var ms = new MemoryStream();
        var writer = new GenericDatumWriter<GenericRecord>(schema);
        var encoder = new BinaryEncoder(ms);
        writer.Write(record, encoder);

        return ms.ToArray();
    }

    private static GenericRecord BuildRecord(
        RecordSchema schema,
        JObject json,
        string path,
        List<string> missingFields)
    {
        var record = new GenericRecord(schema);

        foreach (var field in schema.Fields)
        {
            var fieldPath = path + field.Name;

            if (json.TryGetValue(field.Name, out var value))
            {
                record.Add(field.Name, ConvertToken(value, field.Schema, fieldPath, missingFields));
            }
            else if (field.DefaultValue != null)
            {
                record.Add(field.Name, ConvertToken(field.DefaultValue, field.Schema, fieldPath, missingFields));
            }
            else if (field.Schema.Tag == Schema.Type.Null
                     || (field.Schema is UnionSchema union && union.Schemas.Any(s => s.Tag == Schema.Type.Null)))
            {
                record.Add(field.Name, null);
            }
            else
            {
                missingFields.Add(fieldPath);
            }
        }

        return record;
    }

    // Converts a JSON token to the CLR type the Avro writer expects for the given schema.
    private static object ConvertToken(JToken token, Schema schema, string path, List<string> missingFields)
    {
        if (schema is UnionSchema union)
        {
            if (token.Type == JTokenType.Null)
            {
                if (union.Schemas.Any(s => s.Tag == Schema.Type.Null))
                    return null;
                throw new Exception($"Field '{path}' is null but its schema does not allow null.");
            }

            foreach (var branch in union.Schemas.Where(s => s.Tag != Schema.Type.Null))
            {
                var branchMissing = new List<string>();
                try
                {
                    var converted = ConvertToken(token, branch, path, branchMissing);
                    missingFields.AddRange(branchMissing);
                    return converted;
                }
                catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException
                                               or JsonException or ArgumentException)
                {
                    // try next union branch
                }
            }

            throw new Exception($"Value of field '{path}' does not match any branch of the union schema {union}.");
        }

        try
        {
            switch (schema.Tag)
            {
                case Schema.Type.Null:
                    return null;
                case Schema.Type.Boolean:
                    return token.ToObject<bool>();
                case Schema.Type.Int:
                    return token.ToObject<int>();
                case Schema.Type.Long:
                    return token.ToObject<long>();
                case Schema.Type.Float:
                    return token.ToObject<float>();
                case Schema.Type.Double:
                    return token.ToObject<double>();
                case Schema.Type.String:
                    return token.ToObject<string>();
                case Schema.Type.Bytes:
                    return Encoding.Latin1.GetBytes(token.ToObject<string>());
                case Schema.Type.Fixed:
                    return new GenericFixed((FixedSchema)schema, Encoding.Latin1.GetBytes(token.ToObject<string>()));
                case Schema.Type.Enumeration:
                    return new GenericEnum((EnumSchema)schema, token.ToObject<string>());
                case Schema.Type.Record:
                    if (token is not JObject obj)
                        throw new FormatException($"Field '{path}' must be a JSON object.");
                    return BuildRecord((RecordSchema)schema, obj, path + ".", missingFields);
                case Schema.Type.Array:
                    if (token is not JArray array)
                        throw new FormatException($"Field '{path}' must be a JSON array.");
                    var itemSchema = ((ArraySchema)schema).ItemSchema;
                    return array.Select((item, i) => ConvertToken(item, itemSchema, $"{path}[{i}]", missingFields))
                        .ToArray();
                case Schema.Type.Map:
                    if (token is not JObject map)
                        throw new FormatException($"Field '{path}' must be a JSON object.");
                    var valueSchema = ((MapSchema)schema).ValueSchema;
                    return map.Properties().ToDictionary(
                        p => p.Name,
                        p => ConvertToken(p.Value, valueSchema, $"{path}.{p.Name}", missingFields));
                default:
                    return token.ToObject<object>();
            }
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException
                                       or JsonException or ArgumentException)
        {
            throw new FormatException(
                $"Value of field '{path}' cannot be converted to Avro type '{schema.Tag}': {ex.Message}", ex);
        }
    }
}
