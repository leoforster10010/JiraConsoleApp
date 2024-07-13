namespace JiraConsoleApp.Contracts;

public interface IJcaCore
{
    Task OnStartUp(CancellationToken cancellationToken);
}