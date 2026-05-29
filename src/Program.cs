using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steamstatus.Application.Services;
using Steamstatus.Configuration;
using Steamstatus.db;
using Steamstatus.db.Interface;
using Steamstatus.Infrastructure.Http;
using Steamstatus.Infrastructure.Telegram;
using Telegram.Bot;

var srcPath = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, @"../../../src")
);

var config = new ConfigurationBuilder()
    .SetBasePath(srcPath)
    .AddJsonFile("appsettings.dev.json")
    .Build();

var tgToken = config["Telegram:BotToken"];
if (string.IsNullOrWhiteSpace(tgToken))
{
    throw new InvalidOperationException("Telegram Bot Token is missing");
}


var host = Host.CreateDefaultBuilder(args).ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });
        logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
    }).ConfigureServices((context, services) =>
    {
        services.AddHostedService<StatusMonitorService>();
        services.AddHostedService<TelegramUpdateService>();

        services.AddHttpClient<IServiceStatusClient, HttpServiceStatusClient>();

        services.Configure<MonitoringOptions>(config.GetSection("Monitoring"));

        services.AddSingleton(new TelegramBotClient(tgToken));
        services.AddSingleton<ITelegramNotifier, TelegramNotifier>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITelegramDb<TelegramModel>, DbBaseCD>();
    })
    .Build();
await host.RunAsync();