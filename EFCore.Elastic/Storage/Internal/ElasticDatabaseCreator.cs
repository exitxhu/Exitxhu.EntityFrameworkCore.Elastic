// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ElasticDatabaseCreator : IDatabaseCreator
{
    private readonly IElasticClientWrapper _cosmosClient;
    private readonly IDesignTimeModel _designTimeModel;
    private readonly IUpdateAdapterFactory _updateAdapterFactory;
    private readonly IDatabase _database;
    private readonly ICurrentDbContext _currentContext;
    private readonly IDbContextOptions _contextOptions;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ElasticDatabaseCreator(
        IElasticClientWrapper cosmosClient,
        IDesignTimeModel designTimeModel,
        IUpdateAdapterFactory updateAdapterFactory,
        IDatabase database,
        ICurrentDbContext currentContext,
        IDbContextOptions contextOptions)
    {
        _cosmosClient = cosmosClient;
        _designTimeModel = designTimeModel;
        _updateAdapterFactory = updateAdapterFactory;
        _database = database;
        _currentContext = currentContext;
        _contextOptions = contextOptions;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool EnsureCreated()
    {
        //var model = _designTimeModel.Model;
        //var created = _cosmosClient.CreateDatabaseIfNotExists(model.GetThroughput());

        //foreach (var container in GetContainersToCreate(model))
        //{
        //    created |= _cosmosClient.CreateContainerIfNotExists(container);
        //}

        //if (created)
        //{
        //    InsertData();
        //}

        //SeedData(created);

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        //var model = _designTimeModel.Model;
        //var created = await _cosmosClient.CreateDatabaseIfNotExistsAsync(model.GetThroughput(), cancellationToken)
        //    .ConfigureAwait(false);

        //foreach (var container in GetContainersToCreate(model))
        //{
        //    created |= await _cosmosClient.CreateContainerIfNotExistsAsync(container, cancellationToken)
        //        .ConfigureAwait(false);
        //}

        //if (created)
        //{
        //    await InsertDataAsync(cancellationToken).ConfigureAwait(false);
        //}

        //await SeedDataAsync(created, cancellationToken).ConfigureAwait(false);

        //return created;
        return true;
    }


    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void InsertData()
    {
        var updateAdapter = AddModelData();

        _database.SaveChanges(updateAdapter.GetEntriesToSave());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task InsertDataAsync(CancellationToken cancellationToken = default)
    {
        var updateAdapter = AddModelData();

        return _database.SaveChangesAsync(updateAdapter.GetEntriesToSave(), cancellationToken);
    }

    private IUpdateAdapter AddModelData()
    {
        var updateAdapter = _updateAdapterFactory.CreateStandalone();
        foreach (var entityType in _designTimeModel.Model.GetEntityTypes())
        {
            foreach (var targetSeed in entityType.GetSeedData())
            {
                var runtimeEntityType = updateAdapter.Model.FindEntityType(entityType.Name)!;
                var entry = updateAdapter.CreateEntry(targetSeed, runtimeEntityType);
                entry.EntityState = EntityState.Added;
            }
        }

        return updateAdapter;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SeedData(bool created)
    {
        var coreOptionsExtension =
            _contextOptions.FindExtension<CoreOptionsExtension>()
            ?? new CoreOptionsExtension();

        var seed = coreOptionsExtension.Seeder;
        if (seed != null)
        {
            seed(_currentContext.Context, created);
        }
        else if (coreOptionsExtension.AsyncSeeder != null)
        {
            throw new InvalidOperationException(CoreStrings.MissingSeeder);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task SeedDataAsync(bool created, CancellationToken cancellationToken = default)
    {
        var coreOptionsExtension =
            _contextOptions.FindExtension<CoreOptionsExtension>()
            ?? new CoreOptionsExtension();

        var seedAsync = coreOptionsExtension.AsyncSeeder;
        if (seedAsync != null)
        {
            await seedAsync(_currentContext.Context, created, cancellationToken).ConfigureAwait(false);
        }
        else if (coreOptionsExtension.Seeder != null)
        {
            throw new InvalidOperationException(CoreStrings.MissingSeeder);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool EnsureDeleted()
        //=> _cosmosClient.DeleteDatabase();
        => true;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
        //=> _cosmosClient.DeleteDatabaseAsync(cancellationToken);
        =>Task.FromResult(true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanConnect()
        => throw new NotSupportedException("CosmosStrings.CanConnectNotSupported");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("CosmosStrings.CanConnectNotSupported");

}
