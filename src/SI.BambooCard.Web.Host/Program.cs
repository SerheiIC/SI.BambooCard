using Microsoft.AspNetCore.Mvc;
using SI.BambooCard.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options => 
    options.Filters.Add(new RequireHttpsAttribute()));
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ResolveDependencies(builder.Services);

ResolveHttpClients(builder.Services);

void ResolveHttpClients(IServiceCollection services)
{
    services.AddHttpClient("HackerNewsApi", client =>
    {
        client.BaseAddress = new Uri("https://hacker-news.firebaseio.com");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "BambooCardUserAgent");
    });
}

void ResolveDependencies(IServiceCollection services)
{
    services.AddScoped<IHackerNewsService, HackerNewsService>();
}


 
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }