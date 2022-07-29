using Gateway.Data.Contexts;

namespace Gateway.Data.Infrastructures;

public interface IDbFactory
{
    MongoDbContext Init();
}

public class DbFactory: Disposable, IDbFactory
{
    private MongoDbContext? _dbContext;

    public MongoDbContext Init() => _dbContext ??= new MongoDbContext();
    
    protected override void DisposeCore()
    {
        _dbContext?.Dispose();
    }
}