using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapPickupRestoreTests
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
    public void ReconstructsMapPickupAndReplaysWithoutConsumingSpawnRandom(int type, bool established, bool remove)
    {
        var sim = Room(type); var item = Item(sim); item.ThingId = 99;
        sim.Thinkers.ChangeStatNum(item, 120);
        if (established) sim.Tick();
        var before = Save(sim); Assert.Equal(218, BinaryPrimitives.ReadUInt16LittleEndian(before.AsSpan(4)));
        var expected = Enumerable.Range(0, 12).Select(_ => Step(sim)).ToArray();
        item.Destroy(); if (remove) sim.Tick();
        SimSavegame.Apply(sim, before);
        var restored = Item(sim);
        Assert.Equal(item.Id, restored.Id); Assert.Equal(2, restored.MapPickupThingIndex);
        Assert.Equal(99, restored.ThingId); Assert.Equal(120, restored.StatNum); Assert.False(restored.Dropped);
        Assert.Equal(before, Save(sim)); Assert.Equal(expected, Enumerable.Range(0, 12).Select(_ => Step(sim)).ToArray());
    }

    [Fact]
    public void CollectionRewindAndCollectedSnapshotRestoreDoNotDuplicateAmmo()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.Bullets = 0;
        sim.Rewind.Capture(sim); var before = Save(sim); var id = Item(sim).Id;
        player.X = Fixed.FromInt(200); sim.Tick();
        Assert.Equal(10, player.Inventory.Bullets); Assert.DoesNotContain(sim.Actors, actor => actor.Id == id);
        var collected = Save(sim);
        Assert.True(sim.Rewind.RestoreOldest(sim)); Assert.Equal(before, Save(sim));
        SimSavegame.Apply(sim, collected);
        Assert.DoesNotContain(sim.Actors, actor => actor.Id == id); Assert.Equal(collected, Save(sim));
        Assert.True(sim.Rewind.RestoreOldest(sim)); player.X = Fixed.FromInt(200); sim.Tick();
        Assert.Equal(10, player.Inventory.Bullets); sim.Tick(); Assert.Equal(10, player.Inventory.Bullets);
    }

    [Fact]
    public void ReconstructionUsesPatchedAndMapSpawnDefaults()
    {
        var patch = DehackedPatch.Apply("Thing 5\nID # = 2007\nHit points = 88\nWidth = 786432\nHeight = 1572864\nMass = 123\nReaction time = 17\nBits = 513\n");
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing
                { Type = 2007, X = 200, Z = 16, Angle = 45, Pitch = 12, Roll = 7, Gravity = 0.5, Health = 2, Friendly = true, Ambush = true }]
        }, dehacked: patch);
        var item = Item(sim); var bytes = Save(sim);
        item.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        var restored = Item(sim);
        Assert.Equal(176, restored.ResurrectionHealth); Assert.Equal(Fixed.FromInt(12), restored.ResurrectionRadius);
        Assert.Equal(Fixed.FromInt(24), restored.ResurrectionHeight); Assert.Equal(123, restored.Mass);
        Assert.Equal(17, restored.SpawnReactionTime); Assert.True(restored.Friendly); Assert.True(restored.Ambush);
        Assert.Equal(16, restored.SpawnZOffset); Assert.Equal(bytes, Save(sim));
    }

    [Fact]
    public void PointerReferencesAndProjectileOwnerResolveToReconstructedPickup()
    {
        var sim = Room(); var item = Item(sim); var player = sim.Players.Single();
        player.TracerId = item.Id; item.MasterId = player.Id;
        var missile = sim.SpawnProjectile(item, ProjectileKind.Plasma); var before = Save(sim);
        item.Destroy(); missile.Destroy(); sim.Tick(); SimSavegame.Apply(sim, before);
        Assert.Equal(item.Id, player.TracerId); Assert.Equal(player.Id, Item(sim).MasterId);
        Assert.Same(Item(sim), sim.Actors.OfType<ProjectileActor>().Single(actor => actor.Id == missile.Id).Owner);
        Assert.Equal(before, Save(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpawnNotificationsRespectSavedLifecycle(bool established)
    {
        var sim = Room(); if (established) sim.Tick(); var bytes = Save(sim);
        var item = Item(sim); item.Destroy(); sim.Tick(); var events = 0;
        sim.WorldThingSpawned += actor => { if (actor.Id == item.Id) events++; };
        SimSavegame.Apply(sim, bytes); Assert.Equal(0, events); sim.Tick();
        Assert.Equal(established ? 0 : 1, events);
    }

    [Theory]
    [InlineData(0, 218, "save-map-pickup-header")]
    [InlineData(4, -1, "save-map-pickup-header")]
    [InlineData(24, -2, "save-map-pickup-value")]
    [InlineData(28, 3001, "save-map-pickup-value")]
    [InlineData(32, -1, "save-map-pickup-value")]
    [InlineData(36, 256, "save-map-pickup-value")]
    public void MalformedArchiveIsRejected(int offset, int value, string expectedError)
    {
        var bytes = Save(Room()); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expectedError, error);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WrongOriginOrClassRejectsBeforeMutation(bool wrongClass)
    {
        var sim = Room(); var before = Save(sim); var state = sim.CaptureState(true);
        var pose = state.Actors.Single(pose => pose.MapPickupIdentity.HasValue); var identity = pose.MapPickupIdentity!.Value;
        pose.MapPickupIdentity = wrongClass ? identity with { DoomEdNum = 2001 } : identity with { ThingIndex = int.MaxValue };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(before, Save(sim));
    }

    [Fact]
    public void TwoSavedActorsCannotClaimTheSameMapEntry()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2007, X = 200 }, new LevelThing { Type = 2007, X = 400 }]
        });
        var before = Save(sim); var state = sim.CaptureState(true);
        var poses = state.Actors.Where(pose => pose.MapPickupIdentity.HasValue).ToArray();
        poses[1].MapPickupIdentity = poses[1].MapPickupIdentity!.Value with { ThingIndex = poses[0].MapPickupIdentity!.Value.ThingIndex };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state)); Assert.Equal(before, Save(sim));
    }

    [Fact]
    public void IdentityWithoutThinkerOrderAndOrdinarySavesRemainSupported()
    {
        var sim = Room(); var ordinary = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(ordinary, out var parsed, out var error), error);
        Assert.All(parsed.Actors, pose => Assert.Null(pose.MapPickupIdentity)); Assert.Equal(ordinary, SimSavegame.Write(parsed));
        var bytes = SimSavegame.Write(sim, true, false); Item(sim).Destroy(); sim.Tick();
        SimSavegame.Apply(sim, bytes); Assert.Equal(bytes, SimSavegame.Write(sim, true, false));
    }

    private static uint Step(AuthoritySimulation sim) { sim.Tick(); return sim.Checksum; }
    private static byte[] Save(AuthoritySimulation sim) => SimSavegame.Write(sim, true);
    private static Actor Item(AuthoritySimulation sim) => sim.Actors.Single(actor => actor.MapPickupThingIndex.HasValue);
    private static AuthoritySimulation Room(int type = 2007) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 0 }, new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 200, Id = 77 }]
    });
}
