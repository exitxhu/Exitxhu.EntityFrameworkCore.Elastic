// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ElasticModelValidator : ModelValidator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ElasticModelValidator(ModelValidatorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.Validate(model, logger);

        ValidateDatabaseProperties(model, logger);
        ValidateKeys(model, logger);
        ValidateOnlyETagConcurrencyToken(model, logger);
        ValidateIndexes(model, logger);
        ValidateDiscriminatorMappings(model, logger);
        ValidateCollectionElementTypes(model, logger);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void ValidateCollectionElementTypes(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            ValidateType(entityType, logger);
        }

        static void ValidateType(ITypeBase typeBase, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var property in typeBase.GetDeclaredProperties())
            {
                var typeMapping = property.GetElementType()?.GetTypeMapping();
                while (typeMapping != null)
                {
                    if (typeMapping.Converter != null)
                    {
                        throw new InvalidOperationException(
                            @"CosmosStrings.ElementWithValueConverter(
                                property.ClrType.ShortDisplayName(),
                                typeBase.ShortName(),
                                property.Name,
                                typeMapping.ClrType.ShortDisplayName())");
                    }

                    typeMapping = typeMapping.ElementTypeMapping;
                }
            }

            foreach (var complexProperty in typeBase.GetDeclaredComplexProperties())
            {
                ValidateType(complexProperty.ComplexType, logger);
            }
        }
    }



    protected virtual void ValidateOnlyETagConcurrencyToken(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                if (property.IsConcurrencyToken)
                {
                    var storeName = property.GetJsonPropertyName();
                    if (storeName != "_etag")
                    {
                        throw new InvalidOperationException("CosmosStrings.NonETagConcurrencyToken(entityType.DisplayName(), storeName)");
                    }

                    var etagType = property.GetTypeMapping().Converter?.ProviderClrType ?? property.ClrType;
                    if (etagType != typeof(string))
                    {
                        throw new InvalidOperationException(
                            "CosmosStrings.ETagNonStringStoreType(property.Name, entityType.DisplayName(), etagType.ShortDisplayName())");
                    }
                }
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void ValidateKeys(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey == null
                || !entityType.IsDocumentRoot())
            {
                continue;
            }

            var idProperty = entityType.GetProperties()
                .FirstOrDefault(p => p.GetJsonPropertyName() == CosmosJsonIdConvention.IdPropertyJsonName);
            if (idProperty == null)
            {
                throw new InvalidOperationException("CosmosStrings.NoIdProperty(entityType.DisplayName())");
            }

            var idType = idProperty.GetTypeMapping().Converter?.ProviderClrType
                ?? idProperty.ClrType;
            if (idType != typeof(string))
            {
                throw new InvalidOperationException(
                    "CosmosStrings.IdNonStringStoreType(idProperty.Name, entityType.DisplayName(), idType.ShortDisplayName())");
            }

            var partitionKeyPropertyNames = entityType.GetPartitionKeyPropertyNames();
            if (partitionKeyPropertyNames.Count == 0)
            {
                //logger.NoPartitionKeyDefined(entityType);
            }
            else
            {
                if (entityType.BaseType != null
                    && entityType.FindAnnotation(CosmosAnnotationNames.PartitionKeyNames)?.Value != null)
                {
                    throw new InvalidOperationException(
                        "CosmosStrings.PartitionKeyNotOnRoot(entityType.DisplayName(), entityType.BaseType.DisplayName())");
                }

                foreach (var partitionKeyPropertyName in partitionKeyPropertyNames)
                {
                    var partitionKey = entityType.FindProperty(partitionKeyPropertyName);
                    if (partitionKey == null)
                    {
                        throw new InvalidOperationException(
                            "CosmosStrings.PartitionKeyMissingProperty(entityType.DisplayName(), partitionKeyPropertyName)");
                    }

                    var partitionKeyType = (partitionKey.GetTypeMapping().Converter?.ProviderClrType
                        ?? partitionKey.ClrType).UnwrapNullableType();
                    if (partitionKeyType != typeof(string)
                        && !partitionKeyType.IsNumeric()
                        && partitionKeyType != typeof(bool))
                    {
                        throw new InvalidOperationException(
                            @"CosmosStrings.PartitionKeyBadStoreType(
                                partitionKeyPropertyName,
                                entityType.DisplayName(),
                                partitionKeyType.ShortDisplayName())");
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of mutable in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected override void ValidateNoMutableKeys(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var key in entityType.GetDeclaredKeys())
            {
                var mutableProperty = key.Properties.FirstOrDefault(p => p.ValueGenerated.HasFlag(ValueGenerated.OnUpdate));
                if (mutableProperty != null
                    && !mutableProperty.IsOrdinalKeyProperty())
                {
                    throw new InvalidOperationException(CoreStrings.MutableKeyProperty(mutableProperty.Name));
                }
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void ValidateDatabaseProperties(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            var properties = new Dictionary<string, IPropertyBase>();
            foreach (var property in entityType.GetProperties())
            {
                var jsonName = property.GetJsonPropertyName();
                if (string.IsNullOrWhiteSpace(jsonName))
                {
                    continue;
                }

                if (properties.TryGetValue(jsonName, out var otherProperty))
                {
                    throw new InvalidOperationException(
                        "CosmosStrings.JsonPropertyCollision(property.Name, otherProperty.Name, entityType.DisplayName(), jsonName)");
                }

                properties[jsonName] = property;
            }

            foreach (var navigation in entityType.GetNavigations())
            {
                if (!navigation.IsEmbedded())
                {
                    continue;
                }

                var jsonName = navigation.TargetEntityType.GetContainingPropertyName()!;
                if (properties.TryGetValue(jsonName, out var otherProperty))
                {
                    throw new InvalidOperationException(
                        "CosmosStrings.JsonPropertyCollision(navigation.Name, otherProperty.Name, entityType.DisplayName(), jsonName)");
                }

                properties[jsonName] = navigation;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void ValidateDiscriminatorMappings(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            if (!entityType.IsDocumentRoot()
                && entityType.FindAnnotation(CosmosAnnotationNames.DiscriminatorInKey) != null)
            {
                throw new InvalidOperationException("CosmosStrings.DiscriminatorInKeyOnNonRoot(entityType.DisplayName())");
            }

            if (!entityType.IsDocumentRoot()
                && entityType.FindAnnotation(CosmosAnnotationNames.HasShadowId) != null)
            {
                throw new InvalidOperationException("CosmosStrings.HasShadowIdOnNonRoot(entityType.DisplayName())");
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void ValidateIndexes(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ValidatePropertyMapping(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidatePropertyMapping(model, logger);
    }
}
