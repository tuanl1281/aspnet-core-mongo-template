using AutoMapper;
using Gateway.Data.Infrastructures;
using Gateway.Data.Repositories.Common;
using Gateway.ViewModel.Common.Exception;
using Gateway.ViewModel.Common.Response;

namespace Gateway.Service.Core.Common;

/*
    Generic
    - TE: Entity
    - TF: Filter model
    - TV: View model
    - TA: Add model
    - TU: Update model
*/

public interface IBaseService<TE, TF, TV, TA, TU> where TE : class where TF : class where TV : class where TA : class where TU : class
{
    Task<object> Add(TA model);

    Task<object> Update(TU model, Guid id);

    Task<object> Delete(Guid id);

    Task<TV> Get(Guid id);

    Task<PagingResponseModel<TV>> GetPagedResult(TF filter, Guid userId);
}

public class BaseService<TE, TF, TV, TA, TU>: IBaseService<TE, TF, TV, TA, TU> where TE : class where TF : class where TV : class where TA : class where TU : class
{
    protected readonly IMapper _mapper;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IBaseRepository<TE> _repository;

    protected BaseService(IMapper mapper, IUnitOfWork unitOfWork, IBaseRepository<TE> repository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    public virtual async Task<object> Add(TA model)
    {
        /* Builder */
        var entity = _mapper.Map<TA, TE>(model);
        /* Save */
        await _repository.AddAsync(entity);
        /* Return */
        return null;
    }

    public virtual async Task<object> Update(TU model, Guid id)
    {
        /* Validate */
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException();
        /* Builder */
        entity = _mapper.Map<TU, TE>(model, entity);
        /* Save */
        await _repository.UpdateAsync(entity, id);
        /* Return */
        return id;
    }

    public virtual async Task<object> Delete(Guid id)
    {
        /* Validate */
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException();
        /* Save */
        await _repository.DeleteAsync(id);
        /* Return */
        return id;
    }

    public virtual async Task<TV> Get(Guid id)
    {
        /* Validate */
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException();
        /* Return */
        return _mapper.Map<TE, TV>(entity);
    }

    public virtual async Task<PagingResponseModel<TV>> GetPagedResult(TF filter, Guid userId)
    {
        var result = new PagingResponseModel<TV>();
        /* Query */
        var entities = await _repository.GetAllAsync();
        /* Builder */
        var entityModels = _mapper.Map<List<TE>, List<TV>>(entities);
        /* Return */
        result.TotalCounts = entities.Count;
        result.Data = entityModels;
        return result;
    }
}