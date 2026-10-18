using Foodiya.Application.DTOs.Auth.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Auth;

public interface IAccessTokenService
{
    AuthTokenResponse CreateToken(AppUser user);
}
