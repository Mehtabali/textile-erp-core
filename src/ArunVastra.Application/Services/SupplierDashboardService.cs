using ArunVastra.Application.DTOs.SupplierDashboard;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using ArunVastra.Domain.Enums;

namespace ArunVastra.Application.Services;

public sealed class SupplierDashboardService(ISupplierDashboardRepository supplierDashboardRepository) : ISupplierDashboardService
{
    private readonly ISupplierDashboardRepository _supplierDashboardRepository = supplierDashboardRepository;

    public Task<SupplierDashboardResponse> GetAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId <= 0)
        {
            throw new InvalidOperationException("Current user id is missing from token.");
        }

        if (currentUser.Role != (int)UserRole.Supplier)
        {
            throw new InvalidOperationException("Supplier dashboard is only available for supplier users.");
        }

        return _supplierDashboardRepository.GetAsync(currentUser.UserId, cancellationToken);
    }
}
