using AutoMapper;
using Foodiya.Application.DTOs.IngredientType.Request;
using Foodiya.Application.DTOs.IngredientType.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.IngredientTypes;

namespace Foodiya.Application.Services;

public sealed class IngredientTypeService : IIngredientTypeService
{
    private readonly IIngredientTypeRepository _ingredientTypeRepo;
    private readonly IIngredientRepository _ingredientRepo;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IIngredientTypeFactory _factory;

    public IngredientTypeService(
        IIngredientTypeRepository ingredientTypeRepo,
        IIngredientRepository ingredientRepo,
        IMapper mapper,
        IDateTimeProvider dateTimeProvider,
        IIngredientTypeFactory factory)
    {
        _ingredientTypeRepo = ingredientTypeRepo;
        _ingredientRepo = ingredientRepo;
        _mapper = mapper;
        _dateTimeProvider = dateTimeProvider;
        _factory = factory;
    }

    public async Task<IngredientTypeDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var ingredientType = await _ingredientTypeRepo.GetByUuidAsync(uuid, ct);
        return ingredientType is null ? null : _mapper.Map<IngredientTypeDetailResponse>(ingredientType);
    }

    public async Task<PaginatedResponse<IngredientTypeDetailResponse>> ListAsync(
        int page,
        int pageSize,
        bool? isActive,
        string? search,
        CancellationToken ct = default)
    {
        var listSpec = new IngredientTypeListSpecification(page, pageSize, isActive, search);
        var countSpec = new IngredientTypeCountSpecification(isActive, search);

        var items = await _ingredientTypeRepo.ListAsync(listSpec, ct);
        var totalCount = await _ingredientTypeRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<IngredientTypeDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<IngredientTypeDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<IngredientTypeDetailResponse> CreateAsync(CreateIngredientTypeRequest request, CancellationToken ct = default)
    {
        var ingredientType = _factory.Create(request);

        await _ingredientTypeRepo.InsertAsync(ingredientType, ct);
        await _ingredientTypeRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(ingredientType.Id, ct))!;
    }

    public async Task<IngredientTypeDetailResponse> UpdateAsync(Guid uuid, UpdateIngredientTypeRequest request, CancellationToken ct = default)
    {
        var ingredientType = await _ingredientTypeRepo.GetRequiredByUuidAsync(uuid, nameof(IngredientType), ct);

        _factory.Update(ingredientType, request, _dateTimeProvider.UtcNow);

        _ingredientTypeRepo.Update(ingredientType);
        await _ingredientTypeRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(ingredientType.Id, ct))!;
    }

    public async Task<IngredientTypeDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default)
    {
        var ingredientType = await _ingredientTypeRepo.GetRequiredByUuidAsync(uuid, nameof(IngredientType), ct);

        ingredientType.IsActive = !ingredientType.IsActive;
        ingredientType.DateModif = _dateTimeProvider.UtcNow;

        _ingredientTypeRepo.Update(ingredientType);
        await _ingredientTypeRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(ingredientType.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        var ingredientType = await _ingredientTypeRepo.GetRequiredByUuidAsync(uuid, nameof(IngredientType), ct);

        var hasIngredients = _ingredientRepo.GetAll().Any(ingredient => ingredient.IngredientTypeId == ingredientType.Id);
        if (hasIngredients)
            throw new FoodiyaBadRequestException("Cannot delete an IngredientType that is still used by ingredients.");

        _ingredientTypeRepo.Delete(ingredientType);
        await _ingredientTypeRepo.SaveAsync(ct);
    }

    private async Task<IngredientTypeDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var ingredientType = await _ingredientTypeRepo.GetSingleAsync(new IngredientTypeByIdSpecification(id), ct);
        return ingredientType is null ? null : _mapper.Map<IngredientTypeDetailResponse>(ingredientType);
    }
}
