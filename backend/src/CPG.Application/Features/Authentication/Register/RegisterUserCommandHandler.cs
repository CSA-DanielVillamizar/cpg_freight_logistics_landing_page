using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Authentication.Register;

public sealed class RegisterUserCommandHandler(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider clock)
    : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    /// <summary>Placeholder until an Admin assigns the real CPG DOT/MC license during agent activation.</summary>
    private const string PendingLicenseReference = "PENDING-ASSIGNMENT";

    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await dbContext.Users
            .AnyAsync(u => u.Email == email, cancellationToken)
            .ConfigureAwait(false);

        if (emailTaken)
        {
            throw new Domain.Common.DomainException($"An account with email '{email}' already exists.");
        }

        var companyName = Normalize(request.CompanyName);
        var user = User.Register(
            email,
            passwordHasher.Hash(request.Password),
            request.FullName.Trim(),
            request.Role,
            companyName,
            Normalize(request.PhoneNumber));

        dbContext.Users.Add(user);

        switch (request.Role)
        {
            case UserRole.Carrier:
                dbContext.Carriers.Add(new Carrier
                {
                    CompanyName = companyName ?? user.FullName,
                    UserId = user.Id,
                    DotNumber = Normalize(request.DotNumber),
                    McNumber = Normalize(request.McNumber),
                });
                break;

            case UserRole.Agent:
                dbContext.Agents.Add(new Agent
                {
                    CompanyName = companyName ?? user.FullName,
                    UserId = user.Id,
                    CpgLicenseReference = PendingLicenseReference,
                });
                break;

            case UserRole.Shipper:
            case UserRole.Admin:
            default:
                break;
        }

        var tokens = jwtTokenService.IssueTokens(user);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = tokens.RefreshToken,
            ExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc,
            CreatedAtUtc = clock.UtcNow,
        });

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AuthResponse
        {
            AccessToken = tokens.AccessToken,
            ExpiresAtUtc = tokens.ExpiresAtUtc,
            RefreshToken = tokens.RefreshToken,
            User = new AuthenticatedUser
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
            },
        };
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
