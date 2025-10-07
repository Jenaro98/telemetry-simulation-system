using MongoDB.Driver;
using TelemetryAPI.Configuration;
using TelemetryAPI.Models;
using Microsoft.Extensions.Options;

namespace TelemetryAPI.Services;

public class MongoDbService : IMongoDbService
{
    private readonly IMongoDatabase _database;

    public MongoDbService(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<TelemetryData> GetCollection()
    {
        return _database.GetCollection<TelemetryData>("telemetry");
    }
}
