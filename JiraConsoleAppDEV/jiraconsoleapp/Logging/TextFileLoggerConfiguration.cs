using System.Diagnostics;

namespace JiraConsoleApp.Logging;

public class TextFileLoggerConfiguration
{
    public string Path { get; set; } = $"{Process.GetCurrentProcess().ProcessName}.txt";
}