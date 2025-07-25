// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Extensions;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ElasticConventionSetBuilder : ProviderConventionSetBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ElasticConventionSetBuilder(
        ProviderConventionSetBuilderDependencies dependencies,
        IJsonIdDefinitionFactory definitionFactory)
        : base(dependencies)
        => DefinitionFactory = definitionFactory;

    /// <summary>
    ///     The factory to create a <see cref="IJsonIdDefinition" /> for each entity type.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    protected virtual IJsonIdDefinitionFactory DefinitionFactory { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();

        conventionSet.Add(new ContextContainerConvention(Dependencies));
        conventionSet.Add(new ETagPropertyConvention());
        conventionSet.Add(new CosmosPartitionKeyInPrimaryKeyConvention(Dependencies));
        conventionSet.Add(new CosmosJsonIdConvention(Dependencies, DefinitionFactory));
        conventionSet.Remove(typeof(ForeignKeyIndexConvention));

        conventionSet.Replace<ValueGenerationConvention>(new CosmosValueGenerationConvention(Dependencies));
        conventionSet.Replace<KeyDiscoveryConvention>(new CosmosKeyDiscoveryConvention(Dependencies));
        conventionSet.Replace<InversePropertyAttributeConvention>(new CosmosInversePropertyAttributeConvention(Dependencies));
        conventionSet.Replace<RelationshipDiscoveryConvention>(new CosmosRelationshipDiscoveryConvention(Dependencies));
        //conventionSet.Replace<DiscriminatorConvention>(new CosmosDiscriminatorConvention(Dependencies));
        conventionSet.Replace<ManyToManyJoinEntityTypeConvention>(new CosmosManyToManyJoinEntityTypeConvention(Dependencies));
        conventionSet.Replace<RuntimeModelConvention>(new CosmosRuntimeModelConvention(Dependencies));

        return conventionSet;
    }

    /// <summary>
    ///     Call this method to build a <see cref="ModelBuilder" /> for Cosmos outside of <see cref="DbContext.OnModelCreating" />.
    /// </summary>
    /// <remarks>
    ///     Note that it is unusual to use this method. Consider using <see cref="DbContext" /> in the normal way instead.
    /// </remarks>
    /// <returns>The convention set.</returns>
    public static ModelBuilder CreateModelBuilder()
    {
        using var serviceScope = CreateServiceScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
        return new ModelBuilder(ConventionSet.CreateConventionSet(context), context.GetService<ModelDependencies>());
    }

    private static IServiceScope CreateServiceScope()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkElastic()
            .AddDbContext<DbContext>(
                (p, o) =>
                    o.UseElastic("localhost")
                        .UseInternalServiceProvider(p))
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
    }
}
