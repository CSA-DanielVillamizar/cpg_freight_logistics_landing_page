using CPG.Domain.Enums;
using MediatR;

namespace CPG.Application.Features.Authentication.Register;

/// <summary>
/// Tri-Sign-Up: creates the <c>User</c> and, depending on <see cref="Role"/>, the matching
/// business profile (<c>Carrier</c> or <c>Agent</c>) in a single transaction, then issues
/// tokens exactly like <c>LoginCommand</c> so the visitor lands signed-in (T-SDD Epica 1).
/// </summary>
public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FullName,
    UserRole Role,
    string? CompanyName,
    string? PhoneNumber,
    string? DotNumber,
    string? McNumber) : IRequest<AuthResponse>
{
    public static RegisterUserCommand FromRequest(RegisterRequest request) => new(
        request.Email,
        request.Password,
        request.FullName,
        request.Role,
        request.CompanyName,
        request.PhoneNumber,
        request.DotNumber,
        request.McNumber);
}
