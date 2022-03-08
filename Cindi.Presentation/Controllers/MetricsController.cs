using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cindi.Application.Metrics.Queries.GetMetrics;
using Cindi.Application.Options;
using Cindi.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cindi.Presentation.Controllers
{
    /*[Route("api/[controller]")]
    public class MetricsController : BaseController
    {
        private CindiClusterOptions _option;
        public MetricsController(ILoggerFactory logger,
            IOptionsMonitor<CindiClusterOptions> options) : base(logger.CreateLogger<StepsController>())
        {
            _option = options.CurrentValue;
            options.OnChange((change) =>
            {
                _option = change;
            });
        }

        // GET: api/<controller>
        [HttpPost("request")]
        public async Task<IActionResult> Get(GetMetricsQuery query)
        {
            var result = await Mediator.Send(query);

            return Ok(new HttpQueryResult<object, object>(result, result.Result));
        }
    }*/
}
