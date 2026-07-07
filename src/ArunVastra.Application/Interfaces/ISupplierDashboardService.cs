using ArunVastra.Application.DTOs.SupplierDashboard;
using ArunVastra.Application.Models;

namespace ArunVastra.Application.Interfaces;

public interface ISupplierDashboardService
{
    Task<SupplierDashboardResponse> GetAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);
}
