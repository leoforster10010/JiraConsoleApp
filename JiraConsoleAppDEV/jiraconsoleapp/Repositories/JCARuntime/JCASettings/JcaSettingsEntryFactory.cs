using JiraConsoleApp.Contracts;
using Microsoft.Extensions.Logging;

namespace JiraConsoleApp.Repositories.JCARuntime.JCASettings;

public class JcaSettingsEntryFactory : IJcaSettingsEntryFactory
{
    private readonly string _filepath;
    private readonly ILogger<JcaSettingsEntryFactory> _logger;
    private readonly ISettingHandler _settingHandler;
    private readonly string _username;
    private IJcaSettingsEntry? _cachedJcaSettingsEntry;

    public JcaSettingsEntryFactory(ISettingHandler settingHandler, ILogger<JcaSettingsEntryFactory> logger,
        string? username = null, string filepath = JCADefaultSettings.DFilePath)
    {
        _filepath = filepath;
        _settingHandler = settingHandler;
        _logger = logger;
        _username = username ?? Environment.UserName;
    }

    public IJcaSettingsEntry GetJcaSettingsEntry(bool newEntry = false)
    {
        if (_cachedJcaSettingsEntry is not null && !newEntry)
        {
            return _cachedJcaSettingsEntry;
        }

        if (!File.Exists(_filepath) && !File.Exists(JCADefaultSettings.DFilePath))
        {
            _cachedJcaSettingsEntry = GetDefaultEntry();
            _cachedJcaSettingsEntry = FillUrl(_cachedJcaSettingsEntry);

            _settingHandler.SaveSettings(_cachedJcaSettingsEntry);
            return _cachedJcaSettingsEntry;
        }

        if (_settingHandler.LoadSettings<JcaSettingsEntry>(_filepath) is JcaSettingsEntry loadedSettings)
        {
            _cachedJcaSettingsEntry = new JcaSettingsEntry
            {
                Project = loadedSettings.Project,
                Username = loadedSettings.Username,
                Url = loadedSettings.Url,
                FilePath = _filepath,
                AssignCommand = loadedSettings.AssignCommand,
                ChangeProjectCommand = loadedSettings.ChangeProjectCommand,
                CheckInCommand = loadedSettings.CheckInCommand,
                CheckOutCommand = loadedSettings.CheckOutCommand,
                DeleteIssueCommand = loadedSettings.DeleteIssueCommand,
                ExitCommand = loadedSettings.ExitCommand,
                HelpCommand = loadedSettings.HelpCommand,
                ListProjectIssuesCommand = loadedSettings.ListProjectIssuesCommand,
                ListUserIssuesCommand = loadedSettings.ListUserIssuesCommand,
                LogOutCommand = loadedSettings.LogOutCommand,
                MoveIssueCommand = loadedSettings.MoveIssueCommand,
                OpenCommand = loadedSettings.OpenCommand,
                SettingsCommand = loadedSettings.SettingsCommand,
                TestCommand = loadedSettings.TestCommand,
                WorklogCommand = loadedSettings.WorklogCommand,
                DisposeParameter = loadedSettings.DisposeParameter,
                ForceParameter = loadedSettings.ForceParameter,
                NotAssignedParameter = loadedSettings.NotAssignedParameter
            };
            _cachedJcaSettingsEntry = FillUrl(_cachedJcaSettingsEntry);

            _settingHandler.SaveSettings(_cachedJcaSettingsEntry);
            return _cachedJcaSettingsEntry;
        }

        _logger.LogError("Failed to load Settings");
        throw new Exception();
    }

    private IJcaSettingsEntry GetDefaultEntry()
    {
        return new JcaSettingsEntry
        {
            Project = JCADefaultSettings.DProject,
            Username = _username,
            Url = JCADefaultSettings.DUrl,
            FilePath = JCADefaultSettings.DFilePath,
            AssignCommand = JCADefaultSettings.DAssignCommand,
            ChangeProjectCommand = JCADefaultSettings.DChangeProjectCommand,
            CheckInCommand = JCADefaultSettings.DCheckInCommand,
            CheckOutCommand = JCADefaultSettings.DCheckOutCommand,
            DeleteIssueCommand = JCADefaultSettings.DDeleteIssueCommand,
            ExitCommand = JCADefaultSettings.DExitCommand,
            HelpCommand = JCADefaultSettings.DHelpCommand,
            ListProjectIssuesCommand = JCADefaultSettings.DListProjectIssuesCommand,
            ListUserIssuesCommand = JCADefaultSettings.DListUserIssuesCommand,
            LogOutCommand = JCADefaultSettings.DLogOutCommand,
            MoveIssueCommand = JCADefaultSettings.DMoveIssueCommand,
            OpenCommand = JCADefaultSettings.DOpenCommand,
            SettingsCommand = JCADefaultSettings.DSettingsCommand,
            TestCommand = JCADefaultSettings.DTestCommand,
            WorklogCommand = JCADefaultSettings.DWorklogCommand,
            DisposeParameter = JCADefaultSettings.DDisposeParameter,
            ForceParameter = JCADefaultSettings.DForceParameter,
            NotAssignedParameter = JCADefaultSettings.DNotAssignedParameter
        };
    }

    private IJcaSettingsEntry FillUrl(IJcaSettingsEntry entry)
    {
        if (string.IsNullOrEmpty(entry.Url))
        {
            Console.WriteLine("Please enter  your Jira-Url:");
            entry.Url = Console.ReadLine() ?? string.Empty;
        }

        if (!string.IsNullOrEmpty(entry.Url))
        {
            return entry;
        }

        _logger.LogError("Invalid URL");
        throw new Exception();
    }
}