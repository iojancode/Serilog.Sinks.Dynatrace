# Serilog.Sinks.Dynatrace #

Serilog Sink that sends log events to Dynatrace https://www.dynatrace.com/

**Package** - [Serilog.Sinks.Dynatrace](http://nuget.org/packages/serilog.sinks.dynatrace) | **Platforms** - .NET 4.5, netstandard2.0

Example:
```csharp
var log = new LoggerConfiguration()
    .WriteTo.Dynatrace(accessToken: "xxx.yyyyyy.zzzzz", ingestUrl: "https://{your-environment-id}.live.dynatrace.com/api/v2/logs/ingest")
    .CreateLogger();

var position = new { Latitude = 25, Longitude = 134 };
var elapsedMs = 34;
log.Information("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);
```

Prints to Dynatrace console:
```
Oct 10 16:09:13 desktop-r9hnrih myapp Information Processed { Latitude: 25, Longitude: 134 } in 034 ms.
```

[![Nuget](https://img.shields.io/nuget/v/serilog.sinks.dynatrace.svg)](https://www.nuget.org/packages/Serilog.Sinks.Dynatrace/)
