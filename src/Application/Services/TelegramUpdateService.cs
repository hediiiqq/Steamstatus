using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Steamstatus.db.Interface;
using Steamstatus.Infrastructure.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Steamstatus.Application.Services;

public class TelegramUpdateService : BackgroundService
{
    private readonly TelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramUpdateService> _logger;

    public TelegramUpdateService(
        TelegramBotClient bot,
        IServiceScopeFactory scopeFactory,
        ILogger<TelegramUpdateService> logger)
    {
        _bot = bot;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _bot.OnMessage += OnMessage;

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    private async Task OnMessage(Message msg, UpdateType type)
    {
        if (msg.Text == "/start")
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ITelegramDb<TelegramModel>>();

            var subscriber = new TelegramModel()
            {
                Id = msg.Chat.Id
            };
            var created = db.Create(subscriber);
            db.SaveChanges();

            var answer = created
                ? "Ты подписан на уведомления"
                : "Ты уже подписан";
            _logger.LogInformation("{user} - subscribed to notify", subscriber.Id);
            await _bot.SendMessage(msg.Chat.Id, answer);
            return;
        }

        if (msg.Text == "/stop")
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ITelegramDb<TelegramModel>>();
            var subscriber = new TelegramModel()
            {
                Id = msg.Chat.Id
            };
            var delete = db.Delete(subscriber);
            db.SaveChanges();

            var answer = delete
                ? "Ты больше не подписан на уведомления"
                : "Ты уже неподписан";
            _logger.LogInformation("{user} - unsubscribed to notify", subscriber.Id);
            await _bot.SendMessage(msg.Chat.Id, answer);
        }
    }
}