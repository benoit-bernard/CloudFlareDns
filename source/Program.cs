// See https://aka.ms/new-console-template for more information

using CloudFlareDynDNS.HostedServices;
using CloundFlaraDynDNS.Services.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;

CreateHostBuilder(args).Build().Run();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            var env = hostingContext.HostingEnvironment;

            config.AddEnvironmentVariables();
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true);
        })
        .ConfigureLogging((logContext, logging) =>
        {
            logging.ClearProviders();
            logging.AddConfiguration(logContext.Configuration.GetSection("Logging")); 
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Error);
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.AddLogging();
            services.AddSingleton<ICloudFlareDnsService, CloudFlareDnsService>();
            services.AddSingleton<IPublicIpService, PublicIpService>();
            services.AddSingleton<IDynDnsService, DynDnsService>();
            services.AddHostedService<HostedService>();

            var url = hostContext.Configuration.GetSection("AppSettings")?.GetChildren()?.FirstOrDefault(x => x.Key == "PublicIpApi")?.Value;
            services.AddHttpClient("public.ip.api", client =>
            {
                client.BaseAddress = new Uri(url);
            });
        })
        .UseConsoleLifetime();
