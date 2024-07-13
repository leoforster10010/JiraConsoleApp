namespace JiraConsoleApp.Contracts;

public interface IJcaSettingsEntry : ISettingEntry
{
    string Project { get; set; }
    string Username { get; set; }
    string Url { get; set; }
    string[] AssignCommand { get; set; }
    string[] ChangeProjectCommand { get; set; }
    string[] CheckInCommand { get; set; }
    string[] CheckOutCommand { get; set; }
    string[] DeleteIssueCommand { get; set; }
    string[] ExitCommand { get; set; }
    string[] HelpCommand { get; set; }
    string[] ListProjectIssuesCommand { get; set; }
    string[] ListUserIssuesCommand { get; set; }
    string[] LogOutCommand { get; set; }
    string[] MoveIssueCommand { get; set; }
    string[] OpenCommand { get; set; }
    string[] SettingsCommand { get; set; }
    string[] TestCommand { get; set; }
    string[] WorklogCommand { get; set; }
    string[] DisposeParameter { get; set; }
    string[] ForceParameter { get; set; }
    string[] NotAssignedParameter { get; set; }
    string FilePath { get; set; }
    object? this[string propertyName] { get; set; }
}