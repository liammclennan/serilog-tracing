# SerilogTracing

An **experimental** Serilog extension for producing and capturing hierarchical traces.

## What is _SerilogTracing_?

_SerilogTracing_ integrates Serilog with the `System.Diagnostics.Activity*` types provided by the .NET BCL. This makes
it possible to:

 1. Record traces generated by .NET system libraries and packages through any Serilog sink, 
 2. Generate rich traces using Serilog APIs and idioms, that can still be observed by other consumers of the .NET APIs,
 3. Introduce tracing gradually into applications already instrumented with Serilog.

Wt development time, routing trace information through Serilog means that all of the beautiful, developer-friendly
Serilog outputs can be used for simple local feedback.

Here's what that looks like, routed through Serilog's console sink:

![A screenshot of Windows Terminal showing output from the included Example application.](https://raw.githubusercontent.com/nblumhardt/serilog-tracing/dev/assets/console-output.png)

The example is using Serilog's `ExpressionTemplate` to annotate each span in the trace with timing information. The
layout is fully configurable - check out `Program.cs` in the included example project - so let your ASCII artistry run
wild!

In production, Serilog's existing configuration, enrichment, filtering, formatting, and output facilities
can potentially provide a flexible mechanism for emitting trace data to a wide range of targets.

Notably, any system that accepts traces in text or JSON format should be an easy target for _SerilogTracing_ and a
Serilog sink; here's Seq showing traces delivered using the production _Serilog.Sinks.Seq_ package and a custom
`ITextFormatter` implemented with `ExpressionTemplate`:

![A screenshot of Seq showing output from the included Example application.](https://raw.githubusercontent.com/nblumhardt/serilog-tracing/dev/assets/seq-output.png)

## How does it work?

And what do we mean by "tracing"?

A _trace_ is just a collection of _spans_, and a span is just an event that carries a:

 * trace id,
 * span id,
 * parent span id (optional), and
 * start time.

_SerilogTracing_ generates spans using extension methods on `ILogger`:

```csharp
using var activity = logger.StartActivity("Compute {A} + {B}", a, b);
// ... on `Dispose()` the activity will be recorded as a span
```

The spans generated by _SerilogTracing_ are converted into Serilog `LogEvent`s and routed through the logger. There's
nothing particularly special about these events, except that they add the `ParentSpanId` and `SpanStartTimestamp`
properties.

_SerilogTracing_ relies on the underlying .NET tracing infrastructure to propagate trace ids and record the parent-child
relationships between spans.

The `TracingConfiguration` class from SerilogTracing is used for this:

```csharp
Log.Logger = new LoggerConfiguration()
    ... // Other configuration as usual, then:
    .CreateLogger();

// Tracing will be enabled until the returned handle is disposed.
using var _ = new TracingConfiguration()
    .EnableTracing();
```

In addition to generating spans, _SerilogTracing_ consumes spans generated elsewhere in an application via the
`System.Diagnostics.Activity` APIs, such as those produced by ASP.NET Core or `HttpClient`. Activity sources can be
enabled or disabled using the logger's `MinimumLevel.Override` settings.

Finally, _SerilogTracing_ includes some examples showing how the resulting `LogEvent`s can be formatted for various
trace-aware outputs.

## Starting, enriching, and completing activities

Activities are represented by `LoggerActivity` instances.

`LoggerActivity` has a simple lifetime:

 * The activity is started using one of the `ILogger.StartActivity()` extensions,
 * Properties are added to the activity using `LoggerActivity.AddProperty()`, and
 * The activity is completed either implicitly, by `IDisposable.Dispose()`, or explicitly using `LoggerActivity.Complete()`.

`LoggerActivity.Complete()` accepts optional `LogEventLevel` and `Exception` arguments.

## Displaying output

Use the formatters provides by `Serilog.Tracing.Formatting.DefaultFormatting` to pretty-print spans as text, or 
serialize to JSON.

## Configuring the activity listener

Activity sources can be enabled and disabled using the standard `MinimumLevel.Override()` mechanism.

## SerilogTracing.Sinks.OpenTelemetry

SerilogTracing includes a fork of [Serilog.Sinks.OpenTelemetry](https://github.com/serilog/serilog-sinks-opentelemetry). This is necessary (for now) because Serilog.Sinks.OpenTelemetry only handles log signals. SerilogTrace.Sinks.OpenTelemetry also handles trace signals. 

## What about SerilogTimings?

SerilogTracing is the logical successor to [SerilogTimings](https://github.com/nblumhardt/serilog-timings), which provides very
similar functionality.

Like SerilogTimings, SerilogTracing can track the duration and status of an operation, and emit an event when
the operation completes.

Unlike SerilogTimings, SerilogTracing:

 * can represent and capture hierarchical (parent/child) operations,
 * takes part in distributed traces, and
 * integrates with the tracing support provided by .NET itself and the rest of the package ecosystem.

## Status

This project is experimental. It's not a part of Serilog, not maintained by the Serilog maintainers, and might not
evolve in any particular way: there's currently no plan to integrate this functionality directly into Serilog. (Having
said that, this project _is_ a vehicle to explore those possibilities).
