using JiraConsoleApp;
using JiraConsoleApp.Contracts;
using JiraConsoleApp.Logging;
using JiraConsoleApp.Repositories;
using JiraConsoleApp.Repositories.AppFramework;
using JiraConsoleApp.Repositories.AppFramework.Settings;
using JiraConsoleApp.Repositories.JCARuntime;
using JiraConsoleApp.Repositories.JCARuntime.JCASettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services
            .AddSingleton<IFileHandler, FileHandler>()
            .AddSingleton<ISettingHandler, SettingHandler>()
            .AddSingleton<IJiraFactory, JiraFactory>()
            .AddSingleton<IJcaSettingsEntryFactory, JcaSettingsEntryFactory>()
            .AddSingleton<IJcaCore, JcaCore>();

        services.AddHostedService<Kernel>();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddTextFileLogger(configuration => { configuration.Path = "JiraConsoleAppLog.txt"; });
    })
    .ConfigureHostConfiguration(_ => { })
    .ConfigureAppConfiguration(_ => { })
    .Build();

host.Run();