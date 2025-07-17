using Elastic.Clients.Elasticsearch;

using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

public class ElasticClientWrapper : IElasticClientWrapper
{


    private readonly ISingletonElasticClientWrapper _singletonWrapper;
    private readonly string _databaseId;
    private readonly IExecutionStrategy _executionStrategy;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Database> _databaseLogger;
    private readonly bool? _enableContentResponseOnWrite;


    public static readonly JsonSerializer Serializer;
    static ElasticClientWrapper()
    {
        Serializer = JsonSerializer.Create();
        Serializer.Converters.Add(new ByteArrayConverter());
        Serializer.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        Serializer.DateParseHandling = DateParseHandling.None;
    }

    public ElasticClientWrapper(
        ISingletonElasticClientWrapper singletonWrapper,
        IDbContextOptions dbContextOptions,
        IExecutionStrategy executionStrategy,
        IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger,
        IDiagnosticsLogger<DbLoggerCategory.Database> databaseLogger)
    {
        var options = dbContextOptions.FindExtension<ElasticOptionsExtension>();

        _singletonWrapper = singletonWrapper;
        _executionStrategy = executionStrategy;
        _commandLogger = commandLogger;
        _databaseLogger = databaseLogger;
    }

    public ElasticsearchClient Client => _singletonWrapper.Client;

    public bool CreateItem(string indexName, object newDocument, string id, IUpdateEntry entry)
    {
        var res = Client.Index(newDocument, indexName, new Id(id));
        return res.IsSuccess();
    }

    public async Task<bool> CreateItemAsync(string indexName, object newDocument, string id, IUpdateEntry entry, CancellationToken cancellationToken)
    {
        var res = await Client.IndexAsync(newDocument, indexName, id, cancellationToken);
        return res.IsSuccess();
    }

    public bool DeleteItem(string indexName, string id, IUpdateEntry entry)
    {
        var res = Client.Delete(indexName, new Id(id));
        return res.IsSuccess();
    }

    public Task<bool> DeleteItemAsync(string indexName, string id, IUpdateEntry entry, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public JObject ExecuteReadItem(string cosmosContainer, string resourceId)
    {
        throw new NotImplementedException();
    }

    public Task<JObject> ExecuteReadItemAsync(string cosmosContainer, string resourceId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<JToken> ExecuteSqlQuery(string cosmosContainer, CosmosSqlQuery sqlQuery)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<JToken> ExecuteSqlQueryAsync(string cosmosContainer, CosmosSqlQuery sqlQuery)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<JToken> GetResponseMessageEnumerable(object responseMessage)
    {
        throw new NotImplementedException();
    }

    public bool ReplaceItem(string indexName, string v, JObject document, IUpdateEntry entry)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ReplaceItemAsync(string indexName, string v, JObject document, IUpdateEntry entry, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
