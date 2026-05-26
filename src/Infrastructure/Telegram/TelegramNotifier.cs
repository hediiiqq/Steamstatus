using Microsoft.Extensions.Options;
using Steamstatus.Domain.Enums;

namespace Steamstatus.Infrastructure.Telegram;

public class TelegramNotifier(IOptions<TelegramNotifier> telegramNotifier) : ITelegramNotifier
{
    private readonly IOptions<TelegramNotifier> _telegramNotifier = telegramNotifier;

    public async Task NotifyStatusChangedAsync(string serviceName, ServiceStatus oldStatus, ServiceStatus newStatus,
        CancellationToken cancellationToken)
    {
        var title = newStatus == ServiceStatus.Down
            ? $"⚠️ {serviceName} недоступен"
            : $"✅ {serviceName} восстановлен";
        var message = $"{title}\nСтатус {oldStatus} -> {newStatus}";
    }
}