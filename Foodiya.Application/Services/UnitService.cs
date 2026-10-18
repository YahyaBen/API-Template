using AutoMapper;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.Unit.Request;
using Foodiya.Application.DTOs.Unit.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.Units;

namespace Foodiya.Application.Services;

public sealed class UnitService : IUnitService
{
    private readonly IUnitRepository _unitRepo;
    private readonly IGenericRepository<Ingredient> _ingredientRepo;
    private readonly IGenericRepository<RecipeIngredient> _recipeIngredientRepo;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitFactory _factory;

    public UnitService(
        IUnitRepository unitRepo,
        IGenericRepository<Ingredient> ingredientRepo,
        IGenericRepository<RecipeIngredient> recipeIngredientRepo,
        IMapper mapper,
        IDateTimeProvider dateTimeProvider,
        IUnitFactory factory)
    {
        _unitRepo = unitRepo;
        _ingredientRepo = ingredientRepo;
        _recipeIngredientRepo = recipeIngredientRepo;
        _mapper = mapper;
        _dateTimeProvider = dateTimeProvider;
        _factory = factory;
    }

    public async Task<UnitDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var unit = await _unitRepo.GetByUuidAsync(uuid, ct);
        return unit is null ? null : _mapper.Map<UnitDetailResponse>(unit);
    }

    public async Task<PaginatedResponse<UnitDetailResponse>> ListAsync(
        int page,
        int pageSize,
        bool? isActive,
        string? search,
        CancellationToken ct = default)
    {
        var listSpec = new UnitListSpecification(page, pageSize, isActive, search);
        var countSpec = new UnitCountSpecification(isActive, search);

        var items = await _unitRepo.ListAsync(listSpec, ct);
        var totalCount = await _unitRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<UnitDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<UnitDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<UnitDetailResponse> CreateAsync(CreateUnitRequest request, CancellationToken ct = default)
    {
        var unit = _factory.Create(request);

        await _unitRepo.InsertAsync(unit, ct);
        await _unitRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(unit.Id, ct))!;
    }

    public async Task<UnitDetailResponse> UpdateAsync(Guid uuid, UpdateUnitRequest request, CancellationToken ct = default)
    {
        var unit = await _unitRepo.GetRequiredByUuidAsync(uuid, nameof(Unit), ct);

        _factory.Update(unit, request, _dateTimeProvider.UtcNow);

        _unitRepo.Update(unit);
        await _unitRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(unit.Id, ct))!;
    }

    public async Task<UnitDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default)
    {
        var unit = await _unitRepo.GetRequiredByUuidAsync(uuid, nameof(Unit), ct);

        unit.IsActive = !unit.IsActive;
        unit.DateModif = _dateTimeProvider.UtcNow;

        _unitRepo.Update(unit);
        await _unitRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(unit.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        var unit = await _unitRepo.GetRequiredByUuidAsync(uuid, nameof(Unit), ct);

        var usedByIngredients = _ingredientRepo.GetAll().Any(ingredient => ingredient.DefaultUnitId == unit.Id);
        if (usedByIngredients)
            throw new FoodiyaBadRequestException("Cannot delete a Unit that is still used by ingredients.");

        var usedByRecipeIngredients = _recipeIngredientRepo.GetAll().Any(recipeIngredient => recipeIngredient.UnitId == unit.Id);
        if (usedByRecipeIngredients)
            throw new FoodiyaBadRequestException("Cannot delete a Unit that is still used by recipe ingredients.");

        _unitRepo.Delete(unit);
        await _unitRepo.SaveAsync(ct);
    }

    private async Task<UnitDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var unit = await _unitRepo.GetSingleAsync(new UnitByIdSpecification(id), ct);
        return unit is null ? null : _mapper.Map<UnitDetailResponse>(unit);
    }
}
