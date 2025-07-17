using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

using Nest;

using System.Collections;
using System.Linq.Expressions;

namespace Exitxhu.EntityFrameworkCore.Elastic;
public static class ElasticDbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseElastic(
        this DbContextOptionsBuilder optionsBuilder,
        Action<ElasticOptions> configureOptions)
    {
        var options = new ElasticOptions();
        configureOptions(options);
        var extension = new ElasticDbContextOptionsExtension(options);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}

public class ElasticOptions
{
    public string Url { get; set; }
    public string IndexPrefix { get; set; } = "";
}

public class ElasticDbContextOptionsExtension : IDbContextOptionsExtension
{
    private ElasticOptions _options;

    public ElasticDbContextOptionsExtension(ElasticOptions options)
    {
        _options = options;
        Info = new ExtensionInfo(this);
    }

    public ElasticOptions Options => _options;
    public DbContextOptionsExtensionInfo Info { get; }

    public void ApplyServices(IServiceCollection services)
    {
        services.AddSingleton(_options);
        services.AddSingleton<IElasticClient>(provider =>
        {
            var settings = new ConnectionSettings(new Uri(_options.Url));
            return new ElasticClient(settings);
        });
        //services.AddScoped<IElasticQueryExecutor, ElasticQueryExecutor>();
        //services.AddScoped<IElasticDataStore, ElasticDataStore>();
    }

    public void Validate(IDbContextOptions options) { }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension) { }

        public override string LogFragment => "using ElasticSearch";

        public override bool IsDatabaseProvider => true;

        public override int GetServiceProviderHashCode() => 0;
        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;
    }
}

public interface IElasticQueryExecutor
{
    Task<List<T>> ExecuteAsync<T>(IQueryable<T> query);
}

public interface IElasticDataStore
{
    Task<T> FindAsync<T>(object key);
    Task IndexAsync<T>(T entity);
    Task DeleteAsync<T>(object key);
}

public class ElasticQueryable<T> : IQueryable<T>
{
    public ElasticQueryable(IElasticQueryProvider provider)
    {
        Provider = provider;
        Expression = Expression.Constant(this);
    }

    public ElasticQueryable(IElasticQueryProvider provider, Expression expression)
    {
        Provider = provider;
        Expression = expression;
    }

    public Type ElementType => typeof(T);
    public Expression Expression { get; }
    public IQueryProvider Provider { get; }

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

public interface IElasticQueryProvider : IAsyncQueryProvider
{
    IQueryable<T> CreateQuery<T>(Expression expression);
    Task<List<T>> ExecuteAsync<T>(Expression expression, CancellationToken cancellationToken) where T : class;
}
public class ElasticQueryProvider : IElasticQueryProvider
{
    private readonly IElasticClient _client;

    public ElasticQueryProvider(IElasticClient client)
    {
        _client = client;
    }

    public IQueryable<T> CreateQuery<T>(Expression expression)
    {
        return new ElasticQueryable<T>(this, expression);
    }

    public IQueryable CreateQuery(Expression expression)
    {
        throw new NotImplementedException();
    }

    public object Execute(Expression expression)
    {
        throw new NotSupportedException("Sync execution not supported.");
    }

    public TResult Execute<TResult>(Expression expression)
    {
        throw new NotSupportedException("Sync execution not supported.");
    }

    public async Task<List<T>> ExecuteAsync<T>(Expression expression, CancellationToken cancellationToken) where T : class
    {
        var visitor = new ElasticExpressionVisitor();
        var searchRequest = visitor.Translate<T>(expression);
        var response = await _client.SearchAsync<T>(searchRequest, cancellationToken);
        return response.Documents.ToList();
    }

    TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
public class ElasticExpressionVisitor : ExpressionVisitor
{
    public SearchRequest<T> Translate<T>(Expression expression)
    {
        // Walk the expression tree
        // Extract filters, selects, order by, etc.
        // Convert to `SearchRequest<T>`

        var request = new SearchRequest<T>
        {
            Query = new MatchAllQuery()
        };

        // TODO: Handle `Where`, `OrderBy`, etc.

        return request;
    }
    
}
public class ElasticDbContext : DbContext
{
    public DbSet<Book> Books { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseElastic(opt =>
        {
            opt.Url = "http://localhost:9200";
        });
}

public class Book
{
    public string Id { get; set; }
    public string Title { get; set; }
}


public abstract class ElasticDbContextBase : DbContext
{
    protected ElasticDbContextBase(DbContextOptions options) : base(options)
    {
        //_client = client;
    }
    private readonly IElasticClient _client;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure your entities here
        base.OnModelCreating(modelBuilder);
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                await _client.IndexDocumentAsync(entry.Entity);
            }
            else if (entry.State == EntityState.Deleted)
            {
                var id = entry.Property("Id").CurrentValue;
                await _client.DeleteAsync(new DeleteRequest(entry.Entity.GetType(), id.ToString()));
            }
        }

        return 0;
    }
}