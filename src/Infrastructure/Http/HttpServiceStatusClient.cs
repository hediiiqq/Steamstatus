using Steamstatus.Configuration;
using Steamstatus.Domain.Enums;
using Steamstatus.Domain.Models;

namespace Steamstatus.Infrastructure.Http;

public class HttpServiceStatusClient : IServiceStatusClient
{
    private static HttpClient _httpClient;
    public HttpServiceStatusClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ServiceCheckResult> CheckServiceStatusAsync(ServiceEndpointOptions endpoint,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint.Url, cancellationToken);
            var statusCode = (int)response.StatusCode;
            if (endpoint.ExpectedStatusCodes.Contains(statusCode))
            {
                return new ServiceCheckResult()
                {
                    ServiceName = endpoint.Name,
                    Status = ServiceStatus.Ok,
                };
            }

            return new ServiceCheckResult()
            {
                ServiceName = endpoint.Name,
                Status = ServiceStatus.Down,
                ErrorMessage = $"Error code: {statusCode}"
            };
        }
        catch (Exception e)
        {
            return new ServiceCheckResult()
            {
                ServiceName = endpoint.Name,
                Status = ServiceStatus.Down,
                Checked = DateTimeOffset.UtcNow,
                ErrorMessage = e.Message
            };
        }
    }
}