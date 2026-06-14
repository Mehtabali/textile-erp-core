namespace ArunVastra.Application.DTOs.Categories;

public sealed class UpdateCategoryRequest
{
    public string? ProductName { get; set; }

    public decimal? Gst { get; set; }

    public string? HsnCode { get; set; }

    public decimal? Stitch { get; set; }

    public bool GstRule { get; set; }

    public bool AdditionalCharges { get; set; }
}
