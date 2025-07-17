// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Elastic.Clients.Elasticsearch.Ingest;

using Microsoft.EntityFrameworkCore.Cosmos.Extensions;

using System.Globalization;
using System.Net;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ElasticOptionsExtension : IDbContextOptionsExtension
{
    private string? _connectionString;
    private ElasticBaseVersion _serverVersion = ElasticBaseVersion.V9_X_X;
    private DbContextOptionsExtensionInfo _info;
    private Func<ExecutionStrategyDependencies, IExecutionStrategy>? _executionStrategyFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ElasticOptionsExtension()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected ElasticOptionsExtension(ElasticOptionsExtension copyFrom)
    {
        _connectionString = copyFrom._connectionString;
        _serverVersion = copyFrom._serverVersion;
        _executionStrategyFactory = copyFrom._executionStrategyFactory;

    }

    public virtual string? ConnectionString
        => _connectionString;
    public virtual Func<ExecutionStrategyDependencies, IExecutionStrategy>? ExecutionStrategyFactory
    => _executionStrategyFactory;
    public virtual ElasticOptionsExtension WithConnectionString(string? connectionString)
    {
        var clone = Clone();

        clone._connectionString = connectionString;
        return clone;
    }
    public virtual ElasticBaseVersion ServerVersion
    => _serverVersion;

    public virtual DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);
    public virtual ElasticOptionsExtension WithServerVersion(ElasticBaseVersion serverVersion)
    {
        var clone = Clone();

        clone._serverVersion = serverVersion;
        return clone;
    }
    protected virtual ElasticOptionsExtension Clone()
        => new(this);

    public virtual void ApplyServices(IServiceCollection services)
        => services.AddEntityFrameworkElastic();


    public virtual void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        private string? _logFragment;
        private int? _serviceProviderHash;

        private new ElasticOptionsExtension Extension
            => (ElasticOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider
            => true;

        public override int GetServiceProviderHashCode()
        {
            if (_serviceProviderHash == null)
            {
                var hashCode = new HashCode();

                hashCode.Add(Extension._connectionString);

                hashCode.Add(Extension._serverVersion);

                _serviceProviderHash = hashCode.ToHashCode();
            }

            return _serviceProviderHash.Value;
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo
                && Extension._connectionString == otherInfo.Extension._connectionString
                && Extension._serverVersion == otherInfo.Extension._serverVersion;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            if (!string.IsNullOrEmpty(Extension._connectionString))
            {
                debugInfo["Elastic:" + nameof(ConnectionString)] =
                    Extension._connectionString.GetHashCode().ToString(CultureInfo.InvariantCulture);
            }

        }

        public override string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();

                    builder.Append("ServiceEndPoint=").Append(Extension._connectionString).Append(' ');

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }
    }
}
