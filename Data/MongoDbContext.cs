using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RentACarReport.Models;
using ReportedUsersSystem.Models;

namespace ReportedUsersSystem.Data;

// Settings class
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

// Database context
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    public IMongoCollection<ReportedUser> ReportedUsers => _database.GetCollection<ReportedUser>("ReportedUsers");
}
