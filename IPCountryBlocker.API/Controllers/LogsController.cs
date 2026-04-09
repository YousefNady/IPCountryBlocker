using IPCountryBlocker.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IPCountryBlocker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogRepository _logRepo;

        public LogsController(ILogRepository logRepo)
        {
            _logRepo = logRepo;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("blocked-attempts")]
        public IActionResult GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid pagination parameters."
                });
            }

            var result = _logRepo.GetLogs(page, pageSize);

            return Ok(new
            {
                success = true,
                message = "Blocked attempts logs retrieved successfully.",
                data = result
            });
        }
    }
}