using ArunVastra.Application.DTOs.Users.Agency;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using ArunVastra.Application.Models.Users;
using ArunVastra.Domain.Enums;

namespace ArunVastra.Application.Services;

public sealed class AgencyUserService : IAgencyUserService
{
    private readonly IAgencyUserRepository _agencyUserRepository;
    private readonly IPasswordService _passwordService;
    private readonly ICityRepository _cityRepository;

    public AgencyUserService(
        IAgencyUserRepository agencyUserRepository,
        IPasswordService passwordService,
        ICityRepository cityRepository)
    {
        _agencyUserRepository = agencyUserRepository;
        _passwordService = passwordService;
        _cityRepository = cityRepository;
    }

    public Task<AgencyUserListResponse> ListAsync(
        AgencyUserListRequest request,
        CancellationToken cancellationToken = default)
    {
        request.PageNumber = Math.Max(request.PageNumber, 1);
        request.PageSize = Math.Clamp(request.PageSize, 1, 1000);
        request.Filters ??= new AgencyUserListFiltersRequest();

        return _agencyUserRepository.ListAsync(request, cancellationToken);
    }

    public Task<AgencyUserResponse?> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return _agencyUserRepository.GetByIdAsync(userId, cancellationToken);
    }

    public async Task<AgencyUserResponse> CreateAsync(
        CreateAgencyUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreate(request);

        var email = NormalizeEmail(request.Email);

        if (await _agencyUserRepository.EmailExistsAsync(email, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        await ValidateLocationAsync(request.StateId, request.CityId, cancellationToken);

        var passwordHash = _passwordService.HashPassword(
            new UserAuthModel { Email = email, Role = ((int)UserRole.Agency).ToString() },
            request.Password);

        return await _agencyUserRepository.CreateAsync(
            new AgencyUserCreateModel
            {
                Name = request.Name.Trim(),
                Email = email,
                PasswordHash = passwordHash,
                Phone = NormalizeOptional(request.Phone),
                Mobile = NormalizeOptional(request.Mobile),
                Gstin = NormalizeOptional(request.Gstin)?.ToUpperInvariant(),
                BrandName = NormalizeOptional(request.BrandName),
                Address = NormalizeOptional(request.Address),
                CityId = request.CityId,
                Remarks = NormalizeOptional(request.Remarks),
                Status = request.Status
            },
            cancellationToken);
    }

    public async Task<AgencyUserResponse?> UpdateAsync(
        int userId,
        UpdateAgencyUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUpdate(request);

        var email = NormalizeEmail(request.Email);

        if (await _agencyUserRepository.EmailExistsAsync(email, userId, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        await ValidateLocationAsync(request.StateId, request.CityId, cancellationToken);

        return await _agencyUserRepository.UpdateAsync(
            userId,
            new AgencyUserUpdateModel
            {
                Name = request.Name.Trim(),
                Email = email,
                Phone = NormalizeOptional(request.Phone),
                Mobile = NormalizeOptional(request.Mobile),
                Gstin = NormalizeOptional(request.Gstin)?.ToUpperInvariant(),
                BrandName = NormalizeOptional(request.BrandName),
                Address = NormalizeOptional(request.Address),
                CityId = request.CityId,
                Remarks = NormalizeOptional(request.Remarks),
                Status = request.Status
            },
            cancellationToken);
    }

    private static void ValidateCreate(CreateAgencyUserRequest request)
    {
        ValidateCommon(request.Name, request.Email);

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Password and confirm password do not match.");
        }
    }

    private static void ValidateUpdate(UpdateAgencyUserRequest request)
    {
        ValidateCommon(request.Name, request.Email);
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

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private async Task ValidateLocationAsync(
        int? stateId,
        int? cityId,
        CancellationToken cancellationToken)
    {
        if (!stateId.HasValue && !cityId.HasValue)
        {
            return;
        }

        if (!stateId.HasValue)
        {
            throw new InvalidOperationException("State is required when city is selected.");
        }

        if (stateId.Value <= 0)
        {
            throw new InvalidOperationException("State id must be greater than zero.");
        }

        if (cityId is null)
        {
            if (!await _cityRepository.StateExistsAsync(stateId.Value, cancellationToken))
            {
                throw new InvalidOperationException("State does not exist.");
            }

            return;
        }

        if (cityId.Value <= 0)
        {
            throw new InvalidOperationException("City id must be greater than zero.");
        }

        if (!await _cityRepository.CityBelongsToStateAsync(cityId.Value, stateId.Value, cancellationToken))
        {
            throw new InvalidOperationException("City does not belong to selected state.");
        }
    }
}
