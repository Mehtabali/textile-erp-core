namespace ArunVastra.Application.DTOs.SaleVouchers;

public sealed class VoucherStatusHistoryResponse
{
    public int SaleVoucherId { get; set; }

    public DateTime Date { get; set; }

    public int Status { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? Remarks { get; set; }
}
