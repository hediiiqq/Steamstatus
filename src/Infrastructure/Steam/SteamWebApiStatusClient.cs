using Steamstatus.Common.Constants;
using Steamstatus.Domain.Enums;
using Steamstatus.Domain.Models;

namespace Steamstatus.Infrastructure.Steam;

public class SteamWebApiStatusClient : ISteamStatusClient
{
    public async Task<ServiceCheckResult> CheckSteamApiAsync(CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();


        try
        {
            var response = await httpClient.GetAsync(
                $"https://api.steampowered.com/ISteamWebAPIUtil/GetServerInfo/v1/",
                cancellationToken);
            if (response.IsSuccessStatusCode)
                return new ServiceCheckResult()
                {
                    ServiceName = ServiceNames.SteamApi,
                    Status = ServiceStatus.Ok
                };
            return new ServiceCheckResult()
            {
                ServiceName = ServiceNames.SteamApi,
                Status = ServiceStatus.Down,
                ErrorMessage = $"Error: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ServiceCheckResult()
            {
                ServiceName = ServiceNames.SteamApi,
                Status = ServiceStatus.Down,
                ErrorMessage = ex.Message,
            };
        }
    }
}