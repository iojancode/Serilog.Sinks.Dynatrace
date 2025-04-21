# Serilog.Sinks.Dynatrace #

Serilog Sink that sends log events to Dynatrace <https://www.dynatrace.com/>

**Package** - [Serilog.Sinks.Dynatrace](http://nuget.org/packages/serilog.sinks.dynatrace) | **Platforms** - netstandard2.0, .NET Framework 4.6.1+

## Getting started ##

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

```plaintext
2025-04-20 16:41...    INFO    Processed { Latitude: 25, Longitude: 134 } in 034 ms.

application.id = unknown
elapsed = 34
position.latitude = 25
position.longitude = 134
host.name = desktop-r9hnrih
```

## Log from ASP.NET Core & appsettings.json ##

Extra packages:

```shell
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Settings.Configuration
```

Add `UseSerilog` to the Generic Host:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog((context, logConfig) => logConfig.ReadFrom.Configuration(context.Configuration))
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

[![Nuget](https://img.shields.io/nuget/v/serilog.sinks.dynatrace.svg)](https://www.nuget.org/packages/Serilog.Sinks.Dynatrace/)
