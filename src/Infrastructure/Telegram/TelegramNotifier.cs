using Steamstatus.db.Interface;
using Steamstatus.Domain.Enums;
using Telegram.Bot;

namespace Steamstatus.Infrastructure.Telegram;

public class TelegramNotifier(TelegramBotClient bot, ITelegramDb<TelegramModel> telegramModel) : ITelegramNotifier
{
    private readonly ITelegramDb<TelegramModel> _telegramModel = telegramModel;
    private readonly TelegramBotClient _bot = bot;

    public async Task NotifyStatusChangedAsync(string serviceName, ServiceStatus oldStatus, ServiceStatus newStatus,
        CancellationToken cancellationToken)
    {
        var title = newStatus == ServiceStatus.Down
            ? $"⚠️ {serviceName} недоступен"
            : $"✅ {serviceName} восстановлен";
        var message = $"{title}\nСтатус {oldStatus} -> {newStatus}";

        var subscribers = _telegramModel.GetAllList();
        foreach (var subscriber in subscribers)
        {
            await _bot.SendMessage(subscriber.Id, message, cancellationToken: cancellationToken);
        }
    }
}