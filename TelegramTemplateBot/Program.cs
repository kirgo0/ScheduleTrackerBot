using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core;
using ScheduleTrackingBot.TelegramBot.Core.Extensions;
using Telegram.Bot;

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

var host = builder.Build();

await host.RunAsync();