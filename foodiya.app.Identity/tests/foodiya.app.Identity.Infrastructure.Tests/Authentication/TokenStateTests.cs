using foodiya.app.Identity.Domain.Entities;

namespace foodiya.app.Identity.Infrastructure.Tests.Authentication;

[TestClass]
public sealed class TokenStateTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 4, 10, 30, 0, TimeSpan.Zero);

    [TestMethod]
    [DataRow(-1, false, DisplayName = "Expired one minute ago")]
    [DataRow(0, false, DisplayName = "Exact expiry boundary")]
    [DataRow(1, true, DisplayName = "Expires one minute later")]
    public void RefreshTokenIsActive_ExpirationBoundary_ReturnsExpected(
        int expiryOffsetMinutes,
        bool expected)
    {
        // Arrange
        var token = new RefreshToken
        {
            TokenHash = new string('A', 64),
            ExpiresAtUtc = Now.AddMinutes(expiryOffsetMinutes)
        };

        // Act
        var actual = token.IsActive(Now);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void RefreshTokenIsActive_RevokedBeforeExpiry_ReturnsFalse()
    {
        // Arrange
        var token = new RefreshToken
        {
            TokenHash = new string('A', 64),
            ExpiresAtUtc = Now.AddHours(1),
            RevokedAtUtc = Now
        };

        // Act
        var actual = token.IsActive(Now);

        // Assert
        Assert.IsFalse(actual);
    }

    [TestMethod]
    [DataRow(-1, false, DisplayName = "Expired one minute ago")]
    [DataRow(0, false, DisplayName = "Exact expiry boundary")]
    [DataRow(1, true, DisplayName = "Expires one minute later")]
    public void UserSessionIsActive_ExpirationBoundary_ReturnsExpected(
        int expiryOffsetMinutes,
        bool expected)
    {
        // Arrange
        var session = new UserSession
        {
            ExpiresAtUtc = Now.AddMinutes(expiryOffsetMinutes)
        };

        // Act
        var actual = session.IsActive(Now);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void UserSessionIsActive_RevokedBeforeExpiry_ReturnsFalse()
    {
        // Arrange
        var session = new UserSession
        {
            ExpiresAtUtc = Now.AddHours(1),
            RevokedAtUtc = Now
        };

        // Act
        var actual = session.IsActive(Now);

        // Assert
        Assert.IsFalse(actual);
    }
}
