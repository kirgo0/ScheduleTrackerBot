# Telegram Bot .NET 10 Template

## 🚀 Features

- **Attribute-Based Routing**: Use simple attributes like `[HandleCommand("/start")]` or `[HandleCallback("hello")]` to register handlers
- **Automatic DI Registration**: All handlers are automatically discovered and registered as scoped services
- **Full Update Type Support**: Handle messages, commands, callbacks, stickers, photos, videos, documents, voice, audio, locations, contacts, inline queries, polls, and more
- **Background Hosted Service**: Bot runs as a .NET hosted service with proper lifecycle management
- **Microsoft Logging**: Built-in logging with configurable levels via appsettings.json
- **Flexible Configuration**: Easy configuration through appsettings.json
- **Scalable Architecture**: Easy to extend with new handlers without modifying core code

## 📋 Prerequisites

- .NET 10 SDK
- A Telegram Bot Token (get one from [@BotFather](https://t.me/botfather))

## 🛠️ Setup

1. **Clone or copy the template**

2. **Configure your bot token**
   
   Edit `appsettings.json` or `appsettings.Development.json`:
   ```json
   {
     "TelegramBot": {
       "Token": "YOUR_BOT_TOKEN_HERE"
     }
   }
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the bot**
   ```bash
   dotnet run
   ```

## 🎯 Creating New Handlers

### 1. Command Handler

```csharp
[HandleCommand("/mycommand")]
public class MyCommandHandler : ITelegramHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<MyCommandHandler> _logger;

    public MyCommandHandler(
        ITelegramBotClient botClient,
        ILogger<MyCommandHandler> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
    {
        var message = update.Message;
        if (message?.Chat == null) return;

        await _botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "Hello from my command!",
            cancellationToken: cancellationToken);
    }
}
```

### 2. Callback Query Handler

```csharp
[HandleCallback("button_data")]
public class MyCallbackHandler : ITelegramHandler
{
    private readonly ITelegramBotClient _botClient;

    public MyCallbackHandler(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
    {
        var callbackQuery = update.CallbackQuery;
        if (callbackQuery?.Message == null) return;

        await _botClient.AnswerCallbackQuery(
            callbackQueryId: callbackQuery.Id,
            text: "Button clicked!",
            cancellationToken: cancellationToken);
    }
}
```

### 3. Text Message Handler

```csharp
[HandleText("keyword")]
public class MyTextHandler : ITelegramHandler
{
    private readonly ITelegramBotClient _botClient;

    public MyTextHandler(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
    {
        var message = update.Message;
        if (message?.Chat == null) return;

        await _botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "I detected your keyword!",
            cancellationToken: cancellationToken);
    }
}
```

### 4. Photo Handler

```csharp
[HandlePhoto]
public class MyPhotoHandler : ITelegramHandler
{
    private readonly ITelegramBotClient _botClient;

    public MyPhotoHandler(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
    {
        var message = update.Message;
        if (message?.Photo == null || message.Chat == null) return;

        await _botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "Nice photo!",
            cancellationToken: cancellationToken);
    }
}
```

## 🏷️ Available Attributes

| Attribute | Description | Example |
|-----------|-------------|---------|
| `[HandleCommand]` | Handles bot commands | `[HandleCommand("/start")]` |
| `[HandleCallback]` | Handles callback queries | `[HandleCallback("button_id")]` |
| `[HandleText]` | Handles text messages containing pattern | `[HandleText("hello")]` |
| `[HandleSticker]` | Handles sticker messages | `[HandleSticker]` |
| `[HandlePhoto]` | Handles photo messages | `[HandlePhoto]` |
| `[HandleVideo]` | Handles video messages | `[HandleVideo]` |
| `[HandleDocument]` | Handles document messages | `[HandleDocument]` |
| `[HandleVoice]` | Handles voice messages | `[HandleVoice]` |
| `[HandleAudio]` | Handles audio messages | `[HandleAudio]` |
| `[HandleLocation]` | Handles location messages | `[HandleLocation]` |
| `[HandleContact]` | Handles contact messages | `[HandleContact]` |
| `[HandleInlineQuery]` | Handles inline queries | `[HandleInlineQuery("pattern")]` |
| `[HandleChosenInlineResult]` | Handles chosen inline results | `[HandleChosenInlineResult]` |
| `[HandlePoll]` | Handles poll updates | `[HandlePoll]` |
| `[HandlePollAnswer]` | Handles poll answers | `[HandlePollAnswer]` |

## 💡 Key Features Explained

### Automatic Registration

All classes implementing `ITelegramHandler` and decorated with handler attributes are automatically:
1. Discovered at startup
2. Registered in the DI container as **scoped** services
3. Mapped to their respective update types

This happens in `ServiceCollectionExtensions.RegisterTelegramHandlers()`.

### Dependency Injection

You can inject any registered service into your handlers:

```csharp
public class MyHandler : ITelegramHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<MyHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly MyCustomService _customService;

    public MyHandler(
        ITelegramBotClient botClient,
        ILogger<MyHandler> logger,
        IConfiguration configuration,
        MyCustomService customService)
    {
        _botClient = botClient;
        _logger = logger;
        _configuration = configuration;
        _customService = customService;
    }
}
```

### Multiple Attributes

A single handler can handle multiple patterns:

```csharp
[HandleCommand("/start")]
[HandleCommand("/begin")]
[HandleCommand("/hello")]
public class MultiCommandHandler : ITelegramHandler
{
    // Handles all three commands
}
```

```csharp
[HandleText("hello")]
[HandleText("hi")]
[HandleText("hey")]
public class GreetingHandler : ITelegramHandler
{
    // Handles messages containing any of these words
}
```

## 📚 Resources

- [Telegram Bot API Documentation](https://core.telegram.org/bots/api)
- [Telegram.Bot Library](https://github.com/TelegramBots/Telegram.Bot)
- [.NET Generic Host Documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host)

## 📄 License

This template is free to use for any purpose.

## 🤝 Contributing

Feel free to extend this template and share your improvements!
