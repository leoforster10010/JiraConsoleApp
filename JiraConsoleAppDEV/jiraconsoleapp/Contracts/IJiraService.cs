using Atlassian.Jira;

namespace JiraConsoleApp.Contracts;

public interface IJiraFactory
{
    Task<Jira> GetJiraConnection(bool newConnection = false);
}