using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Steamstatus.Domain.Enums;
using Steamstatus.Infrastructure.Dota;
using Steamstatus.Infrastructure.Steam;

namespace Steamstatus.Application.Services;

public class StatusMonitorService : BackgroundService
{
    private readonly ILogger<StatusMonitorService> _logger;
    private readonly ISteamStatusClient _steamStatusClient;
    private readonly IDotaCoordinatorClient _dotaCoordinatorClient;

    public StatusMonitorService(ILogger<StatusMonitorService> logger, ISteamStatusClient steamStatusClient,
        IDotaCoordinatorClient dotaCoordinatorClient)
    {
        _logger = logger;
        _steamStatusClient = steamStatusClient;
        _dotaCoordinatorClient = dotaCoordinatorClient;
    }

    private int _okSteam = 0;
    private int _downSteam = 0;
    private ServiceStatus _currentSteamStatus = ServiceStatus.Ok;

    private int _okDota = 0;
    private int _downDota = 0;
    private ServiceStatus _currentDotaStatus = ServiceStatus.Ok;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var steamResult = await _steamStatusClient.CheckSteamApiAsync(stoppingToken);
            var dotaResult = await _dotaCoordinatorClient.CheckDotaAsync(stoppingToken);

            _logger.LogInformation("{Service}: {Status}", steamResult.ServiceName, steamResult.Status);
            _logger.LogInformation("{Service}: {Status}", dotaResult.ServiceName, dotaResult.Status);

            if (steamResult.Status == ServiceStatus.Ok)
            {
                _okSteam += 1;
                _downSteam = 0;
                if (_okSteam >= 2 && _currentSteamStatus != ServiceStatus.Ok)
                {
                    _logger.LogInformation("{Service}: {curStatus} -> {Status}", steamResult.ServiceName,_currentSteamStatus , steamResult.Status);
                    _currentSteamStatus = ServiceStatus.Ok;

                }
            }
            else
            {
                _downSteam += 1;
                _okSteam = 0;
                if (_downSteam >= 3 && _currentSteamStatus != ServiceStatus.Down)
                {
                    _logger.LogInformation("{Service}: {curStatus} -> {Status}", steamResult.ServiceName,_currentSteamStatus , steamResult.Status);
                    _currentSteamStatus = ServiceStatus.Down;
                }
            }

            if (dotaResult.Status == ServiceStatus.Ok)
            {
                _okDota += 1;
                _downDota = 0;
                if (_okDota >= 2 && _currentDotaStatus != ServiceStatus.Ok)
                {
                    _logger.LogInformation("{Service}: {curStatus} -> {Status}", dotaResult.ServiceName,_currentDotaStatus , dotaResult.Status);
                    _currentDotaStatus = ServiceStatus.Ok;
                }
            }
            else
            {
                _downDota += 1;
                _okDota = 0;
                if (_downDota >= 3 && _currentDotaStatus != ServiceStatus.Down)
                {
                    _logger.LogInformation("{Service}: {curStatus} -> {Status}", dotaResult.ServiceName,_currentDotaStatus , dotaResult.Status);
                    _currentDotaStatus = ServiceStatus.Down;
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}