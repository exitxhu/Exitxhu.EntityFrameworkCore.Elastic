
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

public static class CosmosAnnotationNames
{
    public const string IndexName = Prefix + "IndexName";


    public const string Prefix = "Elastic:";

    public const string PropertyName = Prefix + "PropertyName";

    public const string PartitionKeyNames = Prefix + "PartitionKeyNames";

    public const string ETagName = Prefix + "ETagName";

    public const string AnalyticalStoreTimeToLive = Prefix + "AnalyticalStoreTimeToLive";

    public const string DefaultTimeToLive = Prefix + "DefaultTimeToLive";

    public const string Throughput = Prefix + "Throughput";

    public const string JsonIdDefinition = Prefix + "JsonIdDefinition";

    public const string DiscriminatorInKey = Prefix + "DiscriminatorInKey";

    public const string HasShadowId = Prefix + nameof(HasShadowId);

    public const string ModelDependencies = Prefix + "ModelDependencies";
}
