using System.Text.Json;
using Foodiya.Infrastructure.Messaging;
using Foodiya.Infrastructure.Identity;
using foodiya.app.Messaging;
using Microsoft.EntityFrameworkCore;

namespace foodiya.app.Identity.Integration.Tests.Messaging;

[TestClass]
public sealed class IdentityProfileEventHandlerTests(TestContext testContext)
{
    [TestMethod]
    public async Task HandleAsync_DuplicateRegistration_CreatesOneProfileAndOneInboxEntry()
    {
        await using var database = new ProfileTestDatabase();
        await database.InitializeAsync(testContext.CancellationToken);
        var userId = Guid.NewGuid();
        var envelope = Registered(userId);
        await using (var db = database.CreateContext())
            await new IdentityProfileEventHandler(db, new IdentityProfileProvisioner(db, TimeProvider.System)).HandleAsync(envelope, testContext.CancellationToken);
        await using (var db = database.CreateContext())
            await new IdentityProfileEventHandler(db, new IdentityProfileProvisioner(db, TimeProvider.System)).HandleAsync(envelope, testContext.CancellationToken);
        await using var verification = database.CreateContext();
        var profile = await verification.AppUsers.SingleAsync(testContext.CancellationToken);
        Assert.AreEqual(userId, profile.Uuid);
        Assert.AreEqual("Mariam", profile.FirstName);
        Assert.AreEqual(1, await verification.IdentityInboxMessages.CountAsync(testContext.CancellationToken));
        Assert.IsEmpty(profile.PasswordHash);
    }

    [TestMethod]
    public async Task HandleAsync_NewEventForExistingSubject_DoesNotOverwriteOrReactivateProfile()
    {
        await using var database = new ProfileTestDatabase();
        await database.InitializeAsync(testContext.CancellationToken);
        var userId = Guid.NewGuid();
        await using var db = database.CreateContext();
        var handler = new IdentityProfileEventHandler(db, new IdentityProfileProvisioner(db, TimeProvider.System));
        await handler.HandleAsync(Registered(userId), testContext.CancellationToken);
        var profile = await db.AppUsers.SingleAsync(testContext.CancellationToken);
        profile.FirstName = "Edited";
        profile.IsActive = false;
        await db.SaveChangesAsync(testContext.CancellationToken);
        await handler.HandleAsync(Registered(userId), testContext.CancellationToken);
        var existing = await db.AppUsers.SingleAsync(testContext.CancellationToken);
        Assert.AreEqual("Edited", existing.FirstName);
        Assert.IsFalse(existing.IsActive);
        Assert.AreEqual(1, await db.AppUsers.CountAsync(testContext.CancellationToken));
        Assert.AreEqual(2, await db.IdentityInboxMessages.CountAsync(testContext.CancellationToken));
    }

    [TestMethod]
    public async Task HandleAsync_EmailConflict_RollsBackInboxAndDoesNotRelinkProfile()
    {
        await using var database = new ProfileTestDatabase();
        await database.InitializeAsync(testContext.CancellationToken);
        await using var db = database.CreateContext();
        var handler = new IdentityProfileEventHandler(db, new IdentityProfileProvisioner(db, TimeProvider.System));
        var originalId = Guid.NewGuid();
        await handler.HandleAsync(Registered(originalId), testContext.CancellationToken);
        var conflicting = Registered(Guid.NewGuid());
        await Assert.ThrowsExactlyAsync<JsonException>(() => handler.HandleAsync(conflicting, testContext.CancellationToken));
        Assert.AreEqual(originalId, (await db.AppUsers.SingleAsync(testContext.CancellationToken)).Uuid);
        Assert.AreEqual(1, await db.IdentityInboxMessages.CountAsync(testContext.CancellationToken));
    }

    [TestMethod]
    public async Task HandleAsync_FailedProfileSave_DoesNotCommitInbox()
    {
        await using var database = new ProfileTestDatabase();
        await database.InitializeAsync(testContext.CancellationToken);
        await using var db = database.CreateContext();
        await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER reject_profile BEFORE INSERT ON AppUser BEGIN SELECT RAISE(ABORT, 'simulated failure'); END;", testContext.CancellationToken);
        var envelope = Registered(Guid.NewGuid());
        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => new IdentityProfileEventHandler(db, new IdentityProfileProvisioner(db, TimeProvider.System)).HandleAsync(envelope, testContext.CancellationToken));
        db.ChangeTracker.Clear();
        Assert.AreEqual(0, await db.AppUsers.CountAsync(testContext.CancellationToken));
        Assert.AreEqual(0, await db.IdentityInboxMessages.CountAsync(testContext.CancellationToken));
        await db.Database.ExecuteSqlRawAsync("DROP TRIGGER reject_profile;", testContext.CancellationToken);
        await new IdentityProfileEventHandler(db, new IdentityProfileProvisioner(db, TimeProvider.System)).HandleAsync(envelope, testContext.CancellationToken);
        Assert.AreEqual(1, await db.AppUsers.CountAsync(testContext.CancellationToken));
        Assert.AreEqual(1, await db.IdentityInboxMessages.CountAsync(testContext.CancellationToken));
    }

    [TestMethod]
    public async Task HandleAsync_UnsupportedSchema_DoesNotCreateProfile()
    {
        await using var database = new ProfileTestDatabase();
        await database.InitializeAsync(testContext.CancellationToken);
        await using var db = database.CreateContext();
        await Assert.ThrowsExactlyAsync<JsonException>(() => new IdentityProfileEventHandler(db, new IdentityProfileProvisioner(db, TimeProvider.System))
            .HandleAsync(Registered(Guid.NewGuid()) with { SchemaVersion = 2 }, testContext.CancellationToken));
        Assert.AreEqual(0, await db.AppUsers.CountAsync(testContext.CancellationToken));
    }

    internal static IntegrationEventEnvelope Registered(Guid userId) => new(Guid.NewGuid(), "UserRegistered", DateTimeOffset.UtcNow,
        JsonSerializer.SerializeToElement(new UserRegisteredEventPayload(userId, "mariam@example.test", "mariam", "Mariam", "Tester")));
}
