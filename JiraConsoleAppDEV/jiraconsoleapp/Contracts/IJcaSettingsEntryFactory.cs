namespace JiraConsoleApp.Contracts;

public interface IJcaSettingsEntryFactory
{
    IJcaSettingsEntry GetJcaSettingsEntry(bool newEntry = false);
}