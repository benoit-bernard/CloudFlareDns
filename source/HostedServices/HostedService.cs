using CloundFlaraDynDNS.Services.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CloudFlareDynDNS.HostedServices
{
    internal class HostedService : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostLifetime;
        private readonly IHostEnvironment         _hostingEnv;
        private readonly IConfiguration           _configuration;
        private readonly ILogger<HostedService>   _logger;
        private readonly IDynDnsService           _iDnsService;

        public HostedService(
            IHostApplicationLifetime hostLifetime, 
            IHostEnvironment hostingEnv, 
            IConfiguration configuration, 
            IDynDnsService dynDnsService,
            ILogger<HostedService> logger)
        {
            _hostLifetime = hostLifetime;
            _hostingEnv = hostingEnv;
            _configuration = configuration;
            _logger = logger;
            _iDnsService = dynDnsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool runOnce = true;
            var  runs    = _configuration["AppSettings:RunOnlyOnce"];
            bool.TryParse(runs, out runOnce);

            _logger.LogInformation($"Executing only once: {runOnce}");

            _logger.LogDebug($"Working dir is {_hostingEnv.ContentRootPath}");
            _logger.LogInformation($".NET environment is {_configuration["DOTNET_ENVIRONMENT"]}");

            await RunService(runOnce);


            _logger.LogInformation("Finished executing. Exiting.");
            _hostLifetime.StopApplication();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting up");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping");
            return base.StopAsync(cancellationToken);
        }

        private async Task RunService(bool runOnce)
        {
            if (runOnce)
            {
                await _iDnsService.UpdateDns();
                return;
            }

            var interval        = _configuration["AppSettings:RunningInterval"];
            int runningInterval = 10;
            int.TryParse(interval, out runningInterval);

            while (!_hostLifetime.ApplicationStopping.IsCancellationRequested)
            {
                await _iDnsService.UpdateDns();
                _logger.LogInformation($"Done updating. Waiting {runningInterval} minutes.");
                _logger.LogInformation($"PRESS CTRL+C to stop");
                await Task.Delay(TimeSpan.FromMinutes(runningInterval));
            }
        }
    }
}
