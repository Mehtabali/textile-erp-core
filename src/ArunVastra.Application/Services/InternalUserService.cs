using ArunVastra.Application.DTOs.Users.Internal;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using ArunVastra.Application.Models.Users;
using ArunVastra.Domain.Enums;

namespace ArunVastra.Application.Services;

public sealed class InternalUserService : IInternalUserService
{
    private readonly IInternalUserRepository _internalUserRepository;
    private readonly IPasswordService _passwordService;

    public InternalUserService(
        IInternalUserRepository internalUserRepository,
        IPasswordService passwordService)
    {
        _internalUserRepository = internalUserRepository;
        _passwordService = passwordService;
    }

    public Task<InternalUserListResponse> ListAsync(
        InternalUserListRequest request,
        CancellationToken cancellationToken = default)
    {
        request.PageNumber = Math.Max(request.PageNumber, 1);
        request.PageSize = Math.Clamp(request.PageSize, 1, 1000);
        request.Filters ??= new InternalUserListFiltersRequest();

        return _internalUserRepository.ListAsync(request, cancellationToken);
    }

    public Task<InternalUserResponse?> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return _internalUserRepository.GetByIdAsync(userId, cancellationToken);
    }

    public async Task<InternalUserResponse> CreateAsync(
        CreateInternalUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreate(request);

        var email = NormalizeEmail(request.Email);

        if (await _internalUserRepository.EmailExistsAsync(email, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var passwordHash = _passwordService.HashPassword(
            new UserAuthModel { Email = email, Role = request.Role.ToString() },
            request.Password);

        return await _internalUserRepository.CreateAsync(
            new InternalUserCreateModel
            {
                Name = request.Name.Trim(),
                Email = email,
                PasswordHash = passwordHash,
                Role = request.Role,
                Phone = NormalizeOptional(request.Phone),
                Mobile = NormalizeOptional(request.Mobile),
                Gstin = NormalizeOptional(request.Gstin),
                BrandName = NormalizeOptional(request.BrandName),
                Remarks = NormalizeOptional(request.Remarks),
                Status = request.Status
            },
            cancellationToken);
    }

    public async Task<InternalUserResponse?> UpdateAsync(
        int userId,
        UpdateInternalUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUpdate(request);

        var email = NormalizeEmail(request.Email);

        if (await _internalUserRepository.EmailExistsAsync(email, userId, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        return await _internalUserRepository.UpdateAsync(
            userId,
            new InternalUserUpdateModel
            {
                Name = request.Name.Trim(),
                Email = email,
                Role = request.Role,
                Phone = NormalizeOptional(request.Phone),
                Mobile = NormalizeOptional(request.Mobile),
                Gstin = NormalizeOptional(request.Gstin),
                BrandName = NormalizeOptional(request.BrandName),
                Remarks = NormalizeOptional(request.Remarks),
                Status = request.Status
            },
            cancellationToken);
    }

    private static void ValidateCreate(CreateInternalUserRequest request)
    {
        ValidateCommon(request.Name, request.Email);
        ValidateRole(request.Role);

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Password and confirm password do not match.");
        }
    }

    private static void ValidateUpdate(UpdateInternalUserRequest request)
    {
        ValidateCommon(request.Name, request.Email);
        ValidateRole(request.Role);
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

    private static void ValidateRole(int role)
    {
        if (role != (int)UserRole.Admin && role != (int)UserRole.FloorManager)
        {
            throw new InvalidOperationException("Internal user type is invalid.");
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
}
