using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JiraConsoleApp.Logging;

[UnsupportedOSPlatform("browser")]
[ProviderAlias("TextFileLogger")]
public class TextFileLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, TextFileLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly IDisposable _onChangeToken;
    private TextFileLoggerConfiguration _currentConfig;

    public TextFileLoggerProvider(
        IOptionsMonitor<TextFileLoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken.Dispose();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new TextFileLogger(name, GetCurrentConfig));
    }

    private TextFileLoggerConfiguration GetCurrentConfig()
    {
        return _currentConfig;
    }
}