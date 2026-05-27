using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Steamstatus.Application.Services;
using Steamstatus.Configuration;
using Steamstatus.db;
using Steamstatus.db.Interface;
using Steamstatus.Infrastructure.Steam;
using Steamstatus.Infrastructure.Dota;
using Steamstatus.Infrastructure.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


var srcPath = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, @"../../../src")
);

var config = new ConfigurationBuilder()
    .SetBasePath(srcPath)
    .AddJsonFile("appsettings.dev.json")
    .Build();


var host = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
    {
        services.Configure<MonitoringOptions>(config.GetSection("Monitoring"));
        services.AddHostedService<StatusMonitorService>();
        services.AddSingleton<ISteamStatusClient, SteamWebApiStatusClient>();
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        services.AddScoped<ITelegramDb<TelegramModel>, DbBaseCD>();

        var SteamApi = config["Steam:Api"];

        if (string.IsNullOrWhiteSpace(SteamApi))
        {
            throw new InvalidOperationException("Steam API URL is missing");
        }

        services.AddSingleton<IDotaCoordinatorClient>(new DotaCoordinatorClient(SteamApi));
    })
    .Build();


using var cts = new CancellationTokenSource();
var TgToken = config["Telegram:BotToken"];
if (string.IsNullOrWhiteSpace(TgToken))
{
    throw new InvalidOperationException("Telegram Bot Token is missing");
}
var bot = new TelegramBotClient(TgToken, cancellationToken: cts.Token);
bot.OnMessage += OnMessage;

async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text == "/start")
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ITelegramDb<TelegramModel>>();

        var subscrider = new TelegramModel()
        {
            Id = msg.Chat.Id
        };
        var created = db.Create(subscrider);
        db.SaveChanges();

        var answer = created
            ? "Ты подписан на уведомления"
            : "Ты уже подписан";
        await bot.SendMessage(msg.Chat.Id, answer);
        return;
    };
    if (msg.Text == "/stop")
    {
        using var scope = host.Services.CreateScope();
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
        await bot.SendMessage(msg.Chat.Id, answer);
        return;
    };
    var sent = await bot.SendMessage(msg.Chat.Id, "123", replyMarkup: new InlineKeyboardButton[]
    {
        new ("switch_inline_query", InlineButtonType.SwitchInlineQuery),
        new ("switch_inline_current_chat", InlineButtonType.SwitchInlineQueryCurrentChat),
    });
    await bot.SendMessage(msg.Chat, $"{msg.From} said : {msg.Text}");
}

await host.RunAsync();