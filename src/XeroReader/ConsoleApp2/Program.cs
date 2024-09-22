﻿using ConsoleApp2;
using ConsoleAppFramework;
using Dynastream.Fit;
using Microsoft.Extensions.DependencyInjection;

await using var log = new LoggerConfiguration().WriteTo.Console().WriteTo.Debug().CreateLogger();
Serilog.Log.Logger = log;

log.Verbose("Boostrapping app.");
var services = new ServiceCollection();
services.AddSingleton<ILogger>(log);

using var serviceProvider = services.BuildServiceProvider(); // using for logger flush(important!)
ConsoleApp.ServiceProvider = serviceProvider;

var app = ConsoleApp.Create();

app.Add<SimpleFitReader>("");

log.Verbose("Start app");
app.Run(args);