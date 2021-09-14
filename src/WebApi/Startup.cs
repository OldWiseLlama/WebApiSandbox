using System;
using System.Reflection;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Filtering;
using App.Metrics.Formatters.Ascii;
using App.Metrics.Scheduling;
using MeasurementHelpers.Performance;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Http.BatchFormatters;
using WebApi.Enrichers;
using WebApi.FeatureHelpers;
using WebApi.Middleware;
using WebApi.Options;
using WebApi.Middleware.Exceptions;
using WebApi.Performance;
using WebApi.Services;


namespace WebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFeatureManagement().UseDisabledFeaturesHandler(new DisabledFeatureHandlers());
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "WebApi", Version = "v1"}); });
            services.Configure<HostContext>(options =>
            {
                _configuration.GetSection("HostContext").Bind(options);
            });
            services.AddLogging(builder =>
            {
                var conf = new LoggerConfiguration()
                    .Enrich.WithProperty("ApplicationName", Assembly.GetEntryAssembly()?.FullName ?? "N/A")
                    .Enrich.WithProperty("PodName", _configuration.GetValue<string>("HostContext:PodName"))
                    .Enrich.With(new AmbientContextEnricher())
                    // .MinimumLevel.Information()
                    // .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    // .MinimumLevel.Override("System", LogEventLevel.Warning)
                    // OR rather use
                    .ReadFrom.Configuration(_configuration);

                if (_configuration.GetValue<bool>("LogOptions:WriteToConsole"))
                {
                    conf.WriteTo.Console();
                }
                
                if (_configuration.GetValue<bool>("LogOptions:WriteToHttp"))
                {
                    var uri = _configuration.GetValue<string>("LogOptions:HttpUri");
                    Console.WriteLine("Using HTTP sink for logging with URI {0}", uri);
                    
                    conf.WriteTo.DurableHttpUsingFileSizeRolledBuffers(
                        requestUri: uri,
                        batchFormatter: new ArrayBatchFormatter(),
                        textFormatter: new ElasticsearchJsonFormatter());
                }

                builder.AddSerilog(conf.CreateLogger());
            });
            services.AddExceptionHandlingMiddleware(options => options.WithMapper<NotSupportedExceptionMapper>());
            services.AddHealthChecks();
            services.AddSingleton<IPerformanceIndexValues, PerformanceIndexValues>();
            services.AddSingleton<IServiceInterface, AwesomeService>();
            services.Decorate<IServiceInterface, GeneratedServiceInterfaceMetricsDecorator>();
            services.AddMetricsTrackingMiddleware();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApi v1"));
            
            app.UseMetricsAllMiddleware();
            app.UseMiddleware<CorrelationIdMiddleware>();
            
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            
            //app.UseHttpsRedirection();

            app.UseRouting();
            
            //app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            app.UseHealthChecks("/health");
        }
    }
}