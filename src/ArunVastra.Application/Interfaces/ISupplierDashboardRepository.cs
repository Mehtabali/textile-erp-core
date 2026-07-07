using ArunVastra.Application.DTOs.SupplierDashboard;

namespace ArunVastra.Application.Interfaces;

public interface ISupplierDashboardRepository
{
    Task<SupplierDashboardResponse> GetAsync(int supplierUserId, CancellationToken cancellationToken = default);
}
