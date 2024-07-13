using Microsoft.Extensions.Logging;

namespace JiraConsoleApp.Logging;

public class TextFileLogger : ILogger
{
    private readonly Func<TextFileLoggerConfiguration> _getCurrentConfig;

    public TextFileLogger(
        string name,
        Func<TextFileLoggerConfiguration> getCurrentConfig)
    {
        _getCurrentConfig = getCurrentConfig;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var config = _getCurrentConfig();
        using var file = new StreamWriter(config.Path, true);
        file.WriteLine("|" + DateTime.Now + ": " + logLevel + "| " + formatter(state, exception) +
                       Environment.NewLine);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return default!;
    }
}