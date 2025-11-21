using DisconnectionSchedule.Services.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TelegramTemplateBot.Background
{
    public class PageFetchService : BackgroundService
    {
        private readonly ILogger<PageFetchService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly IDataUpdateNotifier _dataUpdateNotifier;

        private const string CacheKey = "LatestPageContent";
        private TimeSpan _fetchInterval;
        private string _targetUrl;

        public PageFetchService(
            ILogger<PageFetchService> logger,
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache,
            IConfiguration configuration,
            IDataUpdateNotifier dataUpdateNotifier)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _dataUpdateNotifier = dataUpdateNotifier;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Load configuration
            _fetchInterval = TimeSpan.FromSeconds(
                _configuration.GetValue<int>("PageFetch:IntervalSeconds", 300));
            _targetUrl = _configuration.GetValue<string>("PageFetch:TargetUrl",
                "https://www.dtek-krem.com.ua/ua/shutdowns");

            _logger.LogInformation(
                "PageFetchService started. Fetching from {Url} every {Interval}",
                _targetUrl, _fetchInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await FetchAndProcessPage();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while fetching and processing page");
                }

                await Task.Delay(_fetchInterval, stoppingToken);
            }
        }

        private async Task FetchAndProcessPage()
        {
            _logger.LogInformation("Fetching page from {Url}", _targetUrl);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(_targetUrl);
            response.EnsureSuccessStatusCode();

            var htmlContent = await response.Content.ReadAsStringAsync();

            // Parse HTML and extract JS from XPath
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            var scriptNode = htmlDocument.DocumentNode.SelectSingleNode("/html/body/script[7]");

            if (scriptNode == null)
            {
                _logger.LogWarning("Script element at XPath '/html/body/script[7]' not found");
                return;
            }

            var scriptContent = scriptNode.InnerText;

            // Check if content has changed
            var previousContent = _memoryCache.Get<string>(CacheKey);

            if (previousContent != scriptContent)
            {
                // Update cache
                _memoryCache.Set(CacheKey, scriptContent, new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.NeverRemove
                });

                _logger.LogInformation("Content updated. Cache refreshed. Content length: {Length}",
                    scriptContent?.Length ?? 0);

                // Notify subscribers
                await _dataUpdateNotifier.NotifyUpdate(scriptContent);
            }
            else
            {
                _logger.LogInformation("No content changes detected");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("PageFetchService is stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}
