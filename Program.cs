using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steamstatus.Application.Services;
using Steamstatus.Configuration;
using Steamstatus.db;
using Steamstatus.db.Interface;
using Steamstatus.Infrastructure.Http;
using Steamstatus.Infrastructure.Telegram;
using Telegram.Bot;


var host = Host.CreateDefaultBuilder(args).ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(AppContext.BaseDirectory);
        config.AddJsonFile("appsettings.dev.json", optional: true, reloadOnChange: true);
    }).ConfigureLogging(logging =>
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
        var config = context.Configuration;

        services.Configure<MonitoringOptions>(config.GetSection("Monitoring"));
        services.Configure<TelegramOptions>(config.GetSection("Telegram"));

        services.AddHostedService<StatusMonitorService>();
        services.AddHostedService<TelegramUpdateService>();

        services.AddHttpClient<IServiceStatusClient, HttpServiceStatusClient>();

        services.AddSingleton<TelegramBotClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.BotToken))
            {
                throw new InvalidOperationException("Telegram bot token are required");
            }
            return new TelegramBotClient(options.BotToken);
        });
        services.AddSingleton<ITelegramNotifier, TelegramNotifier>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISubscriberRepository<TelegramModel>, SubscriberRepository>();
    })
    .Build();
await host.RunAsync();