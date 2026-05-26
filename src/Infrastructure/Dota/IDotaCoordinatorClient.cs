

using Steamstatus.Domain.Models;

namespace Steamstatus.Infrastructure.Dota;

public interface IDotaCoordinatorClient
{
    Task<ServiceCheckResult> CheckDotaAsync(CancellationToken cancellationToken);
}