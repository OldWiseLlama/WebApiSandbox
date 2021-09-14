using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using WebApi.Options;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IFeatureManager _featureManager;
        private readonly IServiceInterface _serviceInterface;
        private readonly HostContext _hostContext;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IOptions<HostContext> hostContext, IFeatureManager featureManager, IServiceInterface serviceInterface)
        {
            _logger = logger;
            _featureManager = featureManager;
            _serviceInterface = serviceInterface;
            _hostContext = hostContext.Value;
        }

        [HttpGet]
        [FeatureGate("UseAwesomeFeature")]
        public async Task<IEnumerable<WeatherForecast>> Get(CancellationToken cancellationToken)
        {
            bool isCanceled = cancellationToken.IsCancellationRequested;
            _logger.LogInformation("Executing GET in pod {podName}", _hostContext.PodName);

            var rng = new Random();
            
            await _serviceInterface.PerformAsync(TimeSpan.FromMilliseconds(rng.Next(0, 1000)), cancellationToken);

            var number = await _serviceInterface.GetNumberAsync();
            
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();
        }

        [HttpGet]
        [Route("explode")]
        public async Task<IActionResult> Explode()
        {
            throw new NotSupportedException("Oh noes I exploded!");
        }
    }
}