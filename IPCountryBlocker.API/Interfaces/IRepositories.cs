using IPCountryBlocker.API.Models;

namespace IPCountryBlocker.API.Interfaces
{
    public interface ICountryRepository
    {
        bool AddBlock(BlockedCountry country);
        bool RemoveBlock(string countryCode);
        bool IsBlocked(string countryCode);
        PagedResponse<BlockedCountry> GetBlockedCountries(int page, int pageSize, string? searchTerm);

        bool AddTemporalBlock(TemporalBlock block);
        void RemoveExpiredTemporalBlocks();
        bool IsValidCountryCode(string countryCode);
    }

    public interface ILogRepository
    {
        void AddLog(BlockedAttemptLog log);
        PagedResponse<BlockedAttemptLog> GetLogs(int page, int pageSize);
    }
}