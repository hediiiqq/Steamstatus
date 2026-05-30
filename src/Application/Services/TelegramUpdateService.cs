using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steamstatus.Configuration;
using Steamstatus.db.Interface;
using Steamstatus.Infrastructure.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Steamstatus.Application.Services;

public class TelegramUpdateService : BackgroundService
{
    private readonly TelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramUpdateService> _logger;
    private readonly MonitoringOptions _options;

    public TelegramUpdateService(
        TelegramBotClient bot,
        IServiceScopeFactory scopeFactory,
        ILogger<TelegramUpdateService> logger,
        IOptions<MonitoringOptions> options)
    {
        _bot = bot;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _bot.OnMessage += OnMessage;
        _bot.OnUpdate += OnUpdate;

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    private async Task OnMessage(Message msg, UpdateType type)
    {
        if (msg.Text == "/start")
        {
            var chatId = msg.Chat.Id;

            var services = _options.Services;
            var buttons = services.Select(service =>
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(service.Name, $"Subscribe|{service.Name}")
                });
            var keyboard = new InlineKeyboardMarkup(buttons);
            await _bot.SendMessage(chatId, "Select service:", replyMarkup: keyboard);
        }

        if (msg.Text == "/stop")
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ISubscriberRepository<TelegramModel>>();
            var chatId = msg.Chat.Id;
            var services = db.GetByChatId(chatId);
            var buttons = services.Select(service =>
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(service.ServiceName, $"Unsubscribe|{service.ServiceName}")
                });
            var keyboard = new InlineKeyboardMarkup(buttons);
            await _bot.SendMessage(chatId, "Select service:", replyMarkup: keyboard);
        }
    }

    private async Task OnCallbackQuery(CallbackQuery query)
    {
        await _bot.AnswerCallbackQuery(query.Id);
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ISubscriberRepository<TelegramModel>>();
        var chatId = query.Message.Chat.Id;

        var data = query.Data;
        var parts = data.Split("|");
        var action = parts[0];
        var serviceId = parts[1];

        if (action == "Subscribe")
        {
            var created = db.Create(chatId, serviceId);
            db.SaveChanges();
            var answer = created
                ? $"You subscribed to {serviceId}"
                : $"You all ready subscribed to  {serviceId}";
            await _bot.SendMessage(chatId, answer);
        }

        if (action == "Unsubscribe")
        {
            var deleted = db.Delete(chatId,serviceId);
            db.Delete(chatId, serviceId);
            db.SaveChanges();
            var answer = deleted
                ? $"You unsubscribed to {serviceId}"
                : $"You all ready unsubscribed to  {serviceId}";
            await _bot.SendMessage(chatId, answer);
        }
    }

    private async Task OnUpdate(Update update)
    {
        await OnCallbackQuery(update.CallbackQuery);
    }
}