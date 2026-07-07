namespace ArunVastra.Application.DTOs.SupplierDashboard;

public sealed class SupplierDashboardResponse
{
    public int Ready { get; set; }

    public int Deliver { get; set; }

    public int Enter { get; set; }

    public int Open { get; set; }

    public int ReturnCancel { get; set; }

    public int ActiveProducts { get; set; }

    public int DeactiveProducts { get; set; }
}
