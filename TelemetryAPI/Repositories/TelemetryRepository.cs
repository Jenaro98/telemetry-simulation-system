using MongoDB.Driver;
using TelemetryAPI.Models;
using TelemetryAPI.Services;
using MongoDB.Bson;

namespace TelemetryAPI.Repositories;

public class TelemetryRepository : ITelemetryRepository
{
    private readonly IMongoCollection<TelemetryData> _collection;

    public TelemetryRepository(IMongoDbService mongoDbService)
    {
        _collection = mongoDbService.GetCollection();
    }

    public async Task<string> CreateAsync(TelemetryData telemetryData)
    {
        await _collection.InsertOneAsync(telemetryData);
        return telemetryData.Id!;
    }

    public async Task<TelemetryData?> GetLatestAsync(string deviceId)
    {
        return await _collection
            .Find(x => x.DeviceId == deviceId)
            .SortByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<List<TelemetryData>> GetAllLatestAsync()
    {
        // Get the latest record for each device
        var pipeline = new[]
        {
            new BsonDocument("$sort", new BsonDocument("Timestamp", -1)),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$DeviceId" },
                { "latest", new BsonDocument("$first", "$$ROOT") }
            }),
            new BsonDocument("$replaceRoot", new BsonDocument("newRoot", "$latest"))
        };

        var result = await _collection.Aggregate<TelemetryData>(pipeline).ToListAsync();
        return result;
    }
}
