using AutoMapper;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeLike.Request;
using Foodiya.Application.DTOs.RecipeLike.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.RecipeLikes;

namespace Foodiya.Application.Services;

public sealed class RecipeLikeService : IRecipeLikeService
{
    private readonly IRecipeLikeRepository _recipeLikeRepo;
    private readonly IGenericRepository<Recipe> _recipeRepo;
    private readonly IGenericRepository<AppUser> _userRepo;
    private readonly IMapper _mapper;
    private readonly IRecipeLikeFactory _factory;

    public RecipeLikeService(
        IRecipeLikeRepository recipeLikeRepo,
        IGenericRepository<Recipe> recipeRepo,
        IGenericRepository<AppUser> userRepo,
        IMapper mapper,
        IRecipeLikeFactory factory)
    {
        _recipeLikeRepo = recipeLikeRepo;
        _recipeRepo = recipeRepo;
        _userRepo = userRepo;
        _mapper = mapper;
        _factory = factory;
    }

    public async Task<RecipeLikeDetailResponse?> GetByIdAsync(Guid recipeUuid, Guid userUuid, CancellationToken ct = default)
    {
        var recipeId = (await _recipeRepo.GetRequiredByUuidAsync(recipeUuid, nameof(Recipe), ct)).Id;
        var userId = (await _userRepo.GetRequiredByUuidAsync(userUuid, nameof(AppUser), ct)).Id;
        var recipeLike = await _recipeLikeRepo.GetSingleAsync(new RecipeLikeByIdSpecification(recipeId, userId), ct);
        return recipeLike is null ? null : _mapper.Map<RecipeLikeDetailResponse>(recipeLike);
    }

    public async Task<PaginatedResponse<RecipeLikeDetailResponse>> ListAsync(
        int page,
        int pageSize,
        Guid? recipeUuid,
        Guid? userUuid,
        string? search,
        CancellationToken ct = default)
    {
        var recipeId = recipeUuid.HasValue ? (await _recipeRepo.GetRequiredByUuidAsync(recipeUuid.Value, nameof(Recipe), ct)).Id : (int?)null;
        var userId = userUuid.HasValue ? (await _userRepo.GetRequiredByUuidAsync(userUuid.Value, nameof(AppUser), ct)).Id : (int?)null;
        var listSpec = new RecipeLikeListSpecification(page, pageSize, recipeId, userId, search);
        var countSpec = new RecipeLikeCountSpecification(recipeId, userId, search);

        var items = await _recipeLikeRepo.ListAsync(listSpec, ct);
        var totalCount = await _recipeLikeRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<RecipeLikeDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<RecipeLikeDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<RecipeLikeDetailResponse> CreateAsync(CreateRecipeLikeRequest request, CancellationToken ct = default)
    {
        request.RecipeId = (await _recipeRepo.GetRequiredByUuidAsync(request.RecipeUuid, nameof(Recipe), ct)).Id;
        request.UserId = (await _userRepo.GetRequiredByUuidAsync(request.UserUuid, nameof(AppUser), ct)).Id;
        await EnsureRecipeExistsAsync(request.RecipeId, ct);
        await EnsureUserExistsAsync(request.UserId, ct);

        var existing = await _recipeLikeRepo.GetSingleAsync(new RecipeLikeByIdSpecification(request.RecipeId, request.UserId), ct);
        if (existing is not null)
            throw new FoodiyaValueAlreadyExistsException($"RecipeLike for Recipe ID {request.RecipeId} and User ID {request.UserId} already exists.");

        var recipeLike = _factory.Create(request);

        await _recipeLikeRepo.InsertAsync(recipeLike, ct);
        await _recipeLikeRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(recipeLike.RecipeId, recipeLike.UserId, ct))!;
    }

    public async Task DeleteAsync(Guid recipeUuid, Guid userUuid, CancellationToken ct = default)
    {
        var recipeId = (await _recipeRepo.GetRequiredByUuidAsync(recipeUuid, nameof(Recipe), ct)).Id;
        var userId = (await _userRepo.GetRequiredByUuidAsync(userUuid, nameof(AppUser), ct)).Id;
        var recipeLike = await _recipeLikeRepo.GetSingleAsync(new RecipeLikeByIdSpecification(recipeId, userId), ct)
            ?? throw new FoodiyaNotFoundException($"RecipeLike with Recipe ID {recipeId} and User ID {userId} not found.");

        _recipeLikeRepo.Delete(recipeLike);
        await _recipeLikeRepo.SaveAsync(ct);
    }

    private async Task<RecipeLikeDetailResponse?> GetByIdInternalAsync(int recipeId, int userId, CancellationToken ct)
    {
        var recipeLike = await _recipeLikeRepo.GetSingleAsync(new RecipeLikeByIdSpecification(recipeId, userId), ct);
        return recipeLike is null ? null : _mapper.Map<RecipeLikeDetailResponse>(recipeLike);
    }

    private async Task EnsureRecipeExistsAsync(int recipeId, CancellationToken ct)
    {
        var recipe = await _recipeRepo.GetByIdAsync(recipeId, ct: ct)
            ?? throw new FoodiyaNotFoundException($"Recipe with ID {recipeId} not found.");

        if (recipe.DeletedAt is not null)
            throw new FoodiyaBadRequestException($"Recipe with ID {recipeId} is deleted and cannot be liked.");
    }

    private async Task EnsureUserExistsAsync(int userId, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct: ct)
            ?? throw new FoodiyaNotFoundException($"AppUser with ID {userId} not found.");

        if (user.DeletedAt is not null)
            throw new FoodiyaBadRequestException($"AppUser with ID {userId} is deleted and cannot like recipes.");
    }
}
