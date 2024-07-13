using System.Diagnostics;
using System.Reflection;
using Atlassian.Jira;
using JiraConsoleApp.Contracts;
using Microsoft.Extensions.Logging;

namespace JiraConsoleApp.Repositories.JCARuntime;

public class JcaCore : IJcaCore
{
    private readonly IJcaSettingsEntryFactory _entryFactory;
    private readonly IJiraFactory _jiraFactory;
    private readonly ILogger<JcaCore> _logger;
    private readonly ISettingHandler _settingHandler;

    private readonly Stopwatch _timer = new();
    private Issue? _checkedIssue;

    private Jira _jira;
    private List<Issue>? _listedIssues;
    private IJcaSettingsEntry _settings;


    public JcaCore(IJiraFactory jiraFactory, ILogger<JcaCore> logger,
        ISettingHandler settingHandler, IJcaSettingsEntryFactory entryFactory)
    {
        _jiraFactory = jiraFactory;
        _logger = logger;
        _settingHandler = settingHandler;
        _entryFactory = entryFactory;
    }

    public async Task OnStartUp(CancellationToken cancellationToken)
    {
        _jira = await _jiraFactory.GetJiraConnection();
        _settings = _entryFactory.GetJcaSettingsEntry();

        try
        {
            await ListUserIssuesCommand(new[] {string.Empty});
            if (string.IsNullOrEmpty(_settings.Project))
            {
                _settings.Project = (await _jira.Projects.GetProjectsAsync()).First().Key;
            }

            await IdleForInput(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError($"StartUp failed: {e}");
            await IdleForInput(cancellationToken);
        }
    }

    internal async Task IdleForInput(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Waiting for Input:");
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                await ProcessInput(input);
            }
        }
    }

    private async Task ProcessInput(string? input)
    {
        if (input == null)
        {
            return;
        }

        var inputSplit = input.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
        var command = inputSplit[0].ToLower();
        var commandParameters = inputSplit.Skip(1).DefaultIfEmpty().ToArray();

        var commandProperties = _settings.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.Name.EndsWith("Command"));

        foreach (var property in commandProperties)
        {
            var valueType = property.GetValue(_settings)?.GetType();

            if (valueType is not {IsArray: true} ||
                !typeof(string).IsAssignableFrom(valueType.GetElementType()))
            {
                continue;
            }

            var valueArray = (string[]?) property.GetValue(_settings);

            if (valueArray == null || !valueArray.Contains(command))
            {
                continue;
            }

            var commandMethod = GetType()
                .GetMethod(property.Name, BindingFlags.NonPublic | BindingFlags.Instance);

            if (commandMethod != null)
            {
                try
                {
                    Console.Clear();
                    await (commandMethod.Invoke(this, new object?[] {commandParameters}) as Task);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to call {commandMethod.Name}: {e}");
                }

                return;
            }

            _logger.LogWarning($"Function matching command {property.Name} is not implemented");
            return;
        }

        _logger.LogWarning($"Command {command} is not supported");
        var settingText = _settingHandler.SettingEntryToText(_settings, "Command");
        Console.WriteLine("Available commands are:");
        Console.WriteLine("");
        Console.WriteLine(string.Join(Environment.NewLine, settingText));
    }

    private async Task Command(string[] commandParameters)
    {
    }

    private async Task DeleteIssueCommand(string[] commandParameters)
    {
        if (!commandParameters.Any() || string.IsNullOrEmpty(commandParameters[0]))
        {
            _logger.LogWarning("Missing arguments for DeleteIssueCommand");
            return;
        }

        var issue = await GetIssue(commandParameters[0]);
        Console.WriteLine(
            $"You are about to delete Issue {issue.Key}. Please confirm. [y/n]");
        switch (Console.ReadLine()?.ToLower())
        {
            case "y":
                await _jira.Issues.DeleteIssueAsync(await FormatIssueKey(commandParameters[0]));
                break;
            case "n":
                Console.WriteLine("Deletion aborted.");
                return;
            default:
                Console.WriteLine("Invalid Input, Deletion aborted.");
                return;
        }

        Console.WriteLine($"Deleted Issue {issue.Key}.");
    }

    private async Task ExitCommand(string[] commandParameters)
    {
        var additionalCommand = commandParameters.ElementAtOrDefault(1);
        var forceExit = additionalCommand != null && _settings.ForceParameter.Contains(additionalCommand);
        string[] checkOutParameters = {""};

        if (_checkedIssue != null)
        {
            if (!forceExit)
            {
                Console.WriteLine(
                    $"You are still checked-in into issue {_checkedIssue.Key}. Do you want to check-out[0] or dispose the worklog[1]?");
                switch (Console.ReadLine()?.ToLower())
                {
                    case "0":
                        break;
                    case "1":
                        checkOutParameters[0] = _settings.DisposeParameter[0];
                        break;
                    default:
                        Console.WriteLine("Invalid Input, Exit aborted.");
                        return;
                }
            }

            await CheckOutCommand(checkOutParameters);
        }

        Environment.Exit(0);
    }

    private async Task TestCommand(string[] commandParameters)
    {
    }

    private async Task SettingsCommand(string[] commandParameters)
    {
        var propertyName = commandParameters.ElementAtOrDefault(0);
        if (commandParameters.Any() && !string.IsNullOrEmpty(propertyName))
        {
            var newValues = commandParameters.Skip(1).ToArray();
            _settingHandler.ChangeSetting(_settings, propertyName, newValues);
            _settingHandler.SaveSettings(_settings);
        }
        else
        {
            Process.Start("notepad", _settings.FilePath);
        }
    }

    private async Task LogOutCommand(string[] commandParameters)
    {
        var additionalCommand = commandParameters.ElementAtOrDefault(1);
        var forceExit = additionalCommand != null && _settings.ForceParameter.Contains(additionalCommand);
        string[] checkOutParameters = {""};

        if (_checkedIssue != null)
        {
            if (!forceExit)
            {
                Console.WriteLine(
                    $"You are still checked-in into issue {_checkedIssue.Key}. Do you want to check-out[0] or dispose the worklog[1]?");
                switch (Console.ReadLine()?.ToLower())
                {
                    case "0":
                        break;
                    case "1":
                        checkOutParameters[0] = _settings.DisposeParameter[0];
                        break;
                    default:
                        Console.WriteLine("Invalid Input, LogOut aborted.");
                        return;
                }
            }

            await CheckOutCommand(checkOutParameters);
        }

        _jira = await _jiraFactory.GetJiraConnection(true);
    }

    private async Task AssignCommand(string[] commandParameters)
    {
        if (!commandParameters.Any() || string.IsNullOrEmpty(commandParameters[0]))
        {
            _logger.LogWarning("Missing arguments for AssignCommand");
            return;
        }

        var issue = await GetIssue(commandParameters[0]);
        var assignee = commandParameters.ElementAtOrDefault(1);

        if (_settings.NotAssignedParameter.Contains(assignee))
        {
            assignee = null;
        }
        else
        {
            var formatUsername = await FormatUsername(assignee);
            Console.WriteLine(
                $"You are about to assign {issue.Key} to {formatUsername}. Please confirm.[y/n]");
            switch (Console.ReadLine()?.ToLower())
            {
                case "y":
                    break;
                case "n":
                    Console.WriteLine("Assignment aborted.");
                    return;
                default:
                    Console.WriteLine("Invalid input, assignment aborted.");
                    return;
            }

            assignee = formatUsername;
        }

        if (issue.Assignee != assignee)
        {
            await issue.AssignAsync(assignee);
            issue.SaveChanges();
            Console.WriteLine(assignee != null
                ? $"{issue.Key} assigned to {assignee}."
                : $"Unassigned issue {issue.Key}");
        }
        else
        {
            Console.WriteLine($"Current assignee of {issue.Key} is already {issue.Assignee}");
        }
    }

    private async Task CheckInCommand(string[] commandParameters)
    {
        if (!commandParameters.Any() || string.IsNullOrEmpty(commandParameters[0]))
        {
            _logger.LogWarning("Missing arguments for CheckInCommand");
            return;
        }

        var additionalCommand = commandParameters.ElementAtOrDefault(1);
        var forceCheckIn = additionalCommand != null &&
                           _settings.ForceParameter.Contains(additionalCommand);
        var issue = await GetIssue(commandParameters[0]);

        if (issue.Assignee != _settings.Username)
        {
            if (!forceCheckIn)
            {
                Console.WriteLine(
                    $"You are not assigned to this issue. Current assignee is: {issue.Assignee}. Do you want to assign {issue.Key} to you? [y/n]");
                switch (Console.ReadLine()?.ToLower())
                {
                    case "y":
                        break;
                    case "n":
                        Console.WriteLine("Check-In aborted.");
                        return;
                    default:
                        Console.WriteLine("Invalid Input, CheckIn aborted.");
                        return;
                }
            }

            await issue.AssignAsync(_settings.Username);
            issue.SaveChanges();
            Console.WriteLine($"{issue.Key} assigned to {_settings.Username}.");
        }

        if (_checkedIssue != null)
        {
            if (!forceCheckIn)
            {
                Console.WriteLine(
                    $"You are already checked-in into {_checkedIssue.Key}. Do you want to check-out and check-in into {issue.Key}? [y/n]");
                switch (Console.ReadLine()?.ToLower())
                {
                    case "y":
                        break;
                    case "n":
                        Console.WriteLine("Check-In aborted.");
                        return;
                    default:
                        Console.WriteLine("Invalid Input, CheckIn aborted.");
                        return;
                }
            }

            await CheckOutCommand(new[] {""});
        }

        _checkedIssue = issue;
        _timer.Start();
        Console.WriteLine($"Checked-In into issue {issue.Key}.");
    }

    private async Task CheckOutCommand(string[] commandParameters)
    {
        if (_checkedIssue == null)
        {
            Console.WriteLine("You are not checked-in into any issues.");
            return;
        }

        var additionalCommand = commandParameters.ElementAtOrDefault(0);
        var disposeCheckIn = additionalCommand != null &&
                             _settings.DisposeParameter.Contains(additionalCommand);
        if (!disposeCheckIn && additionalCommand != null)
        {
            await _checkedIssue.AddWorklogAsync(additionalCommand);
            Console.WriteLine($"Added manual worklog of {additionalCommand} to issue {_checkedIssue.Key}.");
        }
        else if (!disposeCheckIn && additionalCommand == null)
        {
            var minutes = _timer.Elapsed.TotalMinutes;
            var timeString = $"{minutes:####}m";
            await _checkedIssue.AddWorklogAsync(timeString);
            Console.WriteLine($"Added worklog of {timeString} to issue {_checkedIssue.Key}.");
        }
        else
        {
            Console.WriteLine($"Worklog of {_checkedIssue.Key} discarded.");
        }

        _timer.Reset();
        _checkedIssue = null;
    }

    private async Task WorklogCommand(string[] commandParameters)
    {
        if (!commandParameters.Any() || string.IsNullOrEmpty(commandParameters.ElementAtOrDefault(1)))
        {
            _logger.LogWarning("Missing arguments for WorklogCommand");
            return;
        }

        var issue = await GetIssue(commandParameters[0]);

        await issue.AddWorklogAsync(commandParameters[1]);
        Console.WriteLine($"Added worklog of {commandParameters[1]} to issue {issue.Key}.");
    }

    private async Task MoveIssueCommand(string[] commandParameters)
    {
        if (!commandParameters.Any() || string.IsNullOrEmpty(commandParameters[0]))
        {
            _logger.LogWarning("Missing arguments for AssignCommand");
            return;
        }

        var additionalCommand = commandParameters.ElementAtOrDefault(1)?.ToLower();
        var issue = await GetIssue(commandParameters[0]);

        var actions = await issue.GetAvailableActionsAsync();
        if (additionalCommand is null or "" || !actions.Any(x =>
                x.Id.ToLower().Contains(additionalCommand) || x.Name.ToLower().Contains(additionalCommand)))
        {
            Console.WriteLine("Please state the Action-Id or Action-Name you want the issue to be moved to.");
            Console.WriteLine($"Available actions for {issue.Key} are:");
            foreach (var action in actions)
            {
                Console.WriteLine($"{action.Name}: {action.Id}");
            }

            Console.WriteLine("");
            additionalCommand = Console.ReadLine()?.ToLower();
        }

        var transition = actions.FirstOrDefault(x =>
            additionalCommand != null && (x.Id == additionalCommand || x.Name.ToLower().Contains(additionalCommand)));

        if (transition != null)
        {
            await issue.WorkflowTransitionAsync(transition.Name);
            Console.WriteLine($"Issue {issue.Key} has been moved to {transition.Name}.");
            return;
        }

        Console.WriteLine("Invalid Input, Movement failed.");
    }

    private async Task ChangeProjectCommand(string[] commandParameters)
    {
        if (commandParameters[0] != null)
        {
            _settings.Project = await FormatProjectKey(commandParameters[0]);
        }
    }

    internal async Task ListUserIssuesCommand(string[] commandParameters)
    {
        var jiraIssues = await LoadUserIssues(commandParameters[0]);
        _listedIssues = jiraIssues;

        foreach (var issue in jiraIssues)
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"{issue.Key.Value}: {issue.Summary}");
            Console.WriteLine(issue.Status.Name);
            Console.WriteLine(issue.Type.Name);
            Console.WriteLine(issue.Priority.Name);
            Console.WriteLine(issue.Assignee);
            Console.WriteLine("");
        }
    }

    internal async Task ListProjectIssuesCommand(string[] commandParameters)
    {
        var jiraIssues = await LoadProjectIssues(commandParameters[0]);
        _listedIssues = jiraIssues;

        foreach (var issue in jiraIssues)
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"{issue.Key.Value}: {issue.Summary}");
            Console.WriteLine(issue.Status.Name);
            Console.WriteLine(issue.Type.Name);
            Console.WriteLine(issue.Priority.Name);
            Console.WriteLine(issue.Assignee);
            Console.WriteLine("");
        }
    }

    internal async Task OpenCommand(string[] commandParameters)
    {
        if (!commandParameters.Any() || string.IsNullOrEmpty(commandParameters[0]))
        {
            if (_listedIssues != null)
            {
                foreach (var issue in _listedIssues.Take(20))
                {
                    Process.Start("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
                        $"{_settings.Url}browse/{issue.Key}");
                }
            }

            return;
        }

        var issueKey = await FormatIssueKey(commandParameters[0]);
        Process.Start("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
            $"{_settings.Url}browse/{issueKey}");
    }

    private async Task HelpCommand(string[] commandParameters)
    {
        var settingText = _settingHandler.SettingEntryToText(_settings, "Command");
        Console.WriteLine("Available commands are:");
        Console.WriteLine("");
        Console.WriteLine(string.Join(Environment.NewLine, settingText));
    }

    private async Task<Issue> GetIssue(string key)
    {
        try
        {
            var issue = await _jira.Issues.GetIssueAsync(await FormatIssueKey(key));
            return issue;
        }
        catch (Exception e)
        {
            _logger.LogError($"Issue not found: {e}");
            throw;
        }
    }

    private async Task<List<Issue>> LoadProjectIssues(string? user = null)
    {
        var jql = $"project = {_settings.Project.ToUpper()}";
        if (!string.IsNullOrEmpty(user))
        {
            user = await FormatUsername(user);
            jql = $"assignee = {user} AND project = {_settings.Project.ToUpper()}";
        }

        IPagedQueryResult<Issue> jiraIssues;
        try
        {
            jiraIssues = await _jira.Issues.GetIssuesFromJqlAsync(jql, int.MaxValue);
            return jiraIssues.ToList();
        }
        catch (Exception e)
        {
            _logger.LogError($"Loading of Issues failed: {e}");
            throw;
        }
    }

    private async Task<List<Issue>> LoadUserIssues(string? project = null)
    {
        var jql = $"assignee = {_settings.Username}";
        if (!string.IsNullOrEmpty(project))
        {
            project = await FormatProjectKey(project);
            jql = $"assignee = {_settings.Username} AND project = {project}";
        }

        IPagedQueryResult<Issue> jiraIssues;
        try
        {
            jiraIssues = await _jira.Issues.GetIssuesFromJqlAsync(jql, int.MaxValue);
            return jiraIssues.ToList();
        }
        catch (Exception e)
        {
            _logger.LogError($"Loading of Issues failed: {e}");
            throw;
        }
    }

    private async Task<string> FormatIssueKey(string key)
    {
        var project = _settings.Project;
        var keySplit = key.Split('-', StringSplitOptions.RemoveEmptyEntries);
        var keyId = keySplit[0];
        if (keySplit.Length > 1)
        {
            project = keySplit[0];
            keyId = keySplit[1];
        }

        if (int.TryParse(keyId, out _))
        {
            return $"{await FormatProjectKey(project)}-{keyId}";
        }

        return $"{await FormatProjectKey(project)}-{0}";
    }

    private async Task<string> FormatProjectKey(string key)
    {
        var projects = await _jira.Projects.GetProjectsAsync();
        foreach (var project in projects)
        {
            if (project.Key == key.ToUpper())
            {
                return project.Key;
            }
        }

        return _settings.Project.ToUpper();
    }

    private async Task<string> FormatUsername(string? inputUsername)
    {
        if (string.IsNullOrWhiteSpace(inputUsername))
        {
            return _settings.Username;
        }

        var userEntrys = await _jira.Users.SearchUsersAsync(inputUsername, JiraUserStatus.Active, int.MaxValue);
        return userEntrys.Where(x => !x.Username.ToLower().StartsWith("admin")).Select(x => x.Username)
            .FirstOrDefault(_settings.Username);
    }
}