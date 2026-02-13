using Microsoft.Extensions.Hosting;
using XcaXds.Commons.Models.Custom;

namespace XcaXds.Tests.FakesAndDoubles;

public class NonRequestingAtnaLogExporter : BackgroundService
{
    private readonly IAtnaLogQueue _atnaLogQueue;
    private readonly AtnaLogExportedChecker _atnaLogExportedChecker;

    public bool AtnaLogExported { get; private set; } = false;

    public NonRequestingAtnaLogExporter(IAtnaLogQueue atnaLogQueue, AtnaLogExportedChecker atnaLogExportedChecker)
    {
        _atnaLogQueue = atnaLogQueue;
        _atnaLogExportedChecker = atnaLogExportedChecker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var auditEventFunction in _atnaLogQueue.DequeueAllAsync(stoppingToken).WithCancellation(stoppingToken))
        {
            var auditEvent = auditEventFunction();

            _atnaLogExportedChecker.AtnaLogExported = true;
        }
    }
}