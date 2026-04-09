namespace IPCountryBlocker.API.Models
{
    public class BlockedCountry
    {
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public DateTime BlockedAt { get; set; }
    }

    public class TemporalBlock
    {
        public string CountryCode { get; set; } = string.Empty;
        public DateTime BlockedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class BlockedAttemptLog
    {
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public bool BlockedStatus { get; set; }
        public string UserAgent { get; set; } = string.Empty;
    }
}