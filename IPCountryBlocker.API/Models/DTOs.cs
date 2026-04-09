using System.ComponentModel.DataAnnotations;

namespace IPCountryBlocker.API.Models
{
    public class BlockCountryRequest
    {
        [Required]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters.")]
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
    }

    public class TemporalBlockRequest
    {
        [Required]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters.")]
        public string CountryCode { get; set; } = string.Empty;

        [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes.")]
        public int DurationMinutes { get; set; }
    }

    public class IpLookupResponse
    {
        public string Ip { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Country_Name { get; set; } = string.Empty;
        public string Org { get; set; } = string.Empty; // ISP
        public bool Error { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class PagedResponse<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<T> Data { get; set; } = new List<T>();
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    public class PagedApiResponse<T> : ApiResponse
    {
        public PagedResponse<T> Data { get; set; } = new PagedResponse<T>();
    }

    public class BlockedCountryDto
    {
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public DateTime BlockedAt { get; set; }
    }

    public class TemporalBlockResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public string CountryCode { get; set; } = string.Empty;
    }

    public class UnblockCountryResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
    }
}