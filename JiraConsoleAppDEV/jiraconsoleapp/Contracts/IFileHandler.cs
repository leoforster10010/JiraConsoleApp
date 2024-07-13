namespace JiraConsoleApp.Contracts;

public interface IFileHandler
{
    void SerializeToJson<T>(T value, string path);
    T DeserializeJson<T>(string path) where T : class;
}