using DisconnectionSchedule.Configuration;
using DisconnectionSchedule.Services.Implementations;
using DisconnectionSchedule.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ScheduleTrackingBot.TelegramBot.Core;
using ScheduleTrackingBot.TelegramBot.Core.Extensions;
using Telegram.Bot;
using TelegramTemplateBot.Background;
using TelegramTemplateBot.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var botToken = builder.Configuration["TelegramBot:Token"]
    ?? throw new InvalidOperationException("Telegram bot token is not configured");

builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));

builder.Services.AddScoped<IBotUpdateHandler, BotUpdateHandler>();

builder.Services.RegisterTelegramHandlers();

builder.Services.AddHostedService<TelegramBotBackgroundService>();

builder.Services.AddMemoryCache();
builder.Services.AddHostedService<PageFetchService>();
builder.Services.AddHttpClient();

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();
    return new MongoClient(settings.ConnectionString);
});
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddSingleton<IDataUpdateNotifier, DataUpdateNotifier>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IQueueRepository, QueueRepository>();
builder.Services.AddSingleton<IDisconnectionDataParser, DisconnectionDataParser>();
builder.Services.AddSingleton<ITelegramNotificationService, TelegramNotificationService>();
builder.Services.AddScoped<IQueueImageService, QueueImageService>();

builder.Services.AddHostedService<DisconnectionDataParser>(provider =>
    (DisconnectionDataParser)provider.GetRequiredService<IDisconnectionDataParser>());
builder.Services.AddHostedService<TelegramNotificationService>(provider =>
    (TelegramNotificationService)provider.GetRequiredService<ITelegramNotificationService>());

var host = builder.Build();

await host.RunAsync();