using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Authentication.Refresh;

public sealed class RefreshTokenCommandHandler(
    IApplicationDbContext dbContext,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider clock)
    : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var stored = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken)
            .ConfigureAwait(false);

        if (stored is null || stored.RevokedAtUtc is not null || clock.UtcNow >= stored.ExpiresAtUtc)
        {
            throw new UnauthorizedException("The refresh token is invalid or has expired.");
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == stored.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("The refresh token is invalid or has expired.");
        }

        // Rotate: revoke the presented token and issue a fresh pair.
        stored.RevokedAtUtc = clock.UtcNow;

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
}
