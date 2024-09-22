using ConsoleApp2;
using ConsoleAppFramework;
using Serilog;

await using var log = new LoggerConfiguration().WriteTo.Console().WriteTo.Debug().CreateLogger();
Serilog.Log.Logger = log;
var app = ConsoleApp.Create();

// app.Add("", () => log.Information("Seems like you need help"));
//app.Add("file", (string fileName) => log.Information($"Process the file {fileName}"));
app.Add<SimpleFitReader>("");
app.Run(args);