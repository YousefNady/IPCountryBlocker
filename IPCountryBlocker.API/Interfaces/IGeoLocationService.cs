using IPCountryBlocker.API.Models;

namespace IPCountryBlocker.API.Interfaces
{
    public interface IGeoLocationService
    {
        Task<IpLookupResponse?> LookupIpAsync(string ipAddress);
    }
}