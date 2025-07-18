// See https://aka.ms/new-console-template for more information

using Elastic.Clients.Elasticsearch;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;

using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;


Console.WriteLine("Hello, World!");

var s = JsonSerializer.SerializeToNode(new DateTime());

var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
    //.GlobalHeaders(new NameValueCollection()
    //{
    //    { "Accept", "application/vnd.elasticsearch+json;compatible-with=8" },
    //    { "Content-Type", "application/vnd.elasticsearch+json;compatible-with=8"}
    //})
    ;


var el = new ElasticsearchClient(settings);

//var t = await el.GetAsync("p",1);
//var ts = await el.IndexAsync<Product>(new Product { Key = 34165, Name = "hahaha" }, "products", 34165);

//var res = t.Source;

var src = new ServiceCollection();

src.AddElastic<AppCtx>("http://localhost:9200", ElasticBaseVersion.V8_X_X);

var srp = src.BuildServiceProvider().CreateScope().ServiceProvider;

var ctx = srp.GetRequiredService<AppCtx>();

//ctx.Add(new Product()
//{
//    Id = 2665,
//    Name = "Test",
//    Tags = ["mineee", "asdas"],
//    Created = DateTime.UtcNow,
//    Description = "i aksdhasdl;kasj jkg afgpoas fjsa",
//    In_stock = 5,
//    Is_active = true,
//    Price = 10,
//    Sold = 1540,
//});
//var mw = ctx.SaveChanges();


//var p1 = ctx.Products.Find(2665);


//p1.Name = "my Test";


var p2 = new Product
{
    Id = 2665,
    Name = "The Test"
};

var t = ctx.Products.Attach(p2);
t.State = EntityState.Modified;


var ct = ctx.ChangeTracker.Entries();

//ctx.Products.Remove(p1);

var m = ctx.SaveChanges();

var con = ctx.Database;







int a = 0;
;
public class AppCtx : DbContext
{
    public DbSet<Product> Products { get; set; }
    public AppCtx(DbContextOptions options) : base(options)
    {

    }
    protected override async void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var prod = modelBuilder.Entity<Product>();
        //prod.HasShadowId();
        prod.ToIndexName("products");
    }

}



public class Product
{
    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? Price { get; set; }
    public int? In_stock { get; set; }
    public int? Sold { get; set; }
    public string[]? Tags { get; set; }
    public string? Description { get; set; }
    public bool? Is_active { get; set; }
    //public string? Created { get; set; }
    public DateTime? Created { get; set; }
}

