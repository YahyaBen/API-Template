namespace foodiya.app.Identity.Infrastructure.Authentication;

internal interface ITokenCryptographyService
{
    string GenerateRefreshToken();
    string HashRefreshToken(string token);
    string HashDeviceInstallationId(string installationId);
}
