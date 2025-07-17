// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using Microsoft.EntityFrameworkCore.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Extensions;

/// <summary>
///     Cosmos-specific extension methods for <see cref="DbContextOptionsBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosDbContextOptionsExtensions
{


    /// <summary>
    ///     Configures the context to connect to an Azure Cosmos database. The connection details need to be specified in a separate call.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="cosmosOptionsAction">An action to allow Cosmos-specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseElastic(
        this DbContextOptionsBuilder optionsBuilder,
        Action<ElasticDbContextOptionsBuilder> cosmosOptionsAction)
    {
        Check.NotNull(optionsBuilder, nameof(optionsBuilder));
        Check.NotNull(cosmosOptionsAction, nameof(cosmosOptionsAction));

        ConfigureWarnings(optionsBuilder);

        cosmosOptionsAction.Invoke(new ElasticDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }


    /// <summary>
    ///     Configures the context to connect to an Azure Cosmos database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="elasticOptionsAction">An optional action to allow additional Cosmos-specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseElastic(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        ElasticBaseVersion serverVersion = ElasticBaseVersion.V9_X_X,
        Action<ElasticDbContextOptionsBuilder> elasticOptionsAction = null)
    {
        Check.NotNull(optionsBuilder, nameof(optionsBuilder));
        Check.NotNull(connectionString, nameof(connectionString));

        var extension = optionsBuilder.Options.FindExtension<ElasticOptionsExtension>()
            ?? new ElasticOptionsExtension();

        extension = extension
            .WithConnectionString(connectionString)
            .WithServerVersion(serverVersion);

        ConfigureWarnings(optionsBuilder);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        elasticOptionsAction?.Invoke(new ElasticDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }

    private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
    {
        var coreOptionsExtension
            = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
            ?? new CoreOptionsExtension();

        coreOptionsExtension = coreOptionsExtension.WithWarningsConfiguration(
            coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                CosmosEventId.SyncNotSupported, WarningBehavior.Throw));

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
    }
}
