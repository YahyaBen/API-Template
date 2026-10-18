namespace Foodiya.Application.DTOs.MoroccanCity.Response;

public sealed class MoroccanCityDetailResponse
{
    public Guid Uuid { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid RegionUuid { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime DateInsert { get; set; }
    public DateTime? DateModif { get; set; }
}
