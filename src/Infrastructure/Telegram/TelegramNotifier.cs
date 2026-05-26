using Microsoft.Extensions.Options;
using Steamstatus.Domain.Enums;

namespace Steamstatus.Infrastructure.Telegram;

public class TelegramNotifier : ITelegramNotifier
{
    private readonly IOptions<TelegramNotifier> _telegramNotifier;

    public TelegramNotifier(IOptions<TelegramNotifier> telegramNotifier)
    {
        _telegramNotifier = telegramNotifier;
    }
    public async Task NotifyStatusChangedAsync(string serviceName, ServiceStatus oldStatus, ServiceStatus newStatus,
        CancellationToken cancellationToken)
    {

    }
}