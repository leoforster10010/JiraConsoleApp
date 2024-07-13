namespace JiraConsoleApp.Contracts;

public interface ISettingEntry
{
    string FilePath { get; set; }
    object? this[string propertyName] { get; set; }
}