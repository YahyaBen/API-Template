namespace Foodiya.Application.DTOs.MoroccanRegion.Response;

public sealed class MoroccanRegionDetailResponse
{
    public Guid Uuid { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime DateInsert { get; set; }
    public DateTime? DateModif { get; set; }
}
