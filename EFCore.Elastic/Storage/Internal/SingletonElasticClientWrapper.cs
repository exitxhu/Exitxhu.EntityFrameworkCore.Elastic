// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Serialization;
using Elastic.Transport;

using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;

using Newtonsoft.Json;

using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SingletonElasticClientWrapper : ISingletonElasticClientWrapper
{
    private readonly string? _connectionString;
    private ElasticsearchClient _client;
    private ElasticsearchClientSettings _clientSetting;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SingletonElasticClientWrapper(IElasticSingletonOptions options)
    {
        _connectionString = options.ConnectionString;

        var settings = new ElasticsearchClientSettings(new SingleNodePool(new Uri(_connectionString)),
            (a, b) =>
            {

                return new DefaultSourceSerializer(
                            b,
                            opts =>
                            {
                                // e.g. remove the DoubleWithFractionalPortionConverter
                                //opts.Converters.Remove(opts.Converters.First(c => c.GetType().Name == "DoubleWithFractionalPortionConverter"));

                                opts.Converters.Add(new CustomDateTimeConverter());
                            }
                        );
            }
            );

        if (options.ServerVersion == ElasticBaseVersion.V8_X_X)
        {
            //settings.GlobalHeaders(new NameValueCollection()
            //{
            //    { "Accept", "application/vnd.elasticsearch+json;compatible-with=8" },
            //    { "Content-Type", "application/vnd.elasticsearch+json;compatible-with=8"}
            //});


        }



        _clientSetting = settings;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ElasticsearchClient Client
        => _client ??= string.IsNullOrEmpty(_connectionString)
                    ? throw new InvalidOperationException("ElasticStrings.ConnectionInfoMissing")
            : new ElasticsearchClient(_clientSetting);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Dispose()
    {
        _client = null;
    }
}
public class CustomDateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
{
    private readonly string _format;

    public CustomDateTimeConverter()
    {
        _format = "yyyy/MM/dd HH:mm:ss";
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString()!, null, DateTimeStyles.AdjustToUniversal);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime().ToString(_format));
    }
}
public class VanillaSerializer : Serializer
{
    public override object? Deserialize(Type type, Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        using var jsonReader = new JsonTextReader(reader);
        return ElasticClientWrapper.Serializer.Deserialize(jsonReader, type);
    }

    public override T Deserialize<T>(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        using var jsonReader = new JsonTextReader(reader);
        return ElasticClientWrapper.Serializer.Deserialize<T>(jsonReader);
    }

    public override async ValueTask<object?> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var json = await reader.ReadToEndAsync().WaitAsync(cancellationToken);
        return JsonConvert.DeserializeObject(json, type);
    }

    public override async ValueTask<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var json = await reader.ReadToEndAsync().WaitAsync(cancellationToken);
        return JsonConvert.DeserializeObject<T>(json)!;
    }
    public override void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
    {
        using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
        using var jsonWriter = new JsonTextWriter(writer)
        {
            Formatting = formatting == SerializationFormatting.Indented ? Formatting.Indented : Formatting.None
        };
        ElasticClientWrapper.Serializer.Serialize(jsonWriter, data);
        jsonWriter.Flush();
    }

    public override async Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None,
        CancellationToken cancellationToken = default)
    {
        using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
        var json = JsonConvert.SerializeObject(data, formatting == SerializationFormatting.Indented ? Formatting.Indented : Formatting.None);
        await writer.WriteAsync(json.AsMemory(), cancellationToken);
        await writer.FlushAsync();
    }
}
