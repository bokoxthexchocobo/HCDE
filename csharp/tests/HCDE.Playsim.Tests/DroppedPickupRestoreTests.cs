using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DroppedPickupRestoreTests
{
    [Theory]
    [InlineData(2007, false, false)]
    [InlineData(2007, false, true)]
    [InlineData(2007, true, false)]
    [InlineData(2007, true, true)]
    [InlineData(2001, false, false)]
    [InlineData(2001, false, true)]
    [InlineData(2001, true, false)]
    [InlineData(2001, true, true)]
    [InlineData(8, false, false)]
    [InlineData(8, false, true)]
    [InlineData(8, true, false)]
    [InlineData(8, true, true)]
    [InlineData(2012, false, false)]
    [InlineData(2012, false, true)]
    [InlineData(2012, true, false)]
    [InlineData(2012, true, true)]
    public void ReconstructsTossedPickupAndReplaysMovementWithoutAnotherDropRoll(int type, bool established, bool remove)
    {
        var (sim, source) = Room();
        Assert.True(sim.SpawnDroppedPickup(source, type, ignoreAmmoSkill: true, pickupAmount: 7,
            inventoryToss: true, depleted: true, suppressWeaponAmmo: true));
        var drop = sim.Actors.Single(actor => actor.SpawnedDroppedPickup);
        drop.ThingId = 77; sim.Thinkers.ChangeStatNum(drop, 120);
        if (established) sim.Tick();
        var bytes = Save(sim); Assert.Equal(217, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 20).Select(_ => Step(sim, drop.Id)).ToArray();
        drop.Destroy(); if (remove) sim.Tick();

        SimSavegame.Apply(sim, bytes);

        var restored = sim.Actors.Single(actor => actor.Id == drop.Id);
        Assert.IsType<Actor>(restored); Assert.True(restored.SpawnedDroppedPickup);
        Assert.Equal(77, restored.ThingId); Assert.Equal(120, restored.StatNum);
        Assert.True(restored.IgnoreAmmoSkill); Assert.True(restored.Depleted);
        Assert.True(restored.SuppressWeaponPickupAmmo); Assert.Equal(7, restored.PickupAmount);
        Assert.Equal(bytes, Save(sim));
        Assert.Equal(expected, Enumerable.Range(0, 20).Select(_ => Step(sim, drop.Id)).ToArray());
    }

    [Fact]
    public void RewindBeforeDropRemovesLaterPickupAndReusesItsId()
    {
        var (sim, source) = Room(); sim.Rewind.Capture(sim); var before = Save(sim);
        Assert.True(sim.SpawnDroppedPickup(source, 2007)); var drop = sim.Actors.Single(actor => actor.SpawnedDroppedPickup);
        Assert.True(sim.Rewind.RestoreOldest(sim));
        Assert.True(drop.Destroyed); Assert.Equal(before, Save(sim));
        Assert.True(sim.SpawnDroppedPickup(source, 2007));
        Assert.Equal(drop.Id, sim.Actors.Single(actor => actor.SpawnedDroppedPickup).Id);
    }

    [Fact]
    public void ReconstructionRestoresPointersAndProjectileOwnership()
    {
        var (sim, source) = Room(); Assert.True(sim.SpawnDroppedPickup(source, 2007));
        var drop = sim.Actors.Single(actor => actor.SpawnedDroppedPickup);
        source.TracerId = drop.Id; drop.MasterId = source.Id;
        var missile = sim.SpawnProjectile(drop, ProjectileKind.Plasma); var bytes = Save(sim);
        drop.Destroy(); missile.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        var restored = sim.Actors.Single(actor => actor.Id == drop.Id);
        Assert.Equal(drop.Id, source.TracerId); Assert.Equal(source.Id, restored.MasterId);
        Assert.Same(restored, sim.Actors.OfType<ProjectileActor>().Single(actor => actor.Id == missile.Id).Owner);
        Assert.Equal(bytes, Save(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpawnNotificationsRespectSavedFreshFlag(bool established)
    {
        var (sim, source) = Room(); sim.SpawnDroppedPickup(source, 2007);
        var drop = sim.Actors.Single(actor => actor.SpawnedDroppedPickup);
        if (established) sim.Tick();
        var bytes = Save(sim); drop.Destroy(); sim.Tick();
        var spawned = 0; sim.WorldThingSpawned += actor => { if (actor.Id == drop.Id) spawned++; };
        SimSavegame.Apply(sim, bytes); Assert.Equal(0, spawned); sim.Tick();
        Assert.Equal(established ? 0 : 1, spawned);
    }

    [Theory]
    [InlineData(0, 217, "save-dropped-pickup-header")]
    [InlineData(4, -1, "save-dropped-pickup-header")]
    [InlineData(8, 2007, "save-dropped-pickup-state")]
    [InlineData(32, 3001, "save-dropped-pickup-value")]
    [InlineData(36, -1, "save-dropped-pickup-value")]
    [InlineData(40, 256, "save-dropped-pickup-value")]
    public void MalformedArchiveIsRejected(int offset, int value, string expectedError)
    {
        var (sim, source) = Room(); sim.SpawnDroppedPickup(source, 2007); var bytes = Save(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expectedError, error);
    }

    [Fact]
    public void ClassMismatchAndConflictingIdentityRejectBeforeMutation()
    {
        var (sim, source) = Room(); sim.SpawnDroppedPickup(source, 2007); var before = Save(sim);
        var state = sim.CaptureState(true); var pose = state.Actors.Single(pose => pose.DroppedPickupIdentity.HasValue);
        pose.DroppedPickupIdentity = pose.DroppedPickupIdentity!.Value with { DoomEdNum = 2001 };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(before, Save(sim));
        state = sim.CaptureState(true); pose = state.Actors.Single(pose => pose.DroppedPickupIdentity.HasValue);
        pose.BotIdentity = new(3001, 0, pose.DroppedPickupIdentity!.Value.JustSpawned);
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state)); Assert.Equal(before, Save(sim));
    }

    [Fact]
    public void ReconstructedDelayedAmmoPickupCanBeCollectedExactlyOnce()
    {
        var (sim, source) = Room(); var player = sim.Players.Single(); player.Inventory.Bullets = 0;
        sim.SpawnDroppedPickup(source, 2007, ignoreAmmoSkill: true, pickupAmount: 9, inventoryToss: true);
        var id = sim.Actors.Single(actor => actor.SpawnedDroppedPickup).Id; var bytes = Save(sim);
        sim.Actors.Single(actor => actor.Id == id).Destroy(); sim.Tick();
        for (var attempt = 0; attempt < 2; attempt++)
        {
            SimSavegame.Apply(sim, bytes);
            for (var i = 0; i < 31; i++) sim.Tick();
            var drop = sim.Actors.Single(actor => actor.Id == id);
            Assert.Equal(0, drop.PickupDelay);
            player.X = drop.X; player.Y = drop.Y;
            sim.Tick(); Assert.Equal(9, player.Inventory.Bullets);
            Assert.DoesNotContain(sim.Actors, actor => actor.Id == id);
            sim.Tick(); Assert.Equal(9, player.Inventory.Bullets);
        }
    }

    [Fact]
    public void IdentityWithoutThinkerOrderStillRestoresPickupLifecycleAndGroup()
    {
        var (sim, source) = Room(); sim.SpawnDroppedPickup(source, 2007);
        var drop = sim.Actors.Single(actor => actor.SpawnedDroppedPickup);
        drop.ThingId = 12; sim.Thinkers.ChangeStatNum(drop, 120);
        var bytes = SimSavegame.Write(sim, true, false);
        drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        var restored = sim.Actors.Single(actor => actor.Id == drop.Id);
        Assert.True(restored.JustSpawned); Assert.Equal(120, restored.StatNum); Assert.Equal(12, restored.ThingId);
        Assert.Equal(bytes, SimSavegame.Write(sim, true, false));
    }

    [Fact]
    public void OrdinarySavesAndParsedLegacyIdentitySavesDoNotAcquirePickupMetadata()
    {
        var (sim, source) = Room(); sim.SpawnDroppedPickup(source, 2007);
        var bytes = SimSavegame.Write(sim); Assert.True(SimSavegame.TryRead(bytes, out var ordinary, out var error), error);
        Assert.All(ordinary.Actors, pose => Assert.Null(pose.DroppedPickupIdentity));
        var state = sim.CaptureState(true);
        foreach (var pose in state.Actors) pose.DroppedPickupIdentity = null;
        var legacy = SimSavegame.Write(state);
        Assert.True(SimSavegame.TryRead(legacy, out var parsed, out error), error);
        Assert.Equal(legacy, SimSavegame.Write(parsed));
    }

    private static (int, int, int, int, uint) Step(AuthoritySimulation sim, uint id)
    {
        sim.Tick(); var actor = sim.Actors.Single(actor => actor.Id == id);
        return (actor.X.Raw, actor.Y.Raw, actor.Z.Raw, actor.PickupDelay, sim.Checksum);
    }
    private static byte[] Save(AuthoritySimulation sim) => SimSavegame.Write(sim, true);
    private static (AuthoritySimulation, BotPawn) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
        });
        var source = sim.AddBot(200, 0); source.Brain!.Enabled = false;
        return (sim, source);
    }
}
