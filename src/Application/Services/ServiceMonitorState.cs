using Steamstatus.Domain.Enums;

namespace Steamstatus.Application.Services;

public class ServiceMonitorState
{
    public ServiceStatus CurrentStatus { get; set; } = ServiceStatus.Ok;
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}