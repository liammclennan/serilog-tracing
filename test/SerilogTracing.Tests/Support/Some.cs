﻿using System.Diagnostics;
using Serilog.Events;
using Serilog.Parsing;

namespace SerilogTracing.Tests.Support;

static class Some
{
    public static string String()
    {
        return $"string-{Guid.NewGuid()}";
    }

    public static int Integer()
    {
        return Interlocked.Increment(ref _integer);
    }

    public static bool Boolean()
    {
        return Integer() % 2 == 0;
    }

    static int _integer = new Random().Next(int.MaxValue / 2);
    
    public static Activity Activity(string? name = null)
    {
        return new Activity(name ?? String());
    }

    public static LogEvent SerilogEvent(string messageTemplate, DateTimeOffset? timestamp = null, Exception? ex = null)
    {
        return SerilogEvent(messageTemplate, new List<LogEventProperty>(), timestamp, ex);
    }

    public static LogEvent SerilogEvent(string messageTemplate, IEnumerable<LogEventProperty> properties, DateTimeOffset? timestamp = null, Exception? ex = null)
    {
        var ts = timestamp ?? DateTimeOffset.UtcNow;
        var parser = new MessageTemplateParser();
        var template = parser.Parse(messageTemplate);
        var logEvent = new LogEvent(
            ts,
            LogEventLevel.Warning,
            ex,
            template,
            properties);

        return logEvent;
    }
}