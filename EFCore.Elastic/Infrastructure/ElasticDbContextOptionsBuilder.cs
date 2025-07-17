// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Net;

using Microsoft.EntityFrameworkCore.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;

/// <summary>
///     Allows Cosmos specific configuration to be performed on <see cref="DbContextOptions" />.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from a call to
///         <see cref="O:CosmosDbContextOptionsExtensions.UseElastic{TContext}" />
///         and it is not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
///         <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
///     </para>
/// </remarks>
public class ElasticDbContextOptionsBuilder : ICosmosDbContextOptionsBuilderInfrastructure
{
    private readonly DbContextOptionsBuilder _optionsBuilder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElasticDbContextOptionsBuilder" /> class.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The options builder.</param>
    public ElasticDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        => _optionsBuilder = optionsBuilder;

    /// <inheritdoc />
    DbContextOptionsBuilder ICosmosDbContextOptionsBuilderInfrastructure.OptionsBuilder
        => _optionsBuilder;

    protected virtual ElasticDbContextOptionsBuilder WithOption(Func<ElasticOptionsExtension, ElasticOptionsExtension> setAction)
    {
        ((IDbContextOptionsBuilderInfrastructure)_optionsBuilder).AddOrUpdateExtension(
            setAction(_optionsBuilder.Options.FindExtension<ElasticOptionsExtension>() ?? new ElasticOptionsExtension()));

        return this;
    }

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
