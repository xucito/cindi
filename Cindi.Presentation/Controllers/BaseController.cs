using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public abstract class BaseController : Controller
    {
        private IMediator _mediator;

        protected IMediator Mediator => _mediator ?? (_mediator = (IMediator)HttpContext.RequestServices.GetService(typeof(IMediator)));

        private ILogger _logger;
        protected ILogger Logger => _logger;

        public BaseController(ILogger logger)
        {
            _logger = logger;
        }
    }
}
