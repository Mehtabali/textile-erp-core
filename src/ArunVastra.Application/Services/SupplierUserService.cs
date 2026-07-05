using ArunVastra.Application.DTOs.Users.Supplier;
using ArunVastra.Application.DTOs.SupplierCategoryMappings;
using ArunVastra.Application.DTOs.SupplierTransportMappings;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using ArunVastra.Application.Models.Users;
using ArunVastra.Domain.Enums;

namespace ArunVastra.Application.Services;

public sealed class SupplierUserService : ISupplierUserService
{
    private readonly ISupplierUserRepository _supplierUserRepository;
    private readonly IPasswordService _passwordService;
    private readonly ICityRepository _cityRepository;
    private readonly ISupplierTransportMappingService _supplierTransportMappingService;
    private readonly ISupplierCategoryMappingService _supplierCategoryMappingService;

    public SupplierUserService(
        ISupplierUserRepository supplierUserRepository,
        IPasswordService passwordService,
        ICityRepository cityRepository,
        ISupplierTransportMappingService supplierTransportMappingService,
        ISupplierCategoryMappingService supplierCategoryMappingService)
    {
        _supplierUserRepository = supplierUserRepository;
        _passwordService = passwordService;
        _cityRepository = cityRepository;
        _supplierTransportMappingService = supplierTransportMappingService;
        _supplierCategoryMappingService = supplierCategoryMappingService;
    }

    public Task<SupplierUserListResponse> ListAsync(
        SupplierUserListRequest request,
        CancellationToken cancellationToken = default)
    {
        request.PageNumber = Math.Max(request.PageNumber, 1);
        request.PageSize = Math.Clamp(request.PageSize, 1, 1000);
        request.Filters ??= new SupplierUserListFiltersRequest();

        return _supplierUserRepository.ListAsync(request, cancellationToken);
    }

    public Task<SupplierUserAutocompleteResponse> GetAutocompleteValuesAsync(
        SupplierUserAutocompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Field = request.Field?.Trim().ToLowerInvariant();
        request.SearchKeyword = string.IsNullOrWhiteSpace(request.SearchKeyword) ? null : request.SearchKeyword.Trim();
        request.MaxResults = Math.Clamp(request.MaxResults, 1, 50);

        return _supplierUserRepository.GetAutocompleteValuesAsync(request, cancellationToken);
    }

    public Task<SupplierUserResponse?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        ValidateId(userId, "Supplier user id");
        return _supplierUserRepository.GetByIdAsync(userId, cancellationToken);
    }

    public Task<string> GetNextUserCodeAsync(CancellationToken cancellationToken = default)
    {
        return _supplierUserRepository.GetNextUserCodeAsync(cancellationToken);
    }

    public async Task<SupplierUserResponse> CreateAsync(CreateSupplierUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreate(request);

        var email = NormalizeEmail(request.Email);
        if (await _supplierUserRepository.EmailExistsAsync(email, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        await ValidateLocationAsync(request.StateId, request.CityId, cancellationToken);

        var agencyName = await ResolveAgencyNameAsync(request.AgencyId, cancellationToken);
        var passwordHash = _passwordService.HashPassword(
            new UserAuthModel { Email = email, Role = ((int)UserRole.Supplier).ToString() },
            request.Password);

        var supplier = await _supplierUserRepository.CreateAsync(
            new SupplierUserCreateModel
            {
                UserCode = NormalizeOptional(request.UserCode),
                Name = request.Name.Trim(),
                Email = email,
                LegacyPassword = request.Password,
                PasswordHash = passwordHash,
                Phone = NormalizeOptional(request.Phone),
                Mobile = NormalizeOptional(request.Mobile),
                Gstin = NormalizeOptional(request.Gstin)?.ToUpperInvariant(),
                BrandName = NormalizeOptional(request.BrandName),
                AgencyId = request.AgencyId,
                AgencyName = agencyName,
                CityId = request.CityId,
                Address = NormalizeOptional(request.Address),
                DharaProfit = request.DharaProfit,
                ExtraCharges = request.ExtraCharges,
                Discount = request.Discount,
                Remarks = NormalizeOptional(request.Remarks)
            },
            cancellationToken);

        await SaveMappingsAsync(supplier.UserId, ResolveTransportIds(request.TransportId, request.TransportIds), request.ProductCategoryId, cancellationToken);

        return await _supplierUserRepository.GetByIdAsync(supplier.UserId, cancellationToken) ?? supplier;
    }

    public async Task<SupplierUserResponse?> UpdateAsync(int userId, UpdateSupplierUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateId(userId, "Supplier user id");
        ValidateUpdate(request);

        var email = NormalizeEmail(request.Email);
        if (await _supplierUserRepository.EmailExistsAsync(email, userId, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        await ValidateLocationAsync(request.StateId, request.CityId, cancellationToken);

        var agencyName = await ResolveAgencyNameAsync(request.AgencyId, cancellationToken);
        var supplier = await _supplierUserRepository.UpdateAsync(
            userId,
            new SupplierUserUpdateModel
            {
                UserCode = NormalizeOptional(request.UserCode),
                Name = request.Name.Trim(),
                Email = email,
                Phone = NormalizeOptional(request.Phone),
                Mobile = NormalizeOptional(request.Mobile),
                Gstin = NormalizeOptional(request.Gstin)?.ToUpperInvariant(),
                BrandName = NormalizeOptional(request.BrandName),
                AgencyId = request.AgencyId,
                AgencyName = agencyName,
                CityId = request.CityId,
                Address = NormalizeOptional(request.Address),
                DharaProfit = request.DharaProfit,
                ExtraCharges = request.ExtraCharges,
                Discount = request.Discount,
                Remarks = NormalizeOptional(request.Remarks)
            },
            cancellationToken);

        if (supplier is null)
        {
            return null;
        }

        await SaveMappingsAsync(userId, ResolveTransportIds(request.TransportId, request.TransportIds), request.ProductCategoryId, cancellationToken);

        return await _supplierUserRepository.GetByIdAsync(userId, cancellationToken) ?? supplier;
    }

    public async Task DeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        ValidateId(userId, "Supplier user id");
        if (!await _supplierUserRepository.DeleteAsync(userId, cancellationToken))
        {
            throw new InvalidOperationException("Supplier was not found.");
        }
    }

    public async Task LockAsync(int userId, CancellationToken cancellationToken = default)
    {
        ValidateId(userId, "Supplier user id");
        if (!await _supplierUserRepository.LockAsync(userId, cancellationToken))
        {
            throw new InvalidOperationException("Supplier was not found.");
        }
    }

    public async Task ResetPasswordAsync(int userId, ResetSupplierPasswordRequest request, CancellationToken cancellationToken = default)
    {
        ValidateId(userId, "Supplier user id");
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        var supplier = await _supplierUserRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Supplier was not found.");
        var passwordHash = _passwordService.HashPassword(
            new UserAuthModel { Email = supplier.Email, Role = ((int)UserRole.Supplier).ToString() },
            request.Password);

        if (!await _supplierUserRepository.ResetPasswordAsync(userId, request.Password, passwordHash, cancellationToken))
        {
            throw new InvalidOperationException("Supplier was not found.");
        }
    }

    public Task<IReadOnlyList<SupplierOptionResponse>> GetAgencyOptionsAsync(string? searchKeyword, int take, CancellationToken cancellationToken = default)
    {
        return _supplierUserRepository.GetAgencyOptionsAsync(NormalizeOptional(searchKeyword), Math.Clamp(take <= 0 ? 20 : take, 1, 50), cancellationToken);
    }

    private static void ValidateCreate(CreateSupplierUserRequest request)
    {
        ValidateCommon(request.Name, request.Email);
        ValidateSupplierFields(
            request.Phone,
            request.Mobile,
            request.Gstin,
            request.BrandName,
            request.Address,
            request.StateId,
            request.CityId,
            request.AgencyId,
            request.DharaProfit);
        ValidateUserCode(request.UserCode);
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Password is required.");
        }
    }

    private static void ValidateUpdate(UpdateSupplierUserRequest request)
    {
        ValidateCommon(request.Name, request.Email);
        ValidateSupplierFields(
            request.Phone,
            request.Mobile,
            request.Gstin,
            request.BrandName,
            request.Address,
            request.StateId,
            request.CityId,
            request.AgencyId,
            request.DharaProfit);
        ValidateUserCode(request.UserCode);
    }

    private static void ValidateCommon(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }
    }

    private static void ValidateUserCode(string? userCode)
    {
        if (NormalizeOptional(userCode)?.Length > 5)
        {
            throw new InvalidOperationException("User code cannot be more than 5 characters.");
        }
    }

    private static void ValidateSupplierFields(
        string? phone,
        string? mobile,
        string? gstin,
        string? brandName,
        string? address,
        int? stateId,
        int? cityId,
        int? agencyId,
        int? dharaProfit)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new InvalidOperationException("Phone is required.");
        }

        if (string.IsNullOrWhiteSpace(mobile))
        {
            throw new InvalidOperationException("Mobile is required.");
        }

        if (string.IsNullOrWhiteSpace(gstin))
        {
            throw new InvalidOperationException("GSTIN is required.");
        }

        if (NormalizeOptional(gstin)?.Length != 15)
        {
            throw new InvalidOperationException("GSTIN must be 15 characters.");
        }

        if (string.IsNullOrWhiteSpace(brandName))
        {
            throw new InvalidOperationException("Brand Name is required.");
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new InvalidOperationException("User Address is required.");
        }

        if (!stateId.HasValue || stateId.Value <= 0)
        {
            throw new InvalidOperationException("State selection is required.");
        }

        if (!cityId.HasValue || cityId.Value <= 0)
        {
            throw new InvalidOperationException("City selection is required.");
        }

        if (!dharaProfit.HasValue)
        {
            throw new InvalidOperationException("Profit is required.");
        }

        if (!agencyId.HasValue || agencyId.Value <= 0)
        {
            throw new InvalidOperationException("Agent selection is required.");
        }
    }

    private async Task SaveMappingsAsync(int supplierUserId, IReadOnlyList<int> transportIds, int? productCategoryId, CancellationToken cancellationToken)
    {
        if (transportIds.Count > 0)
        {
            await _supplierTransportMappingService.SaveAsync(
                new SaveSupplierTransportMappingRequest { SupplierUserId = supplierUserId, TransportIds = transportIds },
                cancellationToken);
        }

        if (productCategoryId.HasValue && productCategoryId.Value > 0)
        {
            await _supplierCategoryMappingService.SaveAsync(
                new SaveSupplierCategoryMappingRequest { SupplierUserId = supplierUserId, ProductCategoryIds = [productCategoryId.Value] },
                cancellationToken);
        }
    }

    private static IReadOnlyList<int> ResolveTransportIds(int? transportId, IReadOnlyList<int>? transportIds)
    {
        var resolvedIds = (transportIds ?? [])
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (transportId.HasValue && transportId.Value > 0 && !resolvedIds.Contains(transportId.Value))
        {
            resolvedIds.Insert(0, transportId.Value);
        }

        return resolvedIds;
    }

    private async Task<string?> ResolveAgencyNameAsync(int? agencyId, CancellationToken cancellationToken)
    {
        if (!agencyId.HasValue)
        {
            return null;
        }

        var agency = (await _supplierUserRepository.GetAgencyOptionsAsync(null, 1000, cancellationToken))
            .FirstOrDefault(item => item.Id == agencyId.Value);

        if (agency is null)
        {
            throw new InvalidOperationException("Agency selection is invalid.");
        }

        return agency.Name;
    }

    private async Task ValidateLocationAsync(int? stateId, int? cityId, CancellationToken cancellationToken)
    {
        if (!stateId.HasValue && !cityId.HasValue)
        {
            return;
        }

        if (!stateId.HasValue)
        {
            throw new InvalidOperationException("State is required when city is selected.");
        }

        if (cityId is null)
        {
            if (!await _cityRepository.StateExistsAsync(stateId.Value, cancellationToken))
            {
                throw new InvalidOperationException("State does not exist.");
            }

            return;
        }

        if (!await _cityRepository.CityBelongsToStateAsync(cityId.Value, stateId.Value, cancellationToken))
        {
            throw new InvalidOperationException("City does not belong to selected state.");
        }
    }

    private static void ValidateId(int id, string name)
    {
        if (id <= 0)
        {
            throw new InvalidOperationException($"{name} must be greater than zero.");
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
