using AutoMapper;
using Foodiya.Application.DTOs.PresetAvatarImage.Request;
using Foodiya.Application.DTOs.PresetAvatarImage.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.PresetAvatarImages;

namespace Foodiya.Application.Services;

public sealed class PresetAvatarImageService : IPresetAvatarImageService
{
    private readonly IPresetAvatarImageRepository _presetAvatarImageRepo;
    private readonly IGenericRepository<AppUser> _appUserRepo;
    private readonly IMapper _mapper;
    private readonly IPresetAvatarImageFactory _factory;

    public PresetAvatarImageService(
        IPresetAvatarImageRepository presetAvatarImageRepo,
        IGenericRepository<AppUser> appUserRepo,
        IMapper mapper,
        IPresetAvatarImageFactory factory)
    {
        _presetAvatarImageRepo = presetAvatarImageRepo;
        _appUserRepo = appUserRepo;
        _mapper = mapper;
        _factory = factory;
    }

    public async Task<PresetAvatarImageDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var presetAvatarImage = await _presetAvatarImageRepo.GetByUuidAsync(uuid, ct);
        return presetAvatarImage is null ? null : _mapper.Map<PresetAvatarImageDetailResponse>(presetAvatarImage);
    }

    public async Task<PaginatedResponse<PresetAvatarImageDetailResponse>> ListAsync(
        int page,
        int pageSize,
        bool? isActive,
        string? search,
        CancellationToken ct = default)
    {
        var listSpec = new PresetAvatarImageListSpecification(page, pageSize, isActive, search);
        var countSpec = new PresetAvatarImageCountSpecification(isActive, search);

        var items = await _presetAvatarImageRepo.ListAsync(listSpec, ct);
        var totalCount = await _presetAvatarImageRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<PresetAvatarImageDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<PresetAvatarImageDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<PresetAvatarImageDetailResponse> CreateAsync(CreatePresetAvatarImageRequest request, CancellationToken ct = default)
    {
        var presetAvatarImage = _factory.Create(request);

        await _presetAvatarImageRepo.InsertAsync(presetAvatarImage, ct);
        await _presetAvatarImageRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(presetAvatarImage.Id, ct))!;
    }

    public async Task<PresetAvatarImageDetailResponse> UpdateAsync(Guid uuid, UpdatePresetAvatarImageRequest request, CancellationToken ct = default)
    {
        var presetAvatarImage = await _presetAvatarImageRepo.GetRequiredByUuidAsync(uuid, nameof(PresetAvatarImage), ct);

        _factory.Update(presetAvatarImage, request);

        _presetAvatarImageRepo.Update(presetAvatarImage);
        await _presetAvatarImageRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(presetAvatarImage.Id, ct))!;
    }

    public async Task<PresetAvatarImageDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default)
    {
        var presetAvatarImage = await _presetAvatarImageRepo.GetRequiredByUuidAsync(uuid, nameof(PresetAvatarImage), ct);

        presetAvatarImage.IsActive = !presetAvatarImage.IsActive;

        _presetAvatarImageRepo.Update(presetAvatarImage);
        await _presetAvatarImageRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(presetAvatarImage.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        var presetAvatarImage = await _presetAvatarImageRepo.GetRequiredByUuidAsync(uuid, nameof(PresetAvatarImage), ct);

        var isAssigned = _appUserRepo.GetAll().Any(user => user.PresetAvatarImageId == presetAvatarImage.Id);
        if (isAssigned)
            throw new FoodiyaBadRequestException("Cannot delete a PresetAvatarImage that is still assigned to users.");

        _presetAvatarImageRepo.Delete(presetAvatarImage);
        await _presetAvatarImageRepo.SaveAsync(ct);
    }

    private async Task<PresetAvatarImageDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var presetAvatarImage = await _presetAvatarImageRepo.GetSingleAsync(new PresetAvatarImageByIdSpecification(id), ct);
        return presetAvatarImage is null ? null : _mapper.Map<PresetAvatarImageDetailResponse>(presetAvatarImage);
    }
}
