using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RentACarReport.Models;
using ReportedUsersSystem.Models;

namespace ReportedUsersSystem.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var mongoClientSettings = MongoClientSettings.FromConnectionString(settings.Value.ConnectionString);
        
        // Configure SSL/TLS settings
        
        mongoClientSettings.SslSettings = new SslSettings
        {
            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
        };
        
        // Disable server certificate validation (for Railway/Docker environments)
        mongoClientSettings.ServerApi = new ServerApi(ServerApiVersion.V1);
        
        var client = new MongoClient(mongoClientSettings);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    public IMongoCollection<ReportedUser> ReportedUsers => _database.GetCollection<ReportedUser>("ReportedUsers");
}

public class MongoDbSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
}