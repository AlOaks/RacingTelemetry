using MongoDB.Driver;
using RacingTelemetry.Models;

namespace RacingTelemetry.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"];
        var databaseName = configuration["MongoDB:Database"];
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }
    public IMongoCollection<TelemetryPoint> TelemetryPoints => _database.GetCollection<TelemetryPoint>("telemetrypoints");
}