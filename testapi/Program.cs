using Exitxhu.EntityFrameworkCore.Elastic;

using Microsoft.EntityFrameworkCore;

using Nest;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;


var s = JsonSerializer.SerializeToNode(new DateTime());
 MethodInfo? GetItemMethodInfo =
    typeof(JsonObject).GetProperty("Item", new[] { typeof(string) })?.GetGetMethod();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppCtx>(a => a.UseElastic(a => a.Url = "http://localhost:9200"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


public class AppCtx : ElasticDbContextBase
{
    public AppCtx(DbContextOptions options) : base(options)
    {

    }

}