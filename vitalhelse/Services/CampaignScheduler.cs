namespace VitalHelse.Services;

/// <summary>
/// Keeps campaign prices in step with campaign dates: expired campaigns are switched off and
/// running ones are applied. Runs at startup and every 30 minutes.
/// </summary>
public class CampaignScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CampaignScheduler> _logger;

    public CampaignScheduler(IServiceScopeFactory scopeFactory, ILogger<CampaignScheduler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Let startup (database creation and seeding) finish first
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));
        do
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                scope.ServiceProvider.GetRequiredService<CampaignService>().SyncSchedule();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Campaign schedule sync failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
