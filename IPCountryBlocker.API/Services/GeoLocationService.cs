using System.Text.Json;
using IPCountryBlocker.API.Interfaces;
using IPCountryBlocker.API.Models;

namespace IPCountryBlocker.API.Services
{
    public class GeoLocationService : IGeoLocationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeoLocationService> _logger;
        private readonly string? _apiKey;

        public GeoLocationService(HttpClient httpClient, IConfiguration config, ILogger<GeoLocationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri(config["GeoLocationApi:BaseUrl"] ?? "https://ipapi.co/");
            _apiKey = config["GeoLocationApi:ApiKey"];
        }

        public async Task<IpLookupResponse?> LookupIpAsync(string ipAddress)
        {
            try
            {
                // Note: ipapi.co uses the format: /{ip}/json/
                // If using an API key, it goes in query or headers depending on their current spec.
                var requestUrl = $"{ipAddress}/json/";
                if (!string.IsNullOrEmpty(_apiKey) && _apiKey != "YOUR_IPAPI_KEY_HERE")
                {
                    requestUrl += $"?key={_apiKey}";
                }

                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<IpLookupResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result != null && result.Error)
                {
                    _logger.LogWarning($"GeoLocation API returned an error: {result.Reason}");
                    return null;
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error communicating with GeoLocation API");
                return null; // Handle gracefully
            }
        }
    }
}