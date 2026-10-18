namespace Foodiya.Application.DTOs.ReferenceData.Response;

public sealed class ReferenceDataItemResponse
{
    public Guid Uuid { get; set; }

    public string Label { get; set; } = string.Empty;

    public string? Code { get; set; }

    public string? Description { get; set; }

    public int? SortOrder { get; set; }

    public Guid? ParentUuid { get; set; }

    public string? Slug { get; set; }

    public string? ImageUrl { get; set; }

    public string? BackgroundColor { get; set; }

    public bool? IsActive { get; set; }
}
