using AutoMapper;
using Foodiya.Application.DTOs.Difficulty.Request;
using Foodiya.Application.DTOs.Difficulty.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.Difficulties;

namespace Foodiya.Application.Services;

public sealed class DifficultyService : IDifficultyService
{
    private readonly IDifficultyRepository _difficultyRepo;
    private readonly IGenericRepository<Recipe> _recipeRepo;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDifficultyFactory _factory;

    public DifficultyService(
        IDifficultyRepository difficultyRepo,
        IGenericRepository<Recipe> recipeRepo,
        IMapper mapper,
        IDateTimeProvider dateTimeProvider,
        IDifficultyFactory factory)
    {
        _difficultyRepo = difficultyRepo;
        _recipeRepo = recipeRepo;
        _mapper = mapper;
        _dateTimeProvider = dateTimeProvider;
        _factory = factory;
    }

    public async Task<DifficultyDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var difficulty = await _difficultyRepo.GetByUuidAsync(uuid, ct);
        return difficulty is null ? null : _mapper.Map<DifficultyDetailResponse>(difficulty);
    }

    public async Task<PaginatedResponse<DifficultyDetailResponse>> ListAsync(
        int page,
        int pageSize,
        bool? isActive,
        string? search,
        CancellationToken ct = default)
    {
        var listSpec = new DifficultyListSpecification(page, pageSize, isActive, search);
        var countSpec = new DifficultyCountSpecification(isActive, search);

        var items = await _difficultyRepo.ListAsync(listSpec, ct);
        var totalCount = await _difficultyRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<DifficultyDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<DifficultyDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<DifficultyDetailResponse> CreateAsync(CreateDifficultyRequest request, CancellationToken ct = default)
    {
        var difficulty = _factory.Create(request);

        await _difficultyRepo.InsertAsync(difficulty, ct);
        await _difficultyRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(difficulty.Id, ct))!;
    }

    public async Task<DifficultyDetailResponse> UpdateAsync(Guid uuid, UpdateDifficultyRequest request, CancellationToken ct = default)
    {
        var difficulty = await _difficultyRepo.GetRequiredByUuidAsync(uuid, nameof(Difficulty), ct);

        _factory.Update(difficulty, request, _dateTimeProvider.UtcNow);

        _difficultyRepo.Update(difficulty);
        await _difficultyRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(difficulty.Id, ct))!;
    }

    public async Task<DifficultyDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default)
    {
        var difficulty = await _difficultyRepo.GetRequiredByUuidAsync(uuid, nameof(Difficulty), ct);

        difficulty.IsActive = !difficulty.IsActive;
        difficulty.DateModif = _dateTimeProvider.UtcNow;

        _difficultyRepo.Update(difficulty);
        await _difficultyRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(difficulty.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        var difficulty = await _difficultyRepo.GetRequiredByUuidAsync(uuid, nameof(Difficulty), ct);

        var hasRecipes = _recipeRepo.GetAll().Any(recipe => recipe.DifficultyId == difficulty.Id);
        if (hasRecipes)
            throw new FoodiyaBadRequestException("Cannot delete a Difficulty that is still used by recipes.");

        _difficultyRepo.Delete(difficulty);
        await _difficultyRepo.SaveAsync(ct);
    }

    private async Task<DifficultyDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var difficulty = await _difficultyRepo.GetSingleAsync(new DifficultyByIdSpecification(id), ct);
        return difficulty is null ? null : _mapper.Map<DifficultyDetailResponse>(difficulty);
    }
}
