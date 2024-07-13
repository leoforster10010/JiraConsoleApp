using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace JiraConsoleApp.Logging;

public static class TextFileLoggerExtensions
{
    public static ILoggingBuilder AddTextFileLogger(
        this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, TextFileLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions
            <TextFileLoggerConfiguration, TextFileLoggerConfiguration>(builder.Services);

        return builder;
    }

    public static ILoggingBuilder AddTextFileLogger(
        this ILoggingBuilder builder,
        Action<TextFileLoggerConfiguration> configure)
    {
        builder.AddTextFileLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}