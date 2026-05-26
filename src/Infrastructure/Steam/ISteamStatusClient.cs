

using Steamstatus.Domain.Models;

namespace Steamstatus.Infrastructure.Steam;

public interface ISteamStatusClient
{
    Task<ServiceCheckResult> CheckSteamApiAsync(CancellationToken cancellationToken);
}