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
        services.AddHostedService<TelegramUpdateService>();
        services.AddSingleton<ISteamStatusClient, SteamWebApiStatusClient>();
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        services.AddScoped<ITelegramDb<TelegramModel>, DbBaseCD>();
        services.AddScoped<ITelegramNotifier, TelegramNotifier>();
        var tgToken = config["Telegram:BotToken"];
        if (string.IsNullOrWhiteSpace(tgToken))
        {
            throw new InvalidOperationException("Telegram Bot Token is missing");
        }
        services.AddSingleton(new TelegramBotClient(tgToken));

        var steamApi = config["Steam:Api"];

        if (string.IsNullOrWhiteSpace(steamApi))
        {
            throw new InvalidOperationException("Steam API URL is missing");
        }

        services.AddSingleton<IDotaCoordinatorClient>(new DotaCoordinatorClient(steamApi));
    })
    .Build();
await host.RunAsync();