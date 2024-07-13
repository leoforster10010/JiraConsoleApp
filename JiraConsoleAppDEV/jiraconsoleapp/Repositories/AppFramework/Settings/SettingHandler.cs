using System.Reflection;
using JiraConsoleApp.Contracts;
using Microsoft.Extensions.Logging;

namespace JiraConsoleApp.Repositories.AppFramework.Settings;

public class SettingHandler : ISettingHandler
{
    private readonly IFileHandler _fileHandler;
    private readonly ILogger<SettingHandler> _logger;

    public SettingHandler(ILogger<SettingHandler> logger, IFileHandler fileHandler)
    {
        _logger = logger;
        _fileHandler = fileHandler;
    }

    public ISettingEntry LoadSettings<T>(string path) where T : class, ISettingEntry
    {
        return _fileHandler.DeserializeJson<T>(path);
    }

    public void SaveSettings<T>(T value) where T : class, ISettingEntry
    {
        _fileHandler.SerializeToJson(value, value.FilePath);
    }

    public IEnumerable<string> SettingEntryToText(ISettingEntry entry, string? pattern = null)
    {
        IEnumerable<string> settingText = new List<string>();
        var properties = entry.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

        if (pattern != null)
        {
            properties = properties.Where(x => x.Name.EndsWith(pattern)).ToArray();
        }

        foreach (var prop in properties.Where(x => x.Name != "Item"))
        {
            try
            {
                var value = prop.GetValue(entry) ?? string.Empty;
                var valueType = value.GetType();
                if (valueType.IsArray && typeof(string).IsAssignableFrom(valueType.GetElementType()))
                {
                    var valueArray = (string[]) value;
                    settingText = settingText.Append($"{prop.Name} = {string.Join(",", valueArray)};");
                }
                else
                {
                    settingText = settingText.Append($"{prop.Name} = {value};");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to read Settings: {e}");
            }
        }

        return settingText;
    }


    public void ChangeSetting(ISettingEntry entry, string propertyName, string[] newValues)
    {
        var propertyType = entry[propertyName]?.GetType();
        if (propertyType == null)
        {
            _logger.LogWarning($"SettingProperty {propertyName} not found");
            return;
        }

        if (propertyType == typeof(string))
        {
            entry[propertyName] = newValues.FirstOrDefault();
        }
        else if (propertyType == typeof(string[]))
        {
            entry[propertyName] = newValues;
        }
        else if (propertyType == typeof(int))
        {
            entry[propertyName] = int.TryParse(newValues.FirstOrDefault(), out _);
        }
        else
        {
            _logger.LogWarning($"SettingPropertyType {propertyType.FullName} not supported");
        }
    }
}