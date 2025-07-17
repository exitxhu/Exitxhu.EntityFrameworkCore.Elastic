// See https://aka.ms/new-console-template for more information

using Elastic.Clients.Elasticsearch;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;

using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;


Console.WriteLine("Hello, World!");

var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
    //.GlobalHeaders(new NameValueCollection()
    //{
    //    { "Accept", "application/vnd.elasticsearch+json;compatible-with=8" },
    //    { "Content-Type", "application/vnd.elasticsearch+json;compatible-with=8"}
    //})
    ;


var el = new ElasticsearchClient(settings);

var t = await el.GetAsync<Product>("products", new Id(1));
var ts = await el.IndexAsync<Product>(new Product { Key = 34165, Name = "hahaha" }, "products", 34165);

var res = t.Source;

var src = new ServiceCollection();

src.AddElastic<AppCtx>("http://localhost:9200", ElasticBaseVersion.V8_X_X);

var srp = src.BuildServiceProvider().CreateScope().ServiceProvider;

var ctx = srp.GetRequiredService<AppCtx>();
ctx.Add(new Product()
{
    Key = 2665,
    Name = "Test",
    Tags = ["mineee"]
});

var m = ctx.SaveChanges();

var con = ctx.Database;

;
public class AppCtx : DbContext
{
    public DbSet<Product> Products { get; set; }
    public AppCtx(DbContextOptions options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>()
            .ToIndexName("products");
    }

}



public class Product
{
    [Key]
    public int Key { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    public int In_stock { get; set; }
    public int Sold { get; set; }
    public string[] Tags { get; set; }
    public string Description { get; set; }
    public bool Is_active { get; set; }
    public string Created { get; set; }
}

