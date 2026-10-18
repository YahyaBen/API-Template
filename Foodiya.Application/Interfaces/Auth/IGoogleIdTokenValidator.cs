using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Auth;

public interface IGoogleIdTokenValidator
{
    Task<GoogleIdentityTokenPayload> ValidateAsync(string idToken, CancellationToken ct = default);
}
