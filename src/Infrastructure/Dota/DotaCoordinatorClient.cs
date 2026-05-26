using Steamstatus.Common.Constants;
using Steamstatus.Domain.Enums;
using Steamstatus.Domain.Models;

namespace Steamstatus.Infrastructure.Dota;

public class DotaCoordinatorClient : IDotaCoordinatorClient
{
    private readonly string _apiKey;

    public DotaCoordinatorClient(string apiKey)
    {
        _apiKey = apiKey;
    }
    public async Task<ServiceCheckResult> CheckDotaAsync(CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();


        try
        {
            var response = await httpClient.GetAsync(
                $"https://api.steampowered.com/IEconDOTA2_570/GetHeroes/v1?key={_apiKey}",
                cancellationToken
            );
            if (response.IsSuccessStatusCode)
                return new ServiceCheckResult()
                {
                    ServiceName = ServiceNames.DotaApi,
                    Status = ServiceStatus.Ok
                };
            return new ServiceCheckResult()
            {
                ServiceName = ServiceNames.DotaApi,
                Status = ServiceStatus.Down,
                ErrorMessage = $"Error: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ServiceCheckResult()
            {
                ServiceName = ServiceNames.DotaApi,
                Status = ServiceStatus.Down,
                ErrorMessage = ex.Message,
            };
        }
    }
}