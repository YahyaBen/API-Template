using foodiya.app.Identity.Infrastructure.Registration;

namespace foodiya.app.Identity.Infrastructure.Tests.Registration;

[TestClass]
public sealed class RegistrationServiceTests
{
    [TestMethod]
    public void MaskEmail_WithoutSeparator_ReturnsFullyMaskedAddress()
    {
        var maskedEmail = RegistrationService.MaskEmail("not-an-email");

        Assert.AreEqual("***", maskedEmail);
    }
}
