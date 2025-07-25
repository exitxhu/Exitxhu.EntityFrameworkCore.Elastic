// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class CosmosEntityTypeExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsDocumentRoot(this IReadOnlyEntityType entityType)
        => entityType.BaseType?.IsDocumentRoot()
            ?? (entityType.FindOwnership() == null
                || entityType[CosmosAnnotationNames.IndexName] != null);

    /// <summary>
    ///     Returns the JSON `id` definition, or <see langword="null" /> if there is none.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    public static IJsonIdDefinition? GetJsonIdDefinition(this IEntityType entityType)
        => entityType.GetOrAddRuntimeAnnotationValue(
            CosmosAnnotationNames.JsonIdDefinition,
            static e =>
                ((ElasticModelRuntimeInitializerDependencies)e!.Model.FindRuntimeAnnotationValue(
                    CosmosAnnotationNames.ModelDependencies)!).JsonIdDefinitionFactory.Create(e),
            entityType);
}
