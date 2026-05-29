using Steamstatus.Configuration;
using Steamstatus.Domain.Models;

namespace Steamstatus.Infrastructure.Http;

public interface IServiceStatusClient
{
    Task<ServiceCheckResult> CheckServiceStatusAsync(ServiceEndpointOptions endpoint,CancellationToken cancellationToken);
}