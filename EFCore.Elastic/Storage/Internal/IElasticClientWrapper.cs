// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Elastic.Clients.Elasticsearch;

using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public interface IElasticClientWrapper
{
    public ElasticsearchClient Client { get; }

    bool CreateItem(string indexName, object newDocument, string id, IUpdateEntry entry);
    Task<bool> CreateItemAsync(string indexName, object newDocument, string id, IUpdateEntry entry, CancellationToken cancellationToken);
    bool DeleteItem(string collectionId, string v, IUpdateEntry entry);
    Task<bool> DeleteItemAsync(string collectionId, string v, IUpdateEntry entry, CancellationToken cancellationToken);
    JObject ExecuteReadItem(string cosmosContainer, string resourceId);
    Task<JObject> ExecuteReadItemAsync(string cosmosContainer, string resourceId, CancellationToken cancellationToken);
    IEnumerable<JToken> ExecuteSqlQuery(string cosmosContainer, CosmosSqlQuery sqlQuery);
    IAsyncEnumerable<JToken> ExecuteSqlQueryAsync(string cosmosContainer, CosmosSqlQuery sqlQuery);
    IEnumerable<JToken> GetResponseMessageEnumerable(object responseMessage);
    bool ReplaceItem(string collectionId, string v, JObject document, IUpdateEntry entry);
    Task<bool> ReplaceItemAsync(string collectionId, string v, JObject document, IUpdateEntry entry, CancellationToken cancellationToken);
    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //bool CreateDatabaseIfNotExists();

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //Task<bool> CreateDatabaseIfNotExistsAsync( CancellationToken cancellationToken = default);

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //bool DeleteDatabase();

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //Task<bool> DeleteDatabaseAsync(CancellationToken cancellationToken = default);

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //bool CreateItem(string containerId, JToken document, IUpdateEntry entry);

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //bool ReplaceItem(
    //    string collectionId,
    //    string documentId,
    //    JObject document,
    //    IUpdateEntry entry);

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //bool DeleteItem(
    //    string containerId,
    //    string documentId,
    //    IUpdateEntry entry);

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //Task<bool> CreateItemAsync(
    //    string containerId,
    //    JToken document,
    //    IUpdateEntry updateEntry,
    //    CancellationToken cancellationToken = default);

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //Task<bool> ReplaceItemAsync(
    //    string collectionId,
    //    string documentId,
    //    JObject document,
    //    IUpdateEntry updateEntry,
    //    CancellationToken cancellationToken = default);

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //Task<bool> DeleteItemAsync(
    //    string containerId,
    //    string documentId,
    //    IUpdateEntry entry,
    //    CancellationToken cancellationToken = default);


    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //JObject? ExecuteReadItem(
    //    string containerId,
    //    PartitionKey partitionKeyValue,
    //    string resourceId);

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //Task<JObject?> ExecuteReadItemAsync(
    //    string containerId,
    //    PartitionKey partitionKeyValue,
    //    string resourceId,
    //    CancellationToken cancellationToken = default);

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //IEnumerable<JToken> ExecuteSqlQuery(
    //    string containerId,
    //    PartitionKey partitionKeyValue,
    //    CosmosSqlQuery query);

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //IAsyncEnumerable<JToken> ExecuteSqlQueryAsync(
    //    string containerId,
    //    PartitionKey partitionKeyValue,
    //    CosmosSqlQuery query);

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //IEnumerable<JToken> GetResponseMessageEnumerable(ResponseMessage responseMessage);
}
