using foodiya.app.Identity.Infrastructure.Authentication;
using foodiya.app.Identity.Infrastructure.Options;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;

namespace foodiya.app.Identity.Infrastructure.Tests.Authentication;

[TestClass]
public sealed class TokenCryptographyServiceTests
{
    private readonly TokenCryptographyService _sut = new(MicrosoftOptions.Create(new AuthenticationTokenOptions
    {
        RefreshTokenPepper = "unit-test-refresh-token-pepper-32-characters"
    }));

    [TestMethod]
    public void HashRefreshToken_SameToken_ReturnsStableNonPlaintextHash()
    {
        // Arrange
        const string refreshToken = "raw-refresh-token-that-must-never-be-stored";

        // Act
        var firstHash = _sut.HashRefreshToken(refreshToken);
        var secondHash = _sut.HashRefreshToken(refreshToken);

        // Assert
        Assert.AreEqual(firstHash, secondHash);
        Assert.AreEqual(64, firstHash.Length);
        Assert.DoesNotContain(refreshToken, firstHash);
        Assert.MatchesRegex("^[0-9A-F]{64}$", firstHash);
    }

    [TestMethod]
    public void HashDeviceInstallationId_WhitespaceVariants_ReturnSameHash()
    {
        // Act
        var compactHash = _sut.HashDeviceInstallationId("device-installation-42");
        var paddedHash = _sut.HashDeviceInstallationId("  device-installation-42  ");

        // Assert
        Assert.AreEqual(compactHash, paddedHash);
    }

    [TestMethod]
    public void HashMethods_SameInput_UseSeparateDomains()
    {
        // Arrange
        const string value = "same-sensitive-value";

        // Act
        var refreshHash = _sut.HashRefreshToken(value);
        var deviceHash = _sut.HashDeviceInstallationId(value);

        // Assert
        Assert.AreNotEqual(refreshHash, deviceHash);
    }

    [TestMethod]
    public void GenerateRefreshToken_RepeatedCalls_ReturnDistinctBase64UrlTokens()
    {
        // Act
        var tokens = Enumerable.Range(0, 8)
            .Select(_ => _sut.GenerateRefreshToken())
            .ToArray();

        // Assert
        Assert.HasCount(8, tokens.Distinct(StringComparer.Ordinal));
        foreach (var token in tokens)
        {
            Assert.MatchesRegex("^[A-Za-z0-9_-]{86}$", token);
        }
    }
}
