using MongoDB.Driver;

namespace TelemetryAPI.Services;

public interface IMongoDbService
{
    IMongoCollection<Models.TelemetryData> GetCollection();
}
