using Exceptionless;
using Exceptionless.Models;
using Microsoft.Extensions.Logging;

namespace ERP_Discoteca.Core.Services;

public class ClockService
{
    private readonly ILogger _logger;

    public ClockService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ClockService>();
    }

    public void LogCurrentTime()
    {
        // TODO: Fix ExceptionlessState reference
        // using var _ = _logger.BeginScope(new ExceptionlessState()
        //    .Tag("Clock")
        //    .Property("Machine", Environment.MachineName));

        using var _ = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Tag"] = "Clock",
            ["Machine"] = Environment.MachineName
        });

        _logger.LogInformation("Current time is {Time}", DateTime.UtcNow);
    }
}
