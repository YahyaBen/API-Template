namespace Foodiya.Domain.Models;

public sealed class ReferenceDataItem
{
    public Guid Uuid { get; init; }

    public string Label { get; init; } = string.Empty;

    public string? Code { get; init; }

    public string? Description { get; init; }

    public int? SortOrder { get; init; }

    public Guid? ParentUuid { get; init; }

    public string? Slug { get; init; }

    public string? ImageUrl { get; init; }

    public string? BackgroundColor { get; init; }

    public bool? IsActive { get; init; }
}
