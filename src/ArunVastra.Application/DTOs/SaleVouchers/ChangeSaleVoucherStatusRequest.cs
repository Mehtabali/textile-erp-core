namespace ArunVastra.Application.DTOs.SaleVouchers;

public sealed class ChangeSaleVoucherStatusRequest
{
    public int Status { get; set; }

    public string? Remarks { get; set; }
}
