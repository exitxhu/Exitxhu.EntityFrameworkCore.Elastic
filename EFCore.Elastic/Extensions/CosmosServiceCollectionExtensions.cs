// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Extensions;

/// <summary>
///     Cosmos-specific extension methods for <see cref="IServiceCollection" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the given Entity Framework <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />
    ///     and configures it to connect to an Azure Cosmos database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is a shortcut for configuring a <see cref="DbContext" /> to use Cosmos. It does not support all options.
    ///         Use <see cref="O:EntityFrameworkServiceCollectionExtensions.AddDbContext" /> and related methods for full control of
    ///         this process.
    ///     </para>
    ///     <para>
    ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
    ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
    ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
    ///         overridden to configure the Cosmos database provider.
    ///     </para>
    ///     <para>
    ///         To configure the <see cref="DbContextOptions{TContext}" /> for the context, either override the
    ///         <see cref="DbContext.OnConfiguring" /> method in your derived context, or supply
    ///         an optional action to configure the <see cref="DbContextOptions" /> for the context.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///         <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be registered.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="cosmosOptionsAction">An optional action to allow additional Cosmos-specific configuration.</param>
    /// <param name="optionsAction">An optional action to configure the <see cref="DbContextOptions" /> for the context.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddElastic<TContext>(
        this IServiceCollection serviceCollection,
        string connectionString,
        ElasticBaseVersion serverVersion = ElasticBaseVersion.V9_X_X,
        Action<ElasticDbContextOptionsBuilder> cosmosOptionsAction = null,
        Action<DbContextOptionsBuilder> optionsAction = null)
        where TContext : DbContext
        => serviceCollection.AddDbContext<TContext>(
            (serviceProvider, options) =>
            {
                optionsAction?.Invoke(options);
                options.UseElastic(connectionString, serverVersion, cosmosOptionsAction);
            });

    /// <summary>
    ///     <para>
    ///         Adds the services required by the Azure Cosmos database provider for Entity Framework
    ///         to an <see cref="IServiceCollection" />.
    ///     </para>
    ///     <para>
    ///         Warning: Do not call this method accidentally. It is much more likely you need
    ///         to call <see cref="AddElastic{TContext}" />.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     Calling this method is no longer necessary when building most applications, including those that
    ///     use dependency injection in ASP.NET or elsewhere.
    ///     It is only needed when building the internal service provider for use with
    ///     the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
    ///     This is not recommend other than for some advanced scenarios.
    /// </remarks>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>
    ///     The same service collection so that multiple calls can be chained.
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IServiceCollection AddEntityFrameworkElastic(this IServiceCollection serviceCollection)
    {
        var builder = new EntityFrameworkServicesBuilder(serviceCollection)
            .TryAdd<LoggingDefinitions, ElasticLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<ElasticOptionsExtension>>()
            .TryAdd<IDatabase, ElasticDatabaseWrapper>()
            .TryAdd<IExecutionStrategyFactory, ElasticExecutionStrategyFactory>()
            .TryAdd<IDbContextTransactionManager, ElasticTransactionManager>()
            .TryAdd<IModelValidator, ElasticModelValidator>()
            .TryAdd<IModelRuntimeInitializer, ElasticModelRuntimeInitializer>()
            .TryAdd<IProviderConventionSetBuilder, ElasticConventionSetBuilder>()
            .TryAdd<IValueGeneratorSelector, ElasticValueGeneratorSelector>()
            .TryAdd<IDatabaseCreator, ElasticDatabaseCreator>()
            .TryAdd<IQueryContextFactory, ElasticQueryContextFactory>()
            .TryAdd<ITypeMappingSource, ElasticTypeMappingSource>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, ElasticQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, ElasticShapedQueryCompilingExpressionVisitorFactory>()
            .TryAdd<ISingletonOptions, IElasticSingletonOptions>(p => p.GetRequiredService<IElasticSingletonOptions>())
            .TryAdd<IQueryTranslationPreprocessorFactory, ElasticQueryTranslationPreprocessorFactory>()
            .TryAdd<IQueryCompilationContextFactory, ElasticQueryCompilationContextFactory>()
            .TryAdd<IQueryTranslationPostprocessorFactory, ElasticQueryTranslationPostprocessorFactory>()
            .TryAddProviderSpecificServices(
                b => b
                    .TryAddSingleton<IElasticSingletonOptions, ElasticSingletonOptions>()
                    .TryAddSingleton<ISingletonElasticClientWrapper, SingletonElasticClientWrapper>()
                    .TryAddSingleton<IQuerySqlGeneratorFactory, QuerySqlGeneratorFactory>()
                    .TryAddSingleton<ElasticModelRuntimeInitializerDependencies, ElasticModelRuntimeInitializerDependencies>()
                    .TryAddSingleton<IJsonIdDefinitionFactory, JsonIdDefinitionFactory>()
                    .TryAddScoped<ISqlExpressionFactory, SqlExpressionFactory>()
                    .TryAddScoped<IMemberTranslatorProvider, CosmosMemberTranslatorProvider>()
                    .TryAddScoped<IMethodCallTranslatorProvider, CosmosMethodCallTranslatorProvider>()
                    .TryAddScoped<IElasticClientWrapper, ElasticClientWrapper>());

        builder.TryAddCoreServices();

        return serviceCollection;
    }
}
