using IPCountryBlocker.API.Interfaces;
using IPCountryBlocker.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

[ApiController]
[Route("api/[controller]")]
public class IpController : ControllerBase
{
    private readonly IGeoLocationService _geoService;
    private readonly ICountryRepository _countryRepo;
    private readonly ILogRepository _logRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IpController(
        IGeoLocationService geoService,
        ICountryRepository countryRepo,
        ILogRepository logRepo,
        IHttpContextAccessor httpContextAccessor)
    {
        _geoService = geoService;
        _countryRepo = countryRepo;
        _logRepo = logRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status502BadGateway)]
    [HttpGet("lookup")]
    public async Task<IActionResult> LookupIp([FromQuery] string? ipAddress)
    {
        var targetIp = ipAddress;

        // Use caller IP if omitted
        if (string.IsNullOrWhiteSpace(targetIp))
        {
            targetIp = GetCallerIp();
        }

        // Valid IP Format check
        if (!IPAddress.TryParse(targetIp, out _))
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Invalid IP Address format."
            });
        }

        var result = await _geoService.LookupIpAsync(targetIp);

        if (result == null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ApiResponse
            {
                Success = false,
                Message = "Failed to retrieve location from third-party API."
            });
        }

        return Ok(result);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("check-block")]
    public async Task<IActionResult> CheckBlock()
    {
        var callerIp = GetCallerIp(); // Fetch external IP

        var userAgent = _httpContextAccessor.HttpContext?
            .Request.Headers["User-Agent"].ToString() ?? "Unknown";

        var geoData = await _geoService.LookupIpAsync(callerIp);

        bool isBlocked = false;
        string countryCode = "Unknown";

        if (geoData != null && !string.IsNullOrEmpty(geoData.Country))
        {
            countryCode = geoData.Country.ToUpperInvariant();
            isBlocked = _countryRepo.IsBlocked(countryCode); // Check if blocked
        }

        // Log attempt
        _logRepo.AddLog(new BlockedAttemptLog
        {
            IpAddress = callerIp,
            Timestamp = DateTime.UtcNow,
            CountryCode = countryCode,
            BlockedStatus = isBlocked,
            UserAgent = userAgent
        });

        return Ok(new
        {
            success = true,
            ipAddress = callerIp,
            countryCode,
            isBlocked
        });
    }

    // =========================
    // HELPER
    // =========================
    private string GetCallerIp()
    {
        var context = _httpContextAccessor.HttpContext;
        var ip = context?.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (string.IsNullOrEmpty(ip))
        {
            ip = context?.Connection.RemoteIpAddress?.ToString();
        }

        // For local dev, handle IPv6 loopback
        if (ip == "::1") return "8.8.8.8"; // Default to Google DNS for testing if local

        return ip ?? "127.0.0.1";
    }
}