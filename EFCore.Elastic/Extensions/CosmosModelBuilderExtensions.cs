// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

global using Microsoft.EntityFrameworkCore.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Extensions;

/// <summary>
///     Cosmos-specific extension methods for <see cref="ModelBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosModelBuilderExtensions
{
    /// <summary>
    ///     Configures the default container name that will be used if no name
    ///     is explicitly configured for an entity type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The default container name.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasDefaultContainer(
        this ModelBuilder modelBuilder,
        string name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        modelBuilder.Model.SetDefaultIndexName(name);

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the default container name that will be used if no name
    ///     is explicitly configured for an entity type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The default container name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder HasDefaultContainer(
        this IConventionModelBuilder modelBuilder,
        string name,
        bool fromDataAnnotation = false)
    {
        if (!modelBuilder.CanSetDefaultIndexName(name, fromDataAnnotation))
        {
            return null;
        }

        modelBuilder.Metadata.SetDefaultIndexName(name, fromDataAnnotation);

        return modelBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given container name can be set as default.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The default container name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given container name can be set as default.</returns>
    public static bool CanSetDefaultIndexName(
        this IConventionModelBuilder modelBuilder,
        string name,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(name, nameof(name));

        return modelBuilder.CanSetAnnotation(CosmosAnnotationNames.IndexName, name, fromDataAnnotation);
    }


    /// <summary>
    ///     Forces model building to always create a "__id" shadow property mapped to the JSON "id". This was the default
    ///     behavior before EF Core 9.0.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="alwaysCreate">
    ///     <see langword="true" /> to force __id creation, <see langword="false" /> to not force __id creation,
    ///     <see langword="null" /> to revert to the default setting.
    /// </param>
    public static ModelBuilder HasShadowIds(this ModelBuilder modelBuilder, bool? alwaysCreate = true)
    {
        modelBuilder.Model.SetHasShadowIds(alwaysCreate);

        return modelBuilder;
    }

    /// <summary>
    ///     Includes the discriminator value of the entity type in the JSON "id" value. This was the default behavior before EF Core 9.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="includeDiscriminator">
    ///     <see langword="true" /> to include the discriminator, <see langword="false" /> to not include the discriminator,
    ///     <see langword="null" /> to revert to the default setting.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasDiscriminatorInJsonIds(
        this ModelBuilder modelBuilder,
        bool? includeDiscriminator = true)
    {
        modelBuilder.Model.SetDiscriminatorInKey(
            includeDiscriminator == null
                ? null
                : includeDiscriminator.Value
                    ? IdDiscriminatorMode.EntityType
                    : IdDiscriminatorMode.None);

        return modelBuilder;
    }

    /// <summary>
    ///     Includes the discriminator value of the root entity type in the JSON "id" value. This allows types with the same
    ///     primary key to be saved in the same container, while still allowing "ReadItem" to be used for lookups of an unknown type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="includeDiscriminator">
    ///     <see langword="true" /> to include the discriminator, <see langword="false" /> to not include the discriminator,
    ///     <see langword="null" /> to revert to the default setting.
    /// </param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasRootDiscriminatorInJsonId(
        this ModelBuilder modelBuilder,
        bool? includeDiscriminator = true)
    {
        modelBuilder.Model.SetDiscriminatorInKey(
            includeDiscriminator == null
                ? null
                : includeDiscriminator.Value
                    ? IdDiscriminatorMode.RootEntityType
                    : IdDiscriminatorMode.None);

        return modelBuilder;
    }


}


