using Steamstatus.Domain.Enums;

namespace Steamstatus.Infrastructure.Telegram;

public interface ITelegramNotifier
{
    Task NotifyStatusChangedAsync(
        string serviceName,
        ServiceStatus oldStatus,
        ServiceStatus newStatus,
        CancellationToken cancellationToken);
}