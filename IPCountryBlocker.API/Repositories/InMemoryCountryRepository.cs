using System.Collections.Concurrent;
using IPCountryBlocker.API.Interfaces;
using IPCountryBlocker.API.Models;

namespace IPCountryBlocker.API.Repositories
{
    public class InMemoryCountryRepository : ICountryRepository
    {
        private readonly ConcurrentDictionary<string, BlockedCountry> _blockedCountries = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, TemporalBlock> _temporalBlocks = new(StringComparer.OrdinalIgnoreCase);

        public bool AddBlock(BlockedCountry country)
        {
            return _blockedCountries.TryAdd(country.CountryCode, country);
        }

        public bool RemoveBlock(string countryCode)
        {
            _temporalBlocks.TryRemove(countryCode, out _); // Clean up temporal if manual delete occurs
            return _blockedCountries.TryRemove(countryCode, out _);
        }

        public bool IsBlocked(string countryCode)
        {
            return _blockedCountries.ContainsKey(countryCode) || _temporalBlocks.ContainsKey(countryCode);
        }

        public PagedResponse<BlockedCountry> GetBlockedCountries(int page, int pageSize, string? searchTerm)
        {
            var query = _blockedCountries.Values.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.CountryCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.CountryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            var total = query.Count();
            var data = query.OrderByDescending(c => c.BlockedAt)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            return new PagedResponse<BlockedCountry>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Data = data
            };
        }

        public bool AddTemporalBlock(TemporalBlock block)
        {
            if (_blockedCountries.ContainsKey(block.CountryCode) || _temporalBlocks.ContainsKey(block.CountryCode))
            {
                return false;
            }
            return _temporalBlocks.TryAdd(block.CountryCode, block);
        }

        public void RemoveExpiredTemporalBlocks()
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _temporalBlocks.Where(kvp => kvp.Value.ExpiresAt <= now).Select(kvp => kvp.Key).ToList();

            foreach (var key in expiredKeys)
            {
                _temporalBlocks.TryRemove(key, out _);
            }
        }

        public bool IsValidCountryCode(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                return false;

            countryCode = countryCode.Trim();

            return countryCode.Length == 2 &&
                   countryCode.All(char.IsLetter);
        }
    }
}