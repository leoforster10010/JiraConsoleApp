using JiraConsoleApp.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JiraConsoleApp;

public class Kernel : IHostedService
{
    private readonly IJcaCore _jcaCore;
    private readonly ILogger<Kernel> _logger;
    private Task _runningTask;

    public Kernel(IJcaCore jcaCore, ILogger<Kernel> logger)
    {
        _jcaCore = jcaCore;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("StartUp");
        try
        {
            _runningTask = _jcaCore.OnStartUp(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e.ToString());
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("ShutDown");
        try
        {
            await _runningTask;
        }
        catch (Exception e)
        {
            _logger.LogCritical(e.ToString());
        }
    }
}