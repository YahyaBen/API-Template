using Microsoft.IdentityModel.Tokens;

namespace foodiya.app.Identity.Infrastructure.Authentication;

public interface IRsaSigningKeyProvider
{
    RsaSecurityKey GetSigningKey();
    RsaSecurityKey GetValidationKey();
    JsonWebKeySet GetPublicJwks();
}
