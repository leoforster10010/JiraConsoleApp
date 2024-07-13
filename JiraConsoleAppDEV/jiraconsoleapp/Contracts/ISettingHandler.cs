namespace JiraConsoleApp.Contracts;

public interface ISettingHandler
{
    void SaveSettings<T>(T value) where T : class, ISettingEntry;
    ISettingEntry LoadSettings<T>(string path) where T : class, ISettingEntry;
    void ChangeSetting(ISettingEntry entry, string propertyName, string[] newValues);
    IEnumerable<string> SettingEntryToText(ISettingEntry entry, string? pattern = null);
}