using AutoMapper;
using Foodiya.Application.DTOs.ChefProfile.Request;
using Foodiya.Application.DTOs.ChefProfile.Response;
using Foodiya.Application.DTOs.Common;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.ChefProfiles;

namespace Foodiya.Application.Services;

public sealed class ChefProfileService : IChefProfileService
{
    private readonly IGenericRepository<ChefProfile> _chefProfileRepo;
    private readonly IGenericRepository<AppUser> _userRepo;
    private readonly IGenericRepository<Recipe> _recipeRepo;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IChefProfileFactory _factory;

    public ChefProfileService(
        IGenericRepository<ChefProfile> chefProfileRepo,
        IGenericRepository<AppUser> userRepo,
        IGenericRepository<Recipe> recipeRepo,
        IMapper mapper,
        IDateTimeProvider dateTimeProvider,
        IChefProfileFactory factory)
    {
        _chefProfileRepo = chefProfileRepo;
        _userRepo = userRepo;
        _recipeRepo = recipeRepo;
        _mapper = mapper;
        _dateTimeProvider = dateTimeProvider;
        _factory = factory;
    }

    public async Task<ChefProfileDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var chefProfile = await _chefProfileRepo.GetByUuidAsync(uuid, ct);
        return chefProfile is null ? null : await GetByIdInternalAsync(chefProfile.Id, ct);
    }

    public async Task<PaginatedResponse<ChefProfileDetailResponse>> ListAsync(
        int page,
        int pageSize,
        bool? isVerified,
        string? search,
        CancellationToken ct = default)
    {
        var listSpec = new ChefProfileListSpecification(page, pageSize, isVerified, search);
        var countSpec = new ChefProfileCountSpecification(isVerified, search);

        var items = await _chefProfileRepo.ListAsync(listSpec, ct);
        var totalCount = await _chefProfileRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<ChefProfileDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<ChefProfileDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<ChefProfileDetailResponse> CreateAsync(Guid userUuid, CreateChefProfileRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.GetRequiredByUuidAsync(userUuid, nameof(AppUser), ct);

        var existingProfile = await _chefProfileRepo.GetSingleAsync(new ChefProfileByUserIdSpecification(user.Id), ct);
        if (existingProfile is not null)
            throw new FoodiyaValueAlreadyExistsException($"ChefProfile for AppUser UUID {userUuid} already exists.");

        var chefProfile = _factory.Create(user.Id, request);

        await _chefProfileRepo.InsertAsync(chefProfile, ct);
        await _chefProfileRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(chefProfile.Id, ct))!;
    }

    public async Task<ChefProfileDetailResponse> CreateSelfAsync(Guid identityUserId, CreateChefProfileRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.GetRequiredByUuidAsync(identityUserId, nameof(AppUser), ct);

        if (user.DeletedAt is not null)
            throw new FoodiyaBadRequestException("Your account has been deleted and cannot create a chef profile.");

        var existingProfile = await _chefProfileRepo.GetSingleAsync(new ChefProfileByUserIdSpecification(user.Id), ct);
        if (existingProfile is not null)
            throw new FoodiyaValueAlreadyExistsException($"A chef profile for your account already exists.");

        var chefProfile = _factory.Create(user.Id, request);

        await _chefProfileRepo.InsertAsync(chefProfile, ct);
        await _chefProfileRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(chefProfile.Id, ct))!;
    }

    public async Task<ChefProfileDetailResponse> UpdateAsync(Guid uuid, UpdateChefProfileRequest request, CancellationToken ct = default)
    {
        var chefProfile = await _chefProfileRepo.GetRequiredByUuidAsync(uuid, nameof(ChefProfile), ct);

        _factory.Update(chefProfile, request);

        _chefProfileRepo.Update(chefProfile);
        await _chefProfileRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(chefProfile.Id, ct))!;
    }

    public async Task<ChefProfileDetailResponse> ToggleVerifiedAsync(Guid uuid, CancellationToken ct = default)
    {
        var chefProfile = await _chefProfileRepo.GetRequiredByUuidAsync(uuid, nameof(ChefProfile), ct);

        chefProfile.IsVerified = !chefProfile.IsVerified;

        _chefProfileRepo.Update(chefProfile);
        await _chefProfileRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(chefProfile.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, SoftDeleteRequest request, CancellationToken ct = default)
    {
        var chefProfile = await _chefProfileRepo.GetRequiredByUuidAsync(uuid, nameof(ChefProfile), ct);

        if (chefProfile.DeletedAt is not null)
            throw new FoodiyaBadRequestException($"ChefProfile with UUID {uuid} is already deleted.");

        var hasActiveRecipes = _recipeRepo.GetAll().Any(r => r.ChefId == chefProfile.Id && r.DeletedAt == null);
        if (hasActiveRecipes)
            throw new FoodiyaBadRequestException("Cannot delete a ChefProfile that still owns active recipes.");

        request.DeletedByUserId = (await _userRepo.GetRequiredByUuidAsync(request.DeletedByUserUuid, nameof(AppUser), ct)).Id;
        chefProfile.DeletedAt = _dateTimeProvider.UtcNow;
        chefProfile.DeletedByUserId = request.DeletedByUserId;
        chefProfile.DeleteReason = request.DeleteReason;
        chefProfile.DateModif = _dateTimeProvider.UtcNow;

        _chefProfileRepo.Update(chefProfile);
        await _chefProfileRepo.SaveAsync(ct);
    }

    private async Task<ChefProfileDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var chefProfile = await _chefProfileRepo.GetSingleAsync(new ChefProfileByIdSpecification(id), ct);
        return chefProfile is null ? null : _mapper.Map<ChefProfileDetailResponse>(chefProfile);
    }
}
