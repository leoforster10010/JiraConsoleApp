using Atlassian.Jira;
using JiraConsoleApp.Contracts;
using JiraConsoleApp.Repositories.JCARuntime;
using Microsoft.Extensions.Logging;

namespace JiraConsoleApp.Repositories;

public class JiraFactory : IJiraFactory
{
    private readonly IJcaSettingsEntryFactory _entryFactory;
    private readonly ILogger<JcaCore> _logger;
    private Jira? _cachedConnection;

    public JiraFactory(ILogger<JcaCore> logger, IJcaSettingsEntryFactory entryFactory)
    {
        _logger = logger;
        _entryFactory = entryFactory;
    }

    public async Task<Jira> GetJiraConnection(bool newConnection = false)
    {
        var settingsEntry = _entryFactory.GetJcaSettingsEntry();

        if (_cachedConnection is not null && !newConnection)
        {
            return _cachedConnection;
        }

        login:
        Console.WriteLine("Enter your Jira-Password:");
        string? password = null;
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }

            password += key.KeyChar;
        }

        try
        {
            var jira = Jira.CreateRestClient(settingsEntry.Url, Environment.UserName,
                password);

            //throws exception if unauthorized(wrong password)
            await jira.ServerInfo.GetServerInfoAsync();
            Console.WriteLine("Login successful");
            _cachedConnection = jira;
            return jira;
        }
        catch (Exception e)
        {
            _logger.LogError($"Authentication failed: {e}");
            goto login;
        }
    }
}