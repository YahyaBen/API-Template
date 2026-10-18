namespace Foodiya.Application.DTOs.Recipe.Response;

public sealed class PaginatedResponse<T>
{
    public IReadOnlyCollection<T> Data { get; init; } = [];
    public PaginationMeta Meta { get; init; } = new();
}

public sealed class PaginationMeta
{
    public int Page { get; init; }
    public int Take { get; init; }
    public int ItemCount { get; init; }
    public int PageCount => Take > 0 ? (int)Math.Ceiling(ItemCount / (double)Take) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < PageCount;
}
