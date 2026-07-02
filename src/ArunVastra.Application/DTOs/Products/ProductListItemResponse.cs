namespace ArunVastra.Application.DTOs.Products;

public sealed class ProductListItemResponse
{
    public int Id { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Barcode { get; set; } = string.Empty;

    public string Hsn { get; set; } = string.Empty;

    public decimal? Purchase { get; set; }

    public decimal? Mrp { get; set; }

    public string Company { get; set; } = string.Empty;

    public string Formula { get; set; } = string.Empty;

    public decimal? SaleRate { get; set; }
}
