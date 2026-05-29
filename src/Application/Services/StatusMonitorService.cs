using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steamstatus.Configuration;
using Steamstatus.Domain.Enums;
using Steamstatus.Infrastructure.Http;
using Steamstatus.Infrastructure.Telegram;


namespace Steamstatus.Application.Services;

public class StatusMonitorService : BackgroundService
{
    private readonly ILogger<StatusMonitorService> _logger;
    private readonly IServiceStatusClient _serviceStatusClient;
    private readonly MonitoringOptions _monitoringOptions;
    private readonly ITelegramNotifier _telegramNotifier;
    private readonly Dictionary<string, ServiceMonitorState> _states = new();

    public StatusMonitorService(ILogger<StatusMonitorService> logger, IServiceStatusClient serviceStatusClient,
        IOptions<MonitoringOptions> monitoringOptions,
        ITelegramNotifier telegramNotifier)
    {
        _logger = logger;
        _serviceStatusClient = serviceStatusClient;
        _monitoringOptions = monitoringOptions.Value;
        _telegramNotifier = telegramNotifier;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var endpoint in _monitoringOptions.Services)
            {
                var result = await _serviceStatusClient.CheckServiceStatusAsync(endpoint, stoppingToken);
                if (!_states.TryGetValue(result.ServiceName, out var state))
                {
                    state = new ServiceMonitorState();
                    _states[result.ServiceName] = state;
                }

                if (result.Status == ServiceStatus.Ok)
                {
                    state.SuccessCount++;
                    state.FailureCount = 0;
                    _logger.LogInformation(
                        "Checked {Service}: {Status}",
                        result.ServiceName,
                        result.Status);
                    if (result.Status == ServiceStatus.Ok &&
                        state.SuccessCount >= _monitoringOptions.RecoveryThreshold &&
                        state.CurrentStatus != ServiceStatus.Ok)
                    {
                        await _telegramNotifier.NotifyStatusChangedAsync(result.ServiceName, state.CurrentStatus,
                            ServiceStatus.Ok, stoppingToken);
                        _logger.LogWarning(
                            "{Service} status changed: {OldStatus} -> {NewStatus}",
                            result.ServiceName,
                            state.CurrentStatus,
                            ServiceStatus.Down);
                        state.CurrentStatus = ServiceStatus.Ok;
                    }
                }
                else
                {
                    state.FailureCount++;
                    state.SuccessCount = 0;
                    _logger.LogInformation(
                        "Checked {Service}: {Status}",
                        result.ServiceName,
                        result.Status);
                    if (result.Status == ServiceStatus.Down &&
                        state.FailureCount >= _monitoringOptions.FailureThreshold &&
                        state.CurrentStatus != ServiceStatus.Down)
                    {
                        await _telegramNotifier.NotifyStatusChangedAsync(result.ServiceName, state.CurrentStatus,
                            ServiceStatus.Down, stoppingToken);
                        _logger.LogWarning(
                            "{Service} status changed: {OldStatus} -> {NewStatus}",
                            result.ServiceName,
                            state.CurrentStatus,
                            ServiceStatus.Down);
                        state.CurrentStatus = ServiceStatus.Down;
                    }
                }
            }

            // poling interval
            var baseInterval = _monitoringOptions.IntervalSeconds;
            var hasDownService = _states.Values.Any(state => state.CurrentStatus == ServiceStatus.Down);
            var delaySeconds = hasDownService
                ? Math.Max(1, baseInterval / 3) //
                : baseInterval;
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }
    }
}