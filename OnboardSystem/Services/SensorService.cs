using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OnboardSystem.Models;

namespace OnboardSystem.Services;
public class SensorService
{
    private readonly IMongoCollection<Sensor> _sensorsCollection;

    public SensorService(
        IOptions<OnboardDatabaseSettings> onboardDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            onboardDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            onboardDatabaseSettings.Value.DatabaseName);

        _sensorsCollection = mongoDatabase.GetCollection<Sensor>(
            onboardDatabaseSettings.Value.SensorsCollectionName);
    }

    public async Task<List<Sensor>> GetAllAsync() =>
            await _sensorsCollection.Find(_ => true).ToListAsync();

    public async Task CreateAsync(Sensor newBook) =>
        await _sensorsCollection.InsertOneAsync(newBook);
}
