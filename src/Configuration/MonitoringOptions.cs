namespace Steamstatus.Configuration;

public sealed class MonitoringOptions
{
    public int IntervalSeconds { get; set; }
    public int FailureThreshold { get; set; }
    public int RecoveryThreshold { get; set; }
}