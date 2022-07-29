using System.Collections;
using Gateway.Data.Contexts;
using Gateway.Data.Repositories.Common;

namespace Gateway.Data.Infrastructures;

public interface IUnitOfWork: IDisposable
{
    MongoDbContext DbContext { get; }

    IBaseRepository<TEntity> Repository<TEntity>() where TEntity : class;
}

public class UnitOfWork: Disposable, IUnitOfWork
{
    private readonly IDbFactory _dbFactory;
    private readonly MongoDbContext _dbContext;
    
    private readonly Hashtable _repositories;
    
    public UnitOfWork(IDbFactory dbFactory)
    {
        _dbFactory = dbFactory;
        _dbContext = _dbContext ?? _dbFactory.Init();
        _repositories = new Hashtable();
    }

    public MongoDbContext DbContext => _dbContext;

    public IBaseRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity).Name;
        /* Get instance */
        if (_repositories.ContainsKey(type))
            return (IBaseRepository<TEntity>) _repositories[type];
        /* Add instance */
        var repositoryType = typeof(BaseRepository<TEntity>);
        var repositoryInstance =
            Activator.CreateInstance(repositoryType
                .MakeGenericType(typeof (TEntity)), _dbFactory);

        _repositories.Add(type, repositoryInstance);
        /* Return */
        return (IBaseRepository<TEntity>) _repositories[type];
    }
}