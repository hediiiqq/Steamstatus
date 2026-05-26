using Steamstatus.Domain.Enums;

namespace Steamstatus.Domain.Models;

public class ServiceCheckResult
{
    public string ServiceName { get; set; } = string.Empty;
    public ServiceStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset Checked { get; init; } = DateTimeOffset.UtcNow;

    public bool IsAvalible => Status == ServiceStatus.Ok;
}