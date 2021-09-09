using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core.Enrichers;
using Serilog.Events;
using Serilog.Filters;

namespace WebApi
{
    public class Program
    {
        private static IMetricsRoot _metrics;
        
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var appLifetime = host.Services.GetService<IHostApplicationLifetime>();
            appLifetime.ApplicationStopping.Register(() => OnShutdown(host.Services));
            appLifetime.ApplicationStarted.Register(() => OnStarted(host.Services));
            await host.RunAsync();
        }

        private static void OnStarted(IServiceProvider services)
        {
            var logger = services.GetService<ILogger<Program>>();
            logger.LogInformation("Application started");
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            _metrics = AppMetrics.CreateDefaultBuilder()
                .OutputMetrics.AsPrometheusPlainText()
                .OutputMetrics.AsPrometheusProtobuf()
                .Build();

            return Host.CreateDefaultBuilder(args)
                .UseEnvironment(Environment.GetEnvironmentVariable("WEB_API_ENVIRONMENT") ?? Environments.Development)
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", false)
                        .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", true)
                        .AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(options => { options.ListenAnyIP(80); });
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureMetrics(_metrics)
                .UseMetrics(options =>
                {
                    options.EndpointOptions = endpointOptions =>
                    {
                        endpointOptions.MetricsTextEndpointOutputFormatter = _metrics.OutputMetricsFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First();
                        endpointOptions.MetricsEndpointOutputFormatter = _metrics.OutputMetricsFormatters.OfType<MetricsPrometheusProtobufOutputFormatter>().First();
                    };
                })
                .UseConsoleLifetime(options => options.SuppressStatusMessages = true);
        }

        private static void OnShutdown(IServiceProvider services)
        {
            // Perform graceful shutdown procedures here.
            var logger = services.GetService<ILogger<Program>>();
            logger.LogInformation("Application is shutting down");
        }
    }
}