// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Elastic.Clients.Elasticsearch;

using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;

using System.Collections.Specialized;

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

        var settings = new ElasticsearchClientSettings(new Uri(_connectionString));

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
