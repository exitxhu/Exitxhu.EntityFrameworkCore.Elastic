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
        IndexResponse res = null;
        if (newDocument is JObject j)
        {
            var t = j.ToObject(entry.EntityType.ClrType);
            res = Client.Index(t, indexName, new Id(id));

        }
        else
            res = Client.Index(newDocument, indexName, new Id(id));
        return res.IsSuccess();
    }

    public async Task<bool> CreateItemAsync(string indexName, object newDocument, string id, IUpdateEntry entry, CancellationToken cancellationToken)
    {
        var res = await Client.IndexAsync(newDocument, indexName, id, cancellationToken);
        return res.IsSuccess();
    }

    public bool DeleteItem(string indexName, string id, IUpdateEntry entry)
    {

        var res = Client.Delete(new DeleteRequest(indexName, id));

        return res.IsSuccess();
    }

    public async Task<bool> DeleteItemAsync(string indexName, string id, IUpdateEntry entry, CancellationToken cancellationToken)
    {
        var res = await Client.DeleteAsync(new DeleteRequest(indexName, id));

        return res.IsSuccess();
    }

    public JObject ExecuteReadItem(string indexName, string id)
    {
        var res = Client.Get(indexName, new Id(id));
        return JObject.FromObject(res.Source);
    }
    public JObject ExecuteReadItem<T>(string indexName, string id)
    {
        var res = Client.Get<T>(indexName, new Id(id));
        var j = JObject.FromObject(res.Source);
        return j;
    }

    public async Task<JObject> ExecuteReadItemAsync(string indexName, string id, CancellationToken cancellationToken)
    {
        var res = await Client.GetAsync(indexName, new Id(id), cancellationToken);
        return JObject.FromObject(res.Source);
    }
    public async Task<JObject> ExecuteReadItemAsync<T>(string indexName, string id, CancellationToken cancellationToken)
    {
        var res = await Client.GetAsync<T>(indexName, new Id(id), cancellationToken);
        return JObject.FromObject(res.Source);
    }


    public IEnumerable<JToken> ExecuteSqlQuery(string indexName, CosmosSqlQuery sqlQuery)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<JToken> ExecuteSqlQueryAsync(string indexName, CosmosSqlQuery sqlQuery)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<JToken> GetResponseMessageEnumerable(object responseMessage)
    {
        throw new NotImplementedException();
    }

    public bool ReplaceItem(string indexName, string id, JObject document, IUpdateEntry entry)
    {
        return CreateItem(indexName, document, id, entry);
    }

    public async Task<bool> ReplaceItemAsync(string indexName, string id, JObject document, IUpdateEntry entry, CancellationToken cancellationToken)
    {
        return await CreateItemAsync(indexName, document, id, entry, cancellationToken);
    }


}
