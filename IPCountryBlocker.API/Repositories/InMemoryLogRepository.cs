using System.Collections.Concurrent;
using IPCountryBlocker.API.Interfaces;
using IPCountryBlocker.API.Models;

namespace IPCountryBlocker.API.Repositories
{
    public class InMemoryLogRepository : ILogRepository
    {
        private readonly ConcurrentQueue<BlockedAttemptLog> _logs = new();

        public void AddLog(BlockedAttemptLog log)
        {
            _logs.Enqueue(log);
            // Optional: Limit queue size to prevent memory leaks in production
            if (_logs.Count > 10000) _logs.TryDequeue(out _);
        }

        public PagedResponse<BlockedAttemptLog> GetLogs(int page, int pageSize)
        {
            var query = _logs.OrderByDescending(l => l.Timestamp);
            var total = query.Count();
            var data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResponse<BlockedAttemptLog>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Data = data
            };
        }
    }
}