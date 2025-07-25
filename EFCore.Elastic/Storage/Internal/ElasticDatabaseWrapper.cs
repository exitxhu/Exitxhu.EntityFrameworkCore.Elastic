// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

///TODO

using System.Net;

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Update.Internal;

using Newtonsoft.Json.Linq;

using Database = Microsoft.EntityFrameworkCore.Storage.Database;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ElasticDatabaseWrapper : Database
{
    private readonly Dictionary<IEntityType, DocumentSource> _documentCollections = new();

    private readonly IElasticClientWrapper _elasticClient;
    private readonly bool _sensitiveLoggingEnabled;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ElasticDatabaseWrapper(
        DatabaseDependencies dependencies,
        IElasticClientWrapper cosmosClient,
        ILoggingOptions loggingOptions)
        : base(dependencies)
    {
        _elasticClient = cosmosClient;

        if (loggingOptions.IsSensitiveDataLoggingEnabled)
        {
            _sensitiveLoggingEnabled = true;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int SaveChanges(IList<IUpdateEntry> entries)
    {
        var rowsAffected = 0;
        var entriesSaved = new HashSet<IUpdateEntry>();
        var rootEntriesToSave = new HashSet<IUpdateEntry>();

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var entityType = entry.EntityType;

            Check.DebugAssert(!entityType.IsAbstract(), $"{entityType} is abstract");

            if (!entityType.IsDocumentRoot())
            {
#pragma warning disable EF1001 // Internal EF Core API usage.
                // #16707
                var root = GetRootDocument((InternalEntityEntry)entry);
#pragma warning restore EF1001 // Internal EF Core API usage.
                if (!entriesSaved.Contains(root)
                    && rootEntriesToSave.Add(root)
                    && root.EntityState == EntityState.Unchanged)
                {
#pragma warning disable EF1001 // Internal EF Core API usage.
                    // #16707
                    ((InternalEntityEntry)root).SetEntityState(EntityState.Modified);
#pragma warning restore EF1001 // Internal EF Core API usage.
                    entries.Add(root);
                }

                continue;
            }

            entriesSaved.Add(entry);

            try
            {
                if (Save(entry))
                {
                    rowsAffected++;
                }
            }
            catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
            {
                var errorEntries = new[] { entry };
                var exception = WrapUpdateException(ex, errorEntries);

                if (exception is not DbUpdateConcurrencyException
                    || !Dependencies.Logger.OptimisticConcurrencyException(
                        entry.Context, errorEntries, (DbUpdateConcurrencyException)exception, null).IsSuppressed)
                {
                    throw exception;
                }
            }
        }

        foreach (var rootEntry in rootEntriesToSave)
        {
            if (!entriesSaved.Contains(rootEntry)
                && Save(rootEntry))
            {
                rowsAffected++;
            }
        }

        return rowsAffected;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override async Task<int> SaveChangesAsync(
        IList<IUpdateEntry> entries,
        CancellationToken cancellationToken = default)
    {
        var rowsAffected = 0;
        var entriesSaved = new HashSet<IUpdateEntry>();
        var rootEntriesToSave = new HashSet<IUpdateEntry>();

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var entityType = entry.EntityType;

            Check.DebugAssert(!entityType.IsAbstract(), $"{entityType} is abstract");

            if (!entityType.IsDocumentRoot())
            {
                var root = GetRootDocument((InternalEntityEntry)entry);
                if (!entriesSaved.Contains(root)
                    && rootEntriesToSave.Add(root)
                    && root.EntityState == EntityState.Unchanged)
                {
#pragma warning disable EF1001 // Internal EF Core API usage.
                    // #16707
                    ((InternalEntityEntry)root).SetEntityState(EntityState.Modified);
#pragma warning restore EF1001 // Internal EF Core API usage.
                    entries.Add(root);
                }

                continue;
            }

            entriesSaved.Add(entry);
            try
            {
                if (await SaveAsync(entry, cancellationToken).ConfigureAwait(false))
                {
                    rowsAffected++;
                }
            }
            catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
            {
                var errorEntries = new[] { entry };
                var exception = WrapUpdateException(ex, errorEntries);

                if (exception is not DbUpdateConcurrencyException
                    || !(await Dependencies.Logger.OptimisticConcurrencyExceptionAsync(
                            entry.Context, errorEntries, (DbUpdateConcurrencyException)exception, null, cancellationToken)
                        .ConfigureAwait(false)).IsSuppressed)
                {
                    throw exception;
                }
            }
        }

        foreach (var rootEntry in rootEntriesToSave)
        {
            if (!entriesSaved.Contains(rootEntry)
                && await SaveAsync(rootEntry, cancellationToken).ConfigureAwait(false))
            {
                rowsAffected++;
            }
        }

        return rowsAffected;
    }

    private bool Save(IUpdateEntry entry)
    {
        var entityType = entry.EntityType;
        var documentSource = GetDocumentSource(entityType);
        var indexName = documentSource.GetIndexName();
        var state = entry.EntityState;

        if (entry.SharedIdentityEntry != null)
        {
            if (entry.EntityState == EntityState.Deleted)
            {
                return false;
            }

            if (state == EntityState.Added)
            {
                state = EntityState.Modified;
            }
        }

        switch (state)
        {
            case EntityState.Added:
                var newDocument = documentSource.GetCurrentDocument(entry);
                if (newDocument != null)
                {
                    documentSource.UpdateDocument(newDocument, entry);
                }
                else
                {
                    newDocument = documentSource.CreateDocument(entry);
                }

                return _elasticClient.CreateItem(indexName, newDocument, documentSource.GetId(entry), entry);

            case EntityState.Modified:
                var document = documentSource.GetCurrentDocument(entry);
                if (document != null)
                {
                    if (documentSource.UpdateDocument(document, entry) == null)
                    {
                        return false;
                    }
                }
                else
                {
                    document = documentSource.CreateDocument(entry);

                    var propertyName = entityType.FindDiscriminatorProperty()?.GetJsonPropertyName();
                    if (propertyName != null)
                    {
                        document[propertyName] =
                            JToken.FromObject(entityType.GetDiscriminatorValue()!, ElasticClientWrapper.Serializer);
                    }
                }

                return _elasticClient.ReplaceItem(
                    indexName, documentSource.GetId(entry.SharedIdentityEntry ?? entry), document, entry);

            case EntityState.Deleted:
                return _elasticClient.DeleteItem(indexName, documentSource.GetId(entry), entry);

            default:
                return false;
        }
    }

    private Task<bool> SaveAsync(IUpdateEntry entry, CancellationToken cancellationToken)
    {
        var entityType = entry.EntityType;
        var documentSource = GetDocumentSource(entityType);
        var collectionId = documentSource.GetIndexName();
        var state = entry.EntityState;

        if (entry.SharedIdentityEntry != null)
        {
            if (entry.EntityState == EntityState.Deleted)
            {
                return Task.FromResult(false);
            }

            if (state == EntityState.Added)
            {
                state = EntityState.Modified;
            }
        }

        switch (state)
        {
            case EntityState.Added:
                var primaryKey = entityType.FindPrimaryKey();
                if (primaryKey != null)
                {
                    // The code below checks for primary key properties that are not configured for value generation but have not
                    // had a non-sentinel (effectively, non-CLR default) value set. For composite keys, we only check if at least
                    // one property has value generation or a value set, since it is normal to have non-value generated parts of composite
                    // keys where one part is the CLR default. However, on Cosmos, we exclude the partition key properties from this
                    // check to ensure that, even if partition key properties have been set, at least one other primary key property is
                    // also set.
                    var partitionPropertyNeedsValue = true;
                    var propertyNeedsValue = true;
                    var allPkPropertiesAreFk = true;
                    IProperty? firstNonPartitionKeyProperty = null;

                    var partitionKeyProperties = entityType.GetPartitionKeyProperties();
                    foreach (var property in primaryKey.Properties)
                    {
                        if (property.IsForeignKey())
                        {
                            // FK properties conceptually get their value from the associated principal key, which can be handled
                            // automatically by the update pipeline in some cases, so exclude from this check.
                            continue;
                        }

                        allPkPropertiesAreFk = false;

                        var isPartitionKeyProperty = partitionKeyProperties.Contains(property);
                        if (!isPartitionKeyProperty)
                        {
                            firstNonPartitionKeyProperty = property;
                        }

                        if (property.ValueGenerated != ValueGenerated.Never
                            || entry.HasExplicitValue(property))
                        {
                            if (!isPartitionKeyProperty)
                            {
                                propertyNeedsValue = false;
                                break;
                            }

                            partitionPropertyNeedsValue = false;
                        }
                    }

                    if (!allPkPropertiesAreFk)
                    {
                        if (firstNonPartitionKeyProperty != null
                            && propertyNeedsValue)
                        {
                            // There were non-partition key properties, so only throw if it is one of these that is not set,
                            // ignoring partition key properties.
                            //Dependencies.Logger.PrimaryKeyValueNotSet(firstNonPartitionKeyProperty!);
                        }
                        else if (firstNonPartitionKeyProperty == null
                                 && partitionPropertyNeedsValue)
                        {
                            // There were no non-partition key properties in the primary key, so in this case check if any of these is not set.
                            //Dependencies.Logger.PrimaryKeyValueNotSet(primaryKey.Properties[0]);
                        }
                    }
                }

                var newDocument = documentSource.GetCurrentDocument(entry);
                if (newDocument != null)
                {
                    documentSource.UpdateDocument(newDocument, entry);
                }
                else
                {
                    newDocument = documentSource.CreateDocument(entry);
                }

                return _elasticClient.CreateItemAsync(
                    collectionId, newDocument, documentSource.GetId(entry.SharedIdentityEntry ?? entry), entry, cancellationToken);

            case EntityState.Modified:
                var document = documentSource.GetCurrentDocument(entry);
                if (document != null)
                {
                    if (documentSource.UpdateDocument(document, entry) == null)
                    {
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    document = documentSource.CreateDocument(entry);

                    var propertyName = entityType.FindDiscriminatorProperty()?.GetJsonPropertyName();
                    if (propertyName != null)
                    {
                        document[propertyName] =
                            JToken.FromObject(entityType.GetDiscriminatorValue()!, ElasticClientWrapper.Serializer);
                    }
                }

                return _elasticClient.ReplaceItemAsync(
                    collectionId,
                    documentSource.GetId(entry.SharedIdentityEntry ?? entry),
                    document,
                    entry,
                    cancellationToken);

            case EntityState.Deleted:
                return _elasticClient.DeleteItemAsync(
                    collectionId, documentSource.GetId(entry), entry, cancellationToken);

            default:
                return Task.FromResult(false);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DocumentSource GetDocumentSource(IEntityType entityType)
    {
        if (!_documentCollections.TryGetValue(entityType, out var documentSource))
        {
            _documentCollections.Add(
                entityType, documentSource = new DocumentSource(entityType, this));
        }

        return documentSource;
    }

#pragma warning disable EF1001 // Internal EF Core API usage.
    // Issue #16707
    private IUpdateEntry GetRootDocument(InternalEntityEntry entry)
    {
        var stateManager = entry.StateManager;
        var ownership = entry.EntityType.FindOwnership()!;
        var principal = stateManager.FindPrincipal(entry, ownership);
        if (principal == null)
        {
            if (_sensitiveLoggingEnabled)
            {
                throw new InvalidOperationException(
                        @"CosmosStrings.OrphanedNestedDocumentSensitive(
                        entry.EntityType.DisplayName(),
                        ownership.PrincipalEntityType.DisplayName(),
                        entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey()!.Properties))");
            }

            throw new InvalidOperationException(
                @"CosmosStrings.OrphanedNestedDocument(
                    entry.EntityType.DisplayName(),
                    ownership.PrincipalEntityType.DisplayName())");
        }

        return principal.EntityType.IsDocumentRoot() ? principal : GetRootDocument(principal);
    }
#pragma warning restore EF1001 // Internal EF Core API usage.

    private Exception WrapUpdateException(Exception exception, IReadOnlyList<IUpdateEntry> entries)
    {
        var entry = entries[0];
        var documentSource = GetDocumentSource(entry.EntityType);
        var id = documentSource.GetId(entry.SharedIdentityEntry ?? entry);

        return exception switch
        {
            _ => new DbUpdateException("CosmosStrings.UpdateStoreException(id)", exception, entries)
        };
    }
}
