using System.Text.Json;
using JiraConsoleApp.Contracts;

namespace JiraConsoleApp.Repositories.AppFramework;

public class FileHandler : IFileHandler
{
    public void SerializeToJson<T>(T value, string path)
    {
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(path, json);
    }

    public T DeserializeJson<T>(string path) where T : class
    {
        var jsonString = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(jsonString) ?? throw new InvalidOperationException();
    }
}