namespace foodiya.app.Identity.Infrastructure.Authentication;

internal interface ILoginTimingProtection
{
    void VerifyDummyPassword(string suppliedPassword);
}
