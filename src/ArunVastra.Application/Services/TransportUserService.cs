using ArunVastra.Application.DTOs.Users.Transport;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using ArunVastra.Application.Models.Users;
using ArunVastra.Domain.Enums;

namespace ArunVastra.Application.Services;

public sealed class TransportUserService : ITransportUserService
{
    private readonly ITransportUserRepository _transportUserRepository;
    private readonly IPasswordService _passwordService;
    private readonly ICityRepository _cityRepository;

    public TransportUserService(
        ITransportUserRepository transportUserRepository,
        IPasswordService passwordService,
        ICityRepository cityRepository)
    {
        _transportUserRepository = transportUserRepository;
        _passwordService = passwordService;
        _cityRepository = cityRepository;
    }

    public Task<TransportUserListResponse> ListAsync(
        TransportUserListRequest request,
        CancellationToken cancellationToken = default)
    {
        request.PageNumber = Math.Max(request.PageNumber, 1);
        request.PageSize = Math.Clamp(request.PageSize, 1, 1000);
        request.Filters ??= new TransportUserListFiltersRequest();

        return _transportUserRepository.ListAsync(request, cancellationToken);
    }

    public Task<TransportUserResponse?> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return _transportUserRepository.GetByIdAsync(userId, cancellationToken);
    }

    public async Task<TransportUserResponse> CreateAsync(
        CreateTransportUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreate(request);

        var email = NormalizeEmail(request.Email);

        if (await _transportUserRepository.EmailExistsAsync(email, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        await ValidateLocationAsync(request.StateId, request.CityId, cancellationToken);

        var passwordHash = _passwordService.HashPassword(
            new UserAuthModel { Email = email, Role = ((int)UserRole.Transport).ToString() },
            request.Password);

        return await _transportUserRepository.CreateAsync(
            new TransportUserCreateModel
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

    public async Task<TransportUserResponse?> UpdateAsync(
        int userId,
        UpdateTransportUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUpdate(request);

        var email = NormalizeEmail(request.Email);

        if (await _transportUserRepository.EmailExistsAsync(email, userId, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        await ValidateLocationAsync(request.StateId, request.CityId, cancellationToken);

        return await _transportUserRepository.UpdateAsync(
            userId,
            new TransportUserUpdateModel
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

    private static void ValidateCreate(CreateTransportUserRequest request)
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

    private static void ValidateUpdate(UpdateTransportUserRequest request)
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
