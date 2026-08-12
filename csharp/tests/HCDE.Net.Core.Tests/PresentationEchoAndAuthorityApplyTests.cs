using System.Text;

namespace HCDE.Net.Core.Tests;

public class PresentationEchoWeaponChangePolicyTests
{
    [Fact]
    public void UninitializedRecipient_RequestsReadyClassAndReseat()
    {
        var flags = PresentationEchoWeaponChangePolicy.ComputeWeaponChangeFlags(
            lastState: default,
            Encoding.UTF8.GetBytes("Pistol"),
            Encoding.UTF8.GetBytes("Pistol"),
            pspriteStateOffset: 0,
            weaponState: LiveConstants.WeaponStateReady);

        Assert.Equal(
            LiveConstants.WeaponChangeReadyClass | LiveConstants.WeaponChangeForceReseat,
            flags);
    }

    [Fact]
    public void ReadyWeaponChange_SetsReadyClassFlag()
    {
        var last = new PresentationEchoLastState(
            Encoding.UTF8.GetBytes("Pistol"),
            Encoding.UTF8.GetBytes("Pistol"),
            pspriteStateOffset: 1,
            weaponState: LiveConstants.WeaponStateReady,
            initialized: true);

        var flags = PresentationEchoWeaponChangePolicy.ComputeWeaponChangeFlags(
            last,
            Encoding.UTF8.GetBytes("Shotgun"),
            Encoding.UTF8.GetBytes("Shotgun"),
            pspriteStateOffset: 1,
            weaponState: LiveConstants.WeaponStateReady);

        Assert.Equal(
            LiveConstants.WeaponChangeReadyClass | LiveConstants.WeaponChangeForceReseat,
            flags);
    }

    [Fact]
    public void LostWeaponReady_ForcesReseat()
    {
        var last = new PresentationEchoLastState(
            Encoding.UTF8.GetBytes("Shotgun"),
            Encoding.UTF8.GetBytes("Shotgun"),
            pspriteStateOffset: 4,
            weaponState: LiveConstants.WeaponStateReady,
            initialized: true);

        var flags = PresentationEchoWeaponChangePolicy.ComputeWeaponChangeFlags(
            last,
            Encoding.UTF8.GetBytes("Shotgun"),
            Encoding.UTF8.GetBytes("Shotgun"),
            pspriteStateOffset: 4,
            weaponState: 0);

        Assert.Equal(LiveConstants.WeaponChangeForceReseat, flags);
    }
}

public class PresentationEchoApplySessionTests
{
    private sealed class RecordingEchoSink : IPresentationEchoApplySink
    {
        public int InventoryCalls { get; private set; }
        public int FollowCalls { get; private set; }

        public bool ReconcileInventory(byte playerSlot, PresentationEchoInventoryItem[] items)
        {
            InventoryCalls++;
            return playerSlot == 0 && items.Length == 2;
        }

        public bool FollowWeapon(PresentationEchoPlayerRecord player)
        {
            FollowCalls++;
            return player.WeaponChangeFlags != 0;
        }
    }

    [Fact]
    public void Apply_ReconcilesInventoryBeforeWeaponFollow()
    {
        var session = new PresentationEchoApplySession(maxClients: 4);
        var sink = new RecordingEchoSink();
        var block = PresentationEchoCodec.CreateExampleBlock();

        Assert.True(session.TryApply(0, block, sink, out var result, out _));
        Assert.True(result.InventoryApplied);
        Assert.Equal(1, result.WeaponFollowAttempts);
        Assert.Equal(1, result.WeaponFollowApplied);
        Assert.Equal(1, sink.InventoryCalls);
        Assert.Equal(1, sink.FollowCalls);
    }
}

public class AuthorityEventsApplySessionTests
{
    private sealed class RecordingAuthoritySink : IAuthorityEventSink
    {
        public List<AuthorityEventType> Routed { get; } = new();

        public bool TryApplyInvasionSpawn(AuthorityEventRecord record)
        {
            Routed.Add(record.EventType);
            return true;
        }

        public bool TryApplyPickupSpawn(AuthorityEventRecord record) => false;
        public bool TryApplyInvasionDespawn(AuthorityEventRecord record) => false;
        public bool TryApplyPickupRetire(AuthorityEventRecord record) => false;
        public bool TryApplyInvasionDamage(AuthorityEventRecord record) => false;
        public bool TryApplyCoopProjectileSpawn(AuthorityEventRecord record)
        {
            Routed.Add(record.EventType);
            return true;
        }

        public bool TryApplyCoopProjectileRetire(AuthorityEventRecord record) => false;
        public bool TryApplyCoopCosmeticSpawn(AuthorityEventRecord record) => false;
    }

    [Fact]
    public void Apply_RoutesKnownRecordsAndCountsMissing()
    {
        var records = new[]
        {
            new AuthorityEventRecord(
                AuthorityEventType.Spawn,
                ReplicatedActorSource.Invasion,
                ReplicatedActorCategory.Monster,
                actorFlags: 0,
                actorId: 1,
                eventTic: 10,
                classId: 0,
                health: 100,
                wave: 1,
                Encoding.UTF8.GetBytes("Imp"),
                posX: 1,
                posY: 2,
                posZ: 3,
                velX: 0,
                velY: 0,
                velZ: 0,
                yaw: 0,
                pitch: 0),
            new AuthorityEventRecord(
                AuthorityEventType.Spawn,
                ReplicatedActorSource.Coop,
                ReplicatedActorCategory.Projectile,
                actorFlags: 0,
                actorId: 2,
                eventTic: 11,
                classId: 7,
                health: 1,
                wave: 0,
                Encoding.UTF8.GetBytes("Rocket"),
                posX: 4,
                posY: 5,
                posZ: 6,
                velX: 0,
                velY: 0,
                velZ: 0,
                yaw: 0,
                pitch: 0),
            new AuthorityEventRecord(
                AuthorityEventType.Damage,
                ReplicatedActorSource.Shared,
                ReplicatedActorCategory.Monster,
                actorFlags: 0,
                actorId: 3,
                eventTic: 12,
                classId: 0,
                health: -10,
                wave: 0,
                Array.Empty<byte>(),
                posX: 0,
                posY: 0,
                posZ: 0,
                velX: 0,
                velY: 0,
                velZ: 0,
                yaw: 0,
                pitch: 0),
        };

        var sink = new RecordingAuthoritySink();
        Assert.True(AuthorityEventsApplySession.TryApply(records, sink, out var result, out _));
        Assert.Equal(3, result.RecordCount);
        Assert.Equal(2, result.Applied);
        Assert.Equal(1, result.Missing);
        Assert.Equal(2, sink.Routed.Count);
    }
}
