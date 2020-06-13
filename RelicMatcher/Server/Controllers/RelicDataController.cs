using RelicMatcher.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RelicMatcher.Server.Service;

namespace RelicMatcher.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RelicDataController : ControllerBase
    {
        private readonly ILogger<RelicDataController> logger;
        private readonly RelicListService _relicListService;

        public RelicDataController(ILogger<RelicDataController> logger, RelicListService relicListService)
        {
            this.logger = logger;
            _relicListService = relicListService;
        }

        [HttpGet]
        public IEnumerable<RelicType> Get()
        {
            return _relicListService.GetRelics();
        }
    }
}
