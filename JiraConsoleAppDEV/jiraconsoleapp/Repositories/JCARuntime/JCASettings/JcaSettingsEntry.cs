using System.Reflection;
using JiraConsoleApp.Contracts;

namespace JiraConsoleApp.Repositories.JCARuntime.JCASettings;

public class JcaSettingsEntry : IJcaSettingsEntry
{
    public string Project { get; set; }
    public string Username { get; set; }
    public string Url { get; set; }
    public string[] AssignCommand { get; set; }
    public string[] ChangeProjectCommand { get; set; }
    public string[] CheckInCommand { get; set; }
    public string[] CheckOutCommand { get; set; }
    public string[] DeleteIssueCommand { get; set; }
    public string[] ExitCommand { get; set; }
    public string[] HelpCommand { get; set; }
    public string[] ListProjectIssuesCommand { get; set; }
    public string[] ListUserIssuesCommand { get; set; }
    public string[] LogOutCommand { get; set; }
    public string[] MoveIssueCommand { get; set; }
    public string[] OpenCommand { get; set; }
    public string[] SettingsCommand { get; set; }
    public string[] TestCommand { get; set; }
    public string[] WorklogCommand { get; set; }
    public string[] DisposeParameter { get; set; }
    public string[] ForceParameter { get; set; }
    public string[] NotAssignedParameter { get; set; }
    public string FilePath { get; set; }

    public object? this[string propertyName]
    {
        get
        {
            var propInfo = GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            return propInfo?.GetValue(this);
        }
        set
        {
            var propInfo = GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            propInfo?.SetValue(this, value);
        }
    }
}