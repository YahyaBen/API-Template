using Foodiya.API.Controllers.Common;
using Foodiya.Application.DTOs.DailyRecipeStat.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class DailyRecipeStatController : BaseController
{
    private readonly IDailyRecipeStatService _dailyRecipeStatService;

    public DailyRecipeStatController(IDailyRecipeStatService dailyRecipeStatService)
    {
        _dailyRecipeStatService = dailyRecipeStatService ?? throw new FoodiyaNullArgumentException(nameof(dailyRecipeStatService));
    }

    /// <summary>
    /// Get a daily recipe stat by UUID
    /// </summary>
    /// <remarks>
    /// Returns the daily recipe statistics for a given UUID.
    /// </remarks>
    /// <param name="uuid">Daily recipe stat UUID</param>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(typeof(DailyRecipeStatDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DailyRecipeStatDetailResponse>> GetById(Guid uuid, CancellationToken ct)
    {
        var stat = await _dailyRecipeStatService.GetByIdAsync(uuid, ct);
        return stat is null ? NotFound() : Ok(stat);
    }


    /// <summary>
    /// Get all daily recipe stats
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of daily recipe statistics, ordered by date descending.
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<DailyRecipeStatDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<DailyRecipeStatDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        var result = await _dailyRecipeStatService.ListAsync(page, pageSize, ct);
        return Ok(result);
    }


    /// <summary>
    /// Get today's daily recipe stats
    /// </summary>
    /// <remarks>
    /// Returns the daily recipe statistics for today (UTC). Returns 404 if stats have not been generated yet.
    /// </remarks>
    [HttpGet("today")]
    [ProducesResponseType(typeof(DailyRecipeStatDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DailyRecipeStatDetailResponse>> GetToday(CancellationToken ct)
    {
        var stat = await _dailyRecipeStatService.GetTodayAsync(ct);
        return stat is null ? NotFound() : Ok(stat);
    }


    /// <summary>
    /// Get the latest daily recipe stats
    /// </summary>
    /// <remarks>
    /// Returns the most recent daily recipe statistics entry. Returns 404 if no stats exist.
    /// </remarks>
    [HttpGet("last")]
    [ProducesResponseType(typeof(DailyRecipeStatDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DailyRecipeStatDetailResponse>> GetLast(CancellationToken ct)
    {
        var stat = await _dailyRecipeStatService.GetLastAsync(ct);
        return stat is null ? NotFound() : Ok(stat);
    }

}
