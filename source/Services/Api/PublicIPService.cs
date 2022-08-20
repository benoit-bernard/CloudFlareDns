using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Net.Http.Json;

namespace CloundFlaraDynDNS.Services.Api
{
    public class PublicIpService : IPublicIpService
    {
        ILogger                           _logger;
        IHttpClientFactory                _httpClientFactory;
        private          HttpClient       _client;
        private readonly AsyncRetryPolicy _retryPolicy;

        public PublicIpService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PublicIpService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            var retryConfig = configuration["AppSettings:MaxRetry"];
            var done        = int.TryParse(retryConfig, out var MaxRetry);
            if (!done)
                MaxRetry = 1;

            _retryPolicy = Policy.Handle<Exception>().RetryAsync(MaxRetry);
            _client = httpClientFactory.CreateClient("public.ip.api");
        }
        public async Task<string> GetPublicIp()
        {
            return await _retryPolicy.ExecuteAsync(async () => await GetPublicIpAsync());
        }

        private async Task<string> GetPublicIpAsync()
        {
            return await _client.GetStringAsync("/");
        }
    }
}
