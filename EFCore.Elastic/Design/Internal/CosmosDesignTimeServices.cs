// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Extensions;
using Microsoft.EntityFrameworkCore.Design.Internal;

[assembly: DesignTimeProviderServices("Microsoft.EntityFrameworkCore.Cosmos.Design.Internal.CosmosDesignTimeServices")]

namespace Microsoft.EntityFrameworkCore.Cosmos.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosDesignTimeServices : IDesignTimeServices
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddEntityFrameworkElastic();
#pragma warning disable EF1001 // Internal EF Core API usage.
        new EntityFrameworkDesignServicesBuilder(serviceCollection)
            .TryAdd<ICSharpRuntimeAnnotationCodeGenerator, CosmosCSharpRuntimeAnnotationCodeGenerator>()
#pragma warning restore EF1001 // Internal EF Core API usage.
            .TryAddCoreServices();
    }
}
