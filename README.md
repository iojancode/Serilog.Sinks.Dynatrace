# Serilog.Sinks.Dynatrace #

Serilog Sink that sends log events to Dynatrace https://www.dynatrace.com/

**Package** - [Serilog.Sinks.Dynatrace](http://nuget.org/packages/serilog.sinks.dynatrace) | **Platforms** - .NET 4.5, netstandard2.0

## Getting started

Enable the sink and log:
```csharp
var log = new LoggerConfiguration()
    .WriteTo.Dynatrace(
        accessToken: "xxx.yyyyyy.zzzzz",
        ingestUrl: "https://{your-environment-id}.live.dynatrace.com/api/v2/logs/ingest")
    .CreateLogger();

var position = new { Latitude = 25, Longitude = 134 };
var elapsedMs = 34;
log.Information("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);
```

Prints to Dynatrace Log Viewer:
```
2021-12-30 16:41...    INFO    Processed { Latitude: 25, Longitude: 134 } in 034 ms.

application.id = unknown
attr.elapsed = 34
attr.position.latitude = 25
attr.position.longitude = 134
host.name = desktop-r9hnrih
```

## Log from ASP.NET Core & appsettings.json

Extra packages:
```shell
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Settings.Configuration
```

Add `UseSerilog` to the Generic Host:
```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog((context, services, logConfig) => 
            logConfig.ReadFrom.Configuration(context.Configuration))
        .ConfigureWebHostDefaults(webBuilder => {
            webBuilder.UseStartup<Startup>();
        });
```

Add to `appsettings.json` configuration:
```json
{
    "Serilog": {
        "Using": [ "Serilog.Sinks.Dynatrace" ],
        "MinimumLevel": "Information",
        "WriteTo": [{
            "Name": "Dynatrace",
            "Args": {
                "accessToken": "xxx.yyyyyy.zzzzz",
                "ingestUrl": "https://{your-environment-id}.live.dynatrace.com/api/v2/logs/ingest"
            }
        }]
    }
}
```

Inject and log:
```csharp
public class HomeController : Controller
{
    private readonly ILogger _logger;

    public HomeController(ILogger<HomeController> logger) { _logger = logger; }

    public IActionResult Index()
    {
        _logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", new { Latitude = 25, Longitude = 134 }, 34);
        return View();
    }
}
```

[![Nuget](https://img.shields.io/nuget/v/serilog.sinks.dynatrace.svg)](https://www.nuget.org/packages/Serilog.Sinks.Dynatrace/)
