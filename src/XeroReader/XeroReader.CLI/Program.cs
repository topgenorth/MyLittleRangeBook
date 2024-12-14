using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using net.opgenorth.mylittlerangebook.data.sqlite;
using net.opgenorth.xero;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true)
    .AddJsonFile($"appsettings.{Environments.Development}.json", true);
builder.AddGarminShotViewDatabase();
builder.Services.AddSerilog(lc =>
    lc .ReadFrom.Configuration(builder.Configuration)
);

using IHost host = builder.Build();
using IServiceScope scope = host.Services.CreateScope();

var sp = scope.ServiceProvider;
var o = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<GarminShotViewSqliteOptions>>();

ILogger log = scope.ServiceProvider.GetRequiredService<ILogger>();
ConsoleApp.ServiceProvider = scope.ServiceProvider;
ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

app.Add<ReadShotViewXLSX>();
app.Add<SqliteMigration>();
log.Verbose("Start app");
app.Run(args);
