using Microsoft.Extensions.DependencyInjection;
using Steamstatus.db.Interface;
using Steamstatus.Domain.Enums;
using Telegram.Bot;

namespace Steamstatus.Infrastructure.Telegram;

public class TelegramNotifier : ITelegramNotifier
{

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TelegramBotClient _bot;
    public TelegramNotifier(TelegramBotClient bot,IServiceScopeFactory scopeFactory)
    {
        _scopeFactory =  scopeFactory;
        _bot = bot;
    }

    public async Task NotifyStatusChangedAsync(string serviceName, ServiceStatus oldStatus, ServiceStatus newStatus,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ISubscriberRepository<TelegramModel>>();
        var subscribers =  db.GetAll();
        var title = newStatus == ServiceStatus.Down
            ? $"⚠️ {serviceName} недоступен"
            : $"✅ {serviceName} восстановлен";
        var message = $"{title}\nСтатус {oldStatus} -> {newStatus}";

        foreach (var subscriber in subscribers)
        {
            await _bot.SendMessage(subscriber.Id, message, cancellationToken: cancellationToken);
        }
    }
}