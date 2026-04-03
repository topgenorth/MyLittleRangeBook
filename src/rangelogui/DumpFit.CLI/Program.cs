using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();
builder.Configuration
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);

builder.Services.AddSerilog(lc =>
{
    lc.WriteTo.Console();

    if (builder.Environment.IsProduction())
    {
        lc.MinimumLevel.Warning();
    }
    else if (builder.Environment.IsStaging())
    {
        lc.MinimumLevel.Information();
    }
    else
    {
        lc.MinimumLevel.Verbose();
    }
});

using var host = builder.Build();
using var scope = host.Services.CreateScope();

ConsoleApp.ServiceProvider = scope.ServiceProvider;
var app = ConsoleApp.Create();

await app.RunAsync(args);

await Log.CloseAndFlushAsync();
