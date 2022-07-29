using System.Linq.Expressions;
using MongoDB.Driver;
using Gateway.Data.Contexts;
using Gateway.Data.Infrastructures;
using Gateway.Data.Collections.Common;
using Gateway.ViewModel.Common.Attributes;

namespace Gateway.Data.Repositories.Common;

public interface IBaseRepository<TEntity> where TEntity : class
{
    #region --- Sync Methods ---
    /// <summary>
    /// Get all entities
    /// </summary>
    /// <returns></returns>
    IEnumerable<TEntity> GetAll();

    /// <summary>
    /// Get entities by lambda expression
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    IEnumerable<TEntity> GetMany(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Get entity by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    TEntity? GetById(object id);

    /// <summary>
    /// Get entity by lambda expression
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    TEntity? Get(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Add new entity
    /// </summary>
    /// <param name="entity"></param>
    void Add(TEntity entity);

    /// <summary>
    /// Update an entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    void Update(TEntity entity, object id);

    /// <summary>
    /// Delete an entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    void Delete(TEntity entity, object id);

    /// <summary>
    /// Delete an entity by id
    /// </summary>
    /// <param name="id"></param>
    void Delete(object id);
    
    /// <summary>
    /// Delete by expression
    /// </summary>
    /// <param name="predicate"></param>
    void Delete(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Delete the entities
    /// </summary>
    /// <param name="entities"></param>
    void DeleteRange(IEnumerable<TEntity> entities);
    #endregion
    
    #region --- Async Methods ---
    /// <summary>
    /// Get all entities
    /// </summary>
    /// <returns></returns>
    Task<List<TEntity>> GetAllAsync();

    /// <summary>
    /// Get entities by lambda expression
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<List<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Get entity by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<TEntity> GetByIdAsync(object id);

    /// <summary>
    /// Get entity by lambda expression
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Add new entity
    /// </summary>
    /// <param name="entity"></param>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// Update an entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    Task UpdateAsync(TEntity entity, object id);

    /// <summary>
    /// Delete an entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    Task DeleteAsync(TEntity entity, object id);

    /// <summary>
    /// Delete an entity by id
    /// </summary>
    /// <param name="id"></param>
    Task DeleteAsync(object id);
    
    /// <summary>
    /// Delete by expression
    /// </summary>
    /// <param name="predicate"></param>
    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Delete the entities
    /// </summary>
    /// <param name="entities"></param>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities);
    #endregion
}

public abstract class BaseRepository<TEntity> where TEntity : class
{
    #region Properties
    private readonly IDbFactory _dbFactory;
    private readonly MongoDbContext _dbContext;
    private readonly IMongoCollection<TEntity> _dbSet;

    protected IDbFactory DbFactory => _dbFactory;
    protected MongoDbContext DbContext => _dbContext;
    #endregion
    
    protected BaseRepository(IDbFactory dbFactory)
    {
        _dbFactory = dbFactory;
        _dbContext ??= _dbFactory.Init();
        _dbSet = _dbContext.Database.GetCollection<TEntity>(GetCollectionName(typeof(TEntity)));
    }

    private string GetCollectionName(Type documentType)
    {
        return ((BsonCollectionAttribute) documentType.GetCustomAttributes(typeof(BsonCollectionAttribute), true).FirstOrDefault())?.CollectionName;
    }
    
    private FilterDefinition<TEntity> FilterId(object id)
    {
        return Builders<TEntity>.Filter.Eq("Id", id);
    }
    
    /// <summary>
    /// Get entity by lambda expression
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public virtual TEntity? Get(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.Find(predicate).FirstOrDefault();
    }

    /// <summary>
    /// Get entity by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual TEntity? GetById(object id)
    {
        return _dbSet.Find(FilterId(id)).FirstOrDefault();
    }

    /// <summary>
    /// Get entities by lambda expression
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public virtual IEnumerable<TEntity> GetMany(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.Find(predicate).ToEnumerable();
    }
    
    /// <summary>
    /// Get all entities
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TEntity> GetAll()
    {
        return _dbSet.Find(Builders<TEntity>.Filter.Empty).ToEnumerable();
    }
    
    /// <summary>
    /// Add new entity
    /// </summary>
    /// <param name="entity"></param>
    public virtual void Add(TEntity entity)
    {
        if (typeof(BaseCollection).IsAssignableFrom(typeof(TEntity)))
            (entity as BaseCollection)!.DateCreated = DateTime.Now;
        
        _dbSet.InsertOne(entity);
    }

    /// <summary>
    /// Update an entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    public virtual void Update(TEntity entity, object id)
    {
        if (typeof(BaseCollection).IsAssignableFrom(typeof(TEntity)))
            (entity as BaseCollection)!.DateUpdated = DateTime.Now;
        
        _dbSet.FindOneAndReplace(FilterId(id), entity);
    }
    
    /// <summary>
    /// Delete an entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    public virtual void Delete(TEntity entity, object id)
    {
        var existing = _dbSet.Find(FilterId(id)).FirstOrDefault();
        if (existing == null)
            return;
        
        if (typeof(BaseCollection).IsAssignableFrom(typeof(TEntity)))
        {
            (existing as BaseCollection)!.IsDeleted = true;
            (existing as BaseCollection)!.DateUpdated = DateTime.Now;
            
            _dbSet.FindOneAndReplace(FilterId(id), entity);
        }
        else
            _dbSet.FindOneAndDelete(FilterId(id));
    }
    
    /// <summary>
    /// Delete by expression
    /// </summary>
    /// <param name="predicate"></param>
    public virtual void Delete(Expression<Func<TEntity, bool>> predicate)
    {
        IEnumerable<TEntity> entities = _dbSet.Find(predicate).ToList();
        foreach (TEntity entity in entities)
        {
            if (!typeof(BaseCollection).IsAssignableFrom(typeof(TEntity)))
                continue;
            
            (entity as BaseCollection)!.IsDeleted = true;
            (entity as BaseCollection)!.DateUpdated = DateTime.Now;
        
            _dbSet.FindOneAndReplace(FilterId((entity as BaseCollection)!.Id), entity);
        }
    }
    
    /// <summary>
    /// Delete entities
    /// </summary>
    /// <param name="entities"></param>
    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        foreach (TEntity entity in entities)
        {
            if (!typeof(BaseCollection).IsAssignableFrom(typeof(TEntity)))
                continue;
            
            (entity as BaseCollection)!.IsDeleted = true;
            (entity as BaseCollection)!.DateUpdated = DateTime.Now;
        
            _dbSet.FindOneAndReplace(FilterId((entity as BaseCollection)!.Id), entity);
        }
    }

    /// <summary>
    /// Get entity by lambda expression
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public virtual Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.Find(predicate).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get entity by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual Task<TEntity> GetByIdAsync(object id)
    {
        return _dbSet.Find(FilterId(id)).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get entities by lambda expression
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public virtual Task<List<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.Find(predicate).ToListAsync();
    }
    
    /// <summary>
    /// Get all entities
    /// </summary>
    /// <returns></returns>
    public Task<List<TEntity>> GetAllAsync()
    {
        return _dbSet.Find(Builders<TEntity>.Filter.Empty).ToListAsync();
    }
    
    /// <summary>
    /// Add new entity
    /// </summary>
    /// <param name="entity"></param>
    public virtual Task AddAsync(TEntity entity)
    {
        if (typeof(BaseCollection).IsAssignableFrom(typeof(TEntity)))
            (entity as BaseCollection)!.DateCreated = DateTime.Now;
        
        return _dbSet.InsertOneAsync(entity);
    }

    /// <summary>
    /// Update an entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    public virtual Task UpdateAsync(TEntity entity, object id)
    {
        if (typeof(BaseCollection).IsAssignableFrom(typeof(TEntity)))
            (entity as BaseCollection)!.DateUpdated = DateTime.Now;
        
        return _dbSet.FindOneAndReplaceAsync(FilterId(id), entity);
    }

    /// <summary>
    /// Delete an entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    public virtual async Task DeleteAsync(TEntity entity, object id)
    {
        var existing = await _dbSet.Find(FilterId(id)).FirstOrDefaultAsync();
        if (existing == null)
            return;
        
        if (typeof(BaseCollection).IsAssignableFrom(typeof(TEntity)))
        {
            (existing as BaseCollection)!.IsDeleted = true;
            (existing as BaseCollection)!.DateUpdated = DateTime.Now;
            
            await _dbSet.FindOneAndReplaceAsync(FilterId(id), entity);
            return;
        }
        
        await _dbSet.FindOneAndDeleteAsync(FilterId(id));
    }
    
    /// <summary>
    /// Delete by expression
    /// </summary>
    /// <param name="predicate"></param>
    public virtual async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
    {
        IEnumerable<TEntity> entities = await _dbSet.Find(predicate).ToListAsync();
        foreach (TEntity entity in entities)
        {
            if (!typeof(BaseCollection).IsAssignableFrom(typeof(TEntity)))
                continue;
            
            (entity as BaseCollection)!.IsDeleted = true;
            (entity as BaseCollection)!.DateUpdated = DateTime.Now;
        
            await _dbSet.FindOneAndReplaceAsync(FilterId((entity as BaseCollection)!.Id), entity);
        }
    }
    
    /// <summary>
    /// Delete entities
    /// </summary>
    /// <param name="entities"></param>
    public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        foreach (TEntity entity in entities)
        {
            if (!typeof(BaseCollection).IsAssignableFrom(typeof(TEntity)))
                continue;
            
            (entity as BaseCollection)!.IsDeleted = true;
            (entity as BaseCollection)!.DateUpdated = DateTime.Now;
        
            await _dbSet.FindOneAndReplaceAsync(FilterId((entity as BaseCollection)!.Id), entity);
        }
    }
}
