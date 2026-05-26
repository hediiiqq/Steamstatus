using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Steamstatus.Application.Services;
using Steamstatus.Configuration;
using Steamstatus.Infrastructure.Steam;
using Steamstatus.Infrastructure.Dota;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


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

        var SteamApi = config["Steam:Api"];

        if (string.IsNullOrWhiteSpace(SteamApi))
        {
            throw new InvalidOperationException("Steam API URL is missing");
        }

        services.AddSingleton<IDotaCoordinatorClient>(new DotaCoordinatorClient(SteamApi));
    })
    .Build();



using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(config["Telegram:BotToken"], cancellationToken: cts.Token);
var me = await bot.GetMe();
bot.OnMessage += OnMessage;

Console.WriteLine($"{me.Username} is now online");
async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text is null) return;
    await bot.SendMessage(msg.Chat,$"{msg.From} said : {msg.Text}");
}
await host.RunAsync();
