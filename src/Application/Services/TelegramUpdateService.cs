using Microsoft.Extensions.Hosting;
using Steamstatus.db.Interface;
using Steamstatus.Infrastructure.Telegram;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Steamstatus.Application.Services;

public class TelegramUpdateService : BackgroundService
{
    private readonly TelegramBotClient _bot;
    private readonly ITelegramDb<TelegramModel> _db;
    private readonly ILogger<TelegramUpdateService> _logger;

    public TelegramUpdateService(TelegramBotClient bot, ITelegramDb<TelegramModel> db,
        ILogger<TelegramUpdateService> logger)
    {
        _bot = bot;
        _db = db;
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
            var subscriber = new TelegramModel()
            {
                Id = msg.Chat.Id
            };
            var created = _db.Create(subscriber);
            _db.SaveChanges();

            var answer = created
                ? "Ты подписан на уведомления"
                : "Ты уже подписан";
            _logger.LogInformation("{user} - subscribed to notify", subscriber.Id);
            await _bot.SendMessage(msg.Chat.Id, answer);
            return;
        }

        if (msg.Text == "/stop")
        {
            var subscriber = new TelegramModel()
            {
                Id = msg.Chat.Id
            };
            var delete = _db.Delete(subscriber);
            _db.SaveChanges();

            var answer = delete
                ? "Ты больше не подписан на уведомления"
                : "Ты уже неподписан";
            _logger.LogInformation("{user} - unsubscribed to notify", subscriber.Id);
            await _bot.SendMessage(msg.Chat.Id, answer);
            return;
        }


        var sent = await _bot.SendMessage(msg.Chat.Id, "123", replyMarkup: new InlineKeyboardButton[]
        {
            new("switch_inline_query", InlineButtonType.SwitchInlineQuery),
            new("switch_inline_current_chat", InlineButtonType.SwitchInlineQueryCurrentChat)
        });
        await _bot.SendMessage(msg.Chat, $"{msg.From} said : {msg.Text}");
    }
}