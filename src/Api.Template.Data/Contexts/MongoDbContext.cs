using MongoDB.Driver;
using Gateway.Data.Infrastructures;
using Gateway.ViewModel.Common.Setting;

namespace Gateway.Data.Contexts;

public interface IMongoDbContext: IDisposable
{
    IMongoDatabase Database { get; }
}

public class MongoDbContext: Disposable, IMongoDbContext
{
    public IMongoDatabase Database { get; }

    public MongoDbContext() { }

    public MongoDbContext(MongoDbSetting connectionSetting)
    {
        var client = new MongoClient(connectionSetting.ConnectionString);
        Database = client.GetDatabase(connectionSetting.DatabaseName);
    }
}