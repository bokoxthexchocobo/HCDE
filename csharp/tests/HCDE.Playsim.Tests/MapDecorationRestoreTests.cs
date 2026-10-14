using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapDecorationRestoreTests
{
    [Theory]
    [InlineData(44, 1)]
    [InlineData(44, 2)]
    [InlineData(44, 3)]
    [InlineData(44, 4)]
    [InlineData(49, 3)]
    [InlineData(23, 1)]
    public void ReconstructsDecorationSpecificAnimationAndCrushFrames(int type, int frame)
    {
        var sim = Room(type); var actor = Decoration(sim); actor.States.Enter(actor, frame);
        var before = Save(sim); var expected = Enumerable.Range(0, 25).Select(_ => Step(sim)).ToArray();
        actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, before);
        Assert.Equal(frame, Decoration(sim).States.Current); Assert.Equal(before, Save(sim));
        Assert.Equal(expected, Enumerable.Range(0, 25).Select(_ => Step(sim)).ToArray());
    }

    [Fact]
    public void RebuildsPatchedDefaultsAndMapSpawnSettings()
    {
        var patch = DehackedPatch.Apply("Thing 5\nID # = 44\nHit points = 88\nWidth = 786432\nHeight = 1572864\nMass = 123\nReaction time = 17\nBits = 512\n");
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing
                { Type = 44, X = 200, Z = 16, Health = 2, Gravity = 0.5, Friendly = true, Ambush = true }]
        }, dehacked: patch);
        var before = Save(sim); Decoration(sim).Destroy(); sim.Tick(); SimSavegame.Apply(sim, before);
        var restored = Decoration(sim);
        Assert.Equal(176, restored.ResurrectionHealth); Assert.Equal(Fixed.FromInt(12), restored.ResurrectionRadius);
        Assert.Equal(Fixed.FromInt(24), restored.ResurrectionHeight); Assert.Equal(123, restored.Mass);
        Assert.Equal(17, restored.SpawnReactionTime); Assert.Equal(16, restored.SpawnZOffset);
        Assert.True(restored.Friendly); Assert.True(restored.Ambush); Assert.Equal(before, Save(sim));
    }

    [Theory]
    [InlineData(2028, false, false)]
    [InlineData(2028, false, true)]
    [InlineData(2028, true, false)]
    [InlineData(2028, true, true)]
    [InlineData(44, false, false)]
    [InlineData(44, false, true)]
    [InlineData(44, true, false)]
    [InlineData(44, true, true)]
    [InlineData(49, false, false)]
    [InlineData(49, false, true)]
    [InlineData(49, true, false)]
    [InlineData(49, true, true)]
    [InlineData(24, false, false)]
    [InlineData(24, false, true)]
    [InlineData(24, true, false)]
    [InlineData(24, true, true)]
    [InlineData(23, false, false)]
    [InlineData(23, false, true)]
    [InlineData(23, true, false)]
    [InlineData(23, true, true)]
    public void ReconstructsMapDefaultsAndReplays(int type, bool established, bool remove)
    {
        var sim = Room(type); var actor = Decoration(sim); actor.ThingId = 99;
        sim.Thinkers.ChangeStatNum(actor, 120); if (established) sim.Tick();
        var height = actor.Height; var radius = actor.Radius; var ceiling = actor.SpawnCeiling;
        var before = Save(sim); Assert.Equal(220, BinaryPrimitives.ReadUInt16LittleEndian(before.AsSpan(4)));
        var expected = Enumerable.Range(0, 25).Select(_ => Step(sim)).ToArray();
        SimSavegame.Apply(sim, before); actor = Decoration(sim);
        actor.Destroy(); if (remove) sim.Tick(); SimSavegame.Apply(sim, before);
        var restored = Decoration(sim);
        Assert.Equal(2, restored.MapDecorationThingIndex); Assert.Equal(actor.Id, restored.Id);
        Assert.Equal(99, restored.ThingId); Assert.Equal(120, restored.StatNum);
        Assert.Equal(height, restored.Height); Assert.Equal(radius, restored.Radius); Assert.Equal(ceiling, restored.SpawnCeiling);
        Assert.Equal(before, Save(sim)); Assert.Equal(expected, Enumerable.Range(0, 25).Select(_ => Step(sim)).ToArray());
    }

    [Fact]
    public void MixedIdentitiesRestorePointersAndProjectileOwnership()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 49, X = 200 }, new LevelThing { Type = 2007, X = 300 }]
        });
        var decoration = Decoration(sim); var pickup = sim.Actors.Single(actor => actor.MapPickupThingIndex.HasValue);
        decoration.MasterId = pickup.Id;
        var puff = sim.SpawnHitscanPuff(100, 0, 20); puff.DamageSource = decoration;
        var missile = sim.SpawnProjectile(decoration, ProjectileKind.Plasma);
        var before = Save(sim);
        decoration.Destroy(); pickup.Destroy(); puff.Destroy(); missile.Destroy(); sim.Tick();
        SimSavegame.Apply(sim, before);
        var restored = Decoration(sim);
        Assert.Equal(pickup.Id, restored.MasterId);
        Assert.Same(restored, sim.Actors.OfType<PuffActor>().Single().DamageSource);
        Assert.Same(restored, sim.Actors.OfType<ProjectileActor>().Single().Owner);
        Assert.Equal(before, Save(sim));
    }

    [Fact]
    public void SnapshotWithoutDecorationRemovesReconstructedDecoration()
    {
        var sim = Room(); var before = Save(sim); Decoration(sim).Destroy(); sim.Tick();
        var after = Save(sim); SimSavegame.Apply(sim, before); Assert.NotNull(Decoration(sim));
        SimSavegame.Apply(sim, after); Assert.DoesNotContain(sim.Actors, actor => actor.MapDecorationThingIndex.HasValue);
        Assert.Equal(after, Save(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NotificationsRespectSavedLifecycle(bool established)
    {
        var sim = Room(); if (established) sim.Tick(); var before = Save(sim);
        var id = Decoration(sim).Id; Decoration(sim).Destroy(); sim.Tick(); var events = 0;
        sim.WorldThingSpawned += actor => { if (actor.Id == id) events++; };
        SimSavegame.Apply(sim, before); Assert.Equal(0, events); sim.Tick();
        Assert.Equal(established ? 0 : 1, events);
    }

    [Theory]
    [InlineData(0, 220, "save-map-decoration-header")]
    [InlineData(4, -1, "save-map-decoration-header")]
    [InlineData(24, -2, "save-map-decoration-value")]
    [InlineData(28, 3001, "save-map-decoration-value")]
    [InlineData(32, -1, "save-map-decoration-value")]
    [InlineData(36, 256, "save-map-decoration-value")]
    public void RejectsMalformedTrailer(int offset, int value, string expected)
    {
        var bytes = Save(Room()); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RejectsWrongMapIdentityBeforeMutation(bool origin)
    {
        var sim = Room(); var before = Save(sim); var state = sim.CaptureState(true);
        var pose = state.Actors.Single(actor => actor.MapDecorationIdentity.HasValue);
        var identity = pose.MapDecorationIdentity!.Value;
        pose.MapDecorationIdentity = origin ? identity with { ThingIndex = int.MaxValue } : identity with { DoomEdNum = 49 };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(before, Save(sim));
    }

    [Fact]
    public void OrdinaryAndIdentityWithoutThinkerOrderRemainSupported()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.All(state.Actors, pose => Assert.Null(pose.MapDecorationIdentity)); Assert.Equal(bytes, SimSavegame.Write(state));
        bytes = SimSavegame.Write(sim, true, false); Decoration(sim).Destroy(); sim.Tick();
        SimSavegame.Apply(sim, bytes); Assert.Equal(bytes, SimSavegame.Write(sim, true, false));
    }

    private static byte[] Save(AuthoritySimulation sim) => SimSavegame.Write(sim, true);
    private static uint Step(AuthoritySimulation sim) { sim.Tick(); return sim.Checksum; }
    private static Actor Decoration(AuthoritySimulation sim) => sim.Actors.Single(actor => actor.MapDecorationThingIndex.HasValue);
    private static AuthoritySimulation Room(int type = 44) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 0 }, new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 200, Id = 77 }]
    });
}
