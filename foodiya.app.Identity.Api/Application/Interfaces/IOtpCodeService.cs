namespace foodiya.app.Identity.Application.Interfaces;

public interface IOtpCodeService
{
    string GenerateCode();
    string HashCode(Guid challengeId, string code);
    bool VerifyCode(Guid challengeId, string code, string expectedHash);
}
