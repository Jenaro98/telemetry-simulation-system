using TelemetryAPI.Models;

namespace TelemetryAPI.Repositories;

public interface ITelemetryRepository
{
    Task<string> CreateAsync(TelemetryData telemetryData);
    Task<TelemetryData?> GetLatestAsync(string deviceId);
    Task<List<TelemetryData>> GetAllLatestAsync();
}
