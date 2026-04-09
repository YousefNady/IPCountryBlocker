using IPCountryBlocker.API.Interfaces;
using IPCountryBlocker.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace IPCountryBlocker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryRepository _countryRepo;

        public CountriesController(ICountryRepository countryRepo)
        {
            _countryRepo = countryRepo;
        }

        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        [HttpPost("block")]
        public IActionResult BlockCountry([FromBody] BlockCountryRequest request)
        {
            var blockedCountry = new BlockedCountry
            {
                CountryCode = request.CountryCode.ToUpperInvariant(),
                CountryName = request.CountryName,
                BlockedAt = DateTime.UtcNow
            };

            if (_countryRepo.AddBlock(blockedCountry))
            {
                return Ok(new ApiResponse { Success = true, Message = "Country blocked successfully." });
            }

            return Conflict(new ApiResponse { Success = false, Message = "Country is already blocked." }); // Prevent duplicates
        }

        [ProducesResponseType(typeof(UnblockCountryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnblockCountryResponseDto), StatusCodes.Status404NotFound)]
        [HttpDelete("block/{countryCode}")]
        public IActionResult UnblockCountry(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                return BadRequest(new ApiResponse { Success = false, Message = "Country code is required." });

            countryCode = countryCode.ToUpperInvariant();

            if (_countryRepo.RemoveBlock(countryCode))
            {
                return Ok(new UnblockCountryResponseDto
                {
                    Success = true,
                    Message = "Country unblocked successfully.",
                    CountryCode = countryCode
                });
            }

            return NotFound(new UnblockCountryResponseDto
            {
                Success = false,
                Message = "Country is not currently blocked.",
                CountryCode = countryCode
            });
        }

        [ProducesResponseType(typeof(PagedResponse<BlockedCountryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("blocked")]
        public IActionResult GetBlockedCountries([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid pagination parameters." });

            var pagedResult = _countryRepo.GetBlockedCountries(page, pageSize, search);

            var blockedCountriesDto = pagedResult.Data.Select(c => new BlockedCountryDto
            {
                CountryCode = c.CountryCode,
                CountryName = c.CountryName,
                BlockedAt = c.BlockedAt
            });

            var response = new PagedApiResponse<BlockedCountryDto>
            {
                Success = true,
                Message = "Blocked countries retrieved successfully.",
                Data = new PagedResponse<BlockedCountryDto>
                {
                    Page = pagedResult.Page,
                    PageSize = pagedResult.PageSize,
                    TotalCount = pagedResult.TotalCount,
                    Data = blockedCountriesDto
                }
            };

            return Ok(response);
        }

        [ProducesResponseType(typeof(TemporalBlockResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(TemporalBlockResponseDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost("temporal-block")]
        public IActionResult TemporalBlock([FromBody] TemporalBlockRequest request)
        {
            if (request.DurationMinutes < 1 || request.DurationMinutes > 1440)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Duration must be between 1 and 1440 minutes."
                });
            }

            if (!_countryRepo.IsValidCountryCode(request.CountryCode))
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid country code."
                });
            }

            var now = DateTime.UtcNow;

            var block = new TemporalBlock
            {
                CountryCode = request.CountryCode.ToUpperInvariant(),
                BlockedAt = now,
                ExpiresAt = now.AddMinutes(request.DurationMinutes)
            };

            var added = _countryRepo.AddTemporalBlock(block);

            if (!added)
            {
                return Conflict(new TemporalBlockResponseDto
                {
                    Success = false,
                    Message = "Country is already temporarily blocked.",
                    CountryCode = block.CountryCode,
                    DurationMinutes = 0
                });
            }

            return Ok(new TemporalBlockResponseDto
            {
                Success = true,
                Message = $"Country temporarily blocked for {request.DurationMinutes} minutes.",
                DurationMinutes = request.DurationMinutes,
                CountryCode = block.CountryCode
            });
        }
    }
}