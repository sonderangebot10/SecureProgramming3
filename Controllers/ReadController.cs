using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureProgramming3.Services;

namespace SecureProgramming3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReadController : ControllerBase
    {
        private readonly ILogger<ReadController> _logger;
        private readonly IParallelReaderService _reader;

        public ReadController(ILogger<ReadController> logger, IParallelReaderService reader)
        {
            _logger = logger;
            _reader = reader;
        }

        [HttpGet("addThread")]
        public async Task<ActionResult<bool>> AddThreadAsync()
        {
            _logger.LogInformation("Add thread operation.");

            var threads = await _reader.AddThread();

            return Ok(threads);
        }

        [HttpGet("removeThread")]
        public async Task<ActionResult<int>> RemoveThreadAsync()
        {
            _logger.LogInformation("Remove thread operation.");

            if (!_reader.IsInitated)
            {
                _logger.LogInformation("Reader is not initiated.");
                return BadRequest(0);
            }

            var threads = await _reader.RemoveThread();

            return Ok(threads);
        }
    }
}
