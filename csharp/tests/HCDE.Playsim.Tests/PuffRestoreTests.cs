using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PuffRestoreTests
{
    [Fact]
    public void OrdinaryRestoreDoesNotRemoveLaterPuffs()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        var puff = sim.SpawnHitscanPuff(128, 0, 24);
        SimSavegame.Apply(sim, bytes);
        Assert.Contains(puff, sim.Actors); Assert.False(puff.Destroyed);
    }

    [Fact]
    public void DestroyedSourceIsArchivedAsNull()
    {
        var sim = Room(); var source = sim.AddBot(200, 0);
        var puff = sim.SpawnHitscanPuff(128, 0, 24); puff.DamageSource = source; source.Destroy();
        var bytes = Save(sim); puff.Destroy(); SimSavegame.Apply(sim, bytes);
        Assert.Null(sim.Actors.OfType<PuffActor>().Single().DamageSource);
        Assert.Equal(bytes, Save(sim));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    [InlineData(6, false)]
    [InlineData(6, true)]
    [InlineData(10, false)]
    [InlineData(10, true)]
    [InlineData(12, false)]
    [InlineData(12, true)]
    public void ReconstructsEachAnimationFrameAndReplaysExpiration(int ticks, bool remove)
    {
        var sim = Room(); var puff = sim.SpawnHitscanPuff(128, 0, 24, 77);
        puff.DamageSource = sim.Players.Single(); sim.Thinkers.ChangeStatNum(puff, 120);
        for (var i = 0; i < ticks; i++) sim.Tick();
        Assert.False(puff.Destroyed);
        var before = Save(sim); Assert.Equal(222, BinaryPrimitives.ReadUInt16LittleEndian(before.AsSpan(4)));
        var expected = Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray();
        SimSavegame.Apply(sim, before); puff = sim.Actors.OfType<PuffActor>().Single();
        puff.Destroy(); if (remove) sim.Tick();
        SimSavegame.Apply(sim, before);
        var restored = sim.Actors.OfType<PuffActor>().Single();
        Assert.Equal(77, restored.ThingId); Assert.Equal(120, restored.StatNum);
        Assert.Same(sim.Players.Single(), restored.DamageSource);
        Assert.Equal(before, Save(sim));
        Assert.Equal(expected, Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray());
        Assert.Empty(sim.Actors.OfType<PuffActor>());
    }

    [Fact]
    public void ReconnectsReconstructedSourceAndProjectileOwner()
    {
        var sim = Room(); var source = sim.AddBot(200, 0); source.Brain!.Enabled = false;
        var puff = sim.SpawnHitscanPuff(128, 0, 24); puff.DamageSource = source;
        var missile = sim.SpawnProjectile(puff, ProjectileKind.Plasma); var before = Save(sim);
        source.Destroy(); puff.Destroy(); missile.Destroy(); sim.Tick();
        SimSavegame.Apply(sim, before);
        var restored = sim.Actors.OfType<PuffActor>().Single();
        Assert.Same(sim.Actors.Single(actor => actor.Id == source.Id), restored.DamageSource);
        Assert.Same(restored, sim.Actors.OfType<ProjectileActor>().Single().Owner);
        Assert.Equal(before, Save(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoreDoesNotPublishSpawnUntilSavedFreshTick(bool established)
    {
        var sim = Room(); var puff = sim.SpawnHitscanPuff(128, 0, 24);
        if (established) sim.Tick(); var before = Save(sim);
        puff.Destroy(); sim.Tick(); var events = 0;
        sim.WorldThingSpawned += actor => { if (actor.Id == puff.Id) events++; };
        SimSavegame.Apply(sim, before); Assert.Equal(0, events); sim.Tick();
        Assert.Equal(established ? 0 : 1, events);
    }

    [Fact]
    public void OrdinarySaveFormatAndIdentityWithoutThinkerOrderRemainSupported()
    {
        var sim = Room(); var puff = sim.SpawnHitscanPuff(128, 0, 24);
        var ordinary = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(ordinary, out var parsed, out var error), error);
        Assert.All(parsed.Actors, pose => Assert.Null(pose.PuffIdentity));
        Assert.Equal(ordinary, SimSavegame.Write(parsed));
        var identity = SimSavegame.Write(sim, true, false); puff.Destroy(); sim.Tick();
        SimSavegame.Apply(sim, identity); Assert.Equal(identity, SimSavegame.Write(sim, true, false));
    }

    [Theory]
    [InlineData(0, 219, "save-puff-header")]
    [InlineData(4, -1, "save-puff-header")]
    [InlineData(20, -2, "save-puff-value")]
    [InlineData(24, 256, "save-puff-value")]
    public void RejectsMalformedTrailer(int offset, int value, string expected)
    {
        var sim = Room(); sim.SpawnHitscanPuff(128, 0, 24); var bytes = Save(sim);
        var innerEnd = bytes.Length - 16;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(innerEnd - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(innerEnd - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
    }

    [Fact]
    public void InvalidSourceIsRejectedBeforeMutation()
    {
        var sim = Room(); sim.SpawnHitscanPuff(128, 0, 24); var before = Save(sim);
        var state = sim.CaptureState(true); var pose = state.Actors.Single(actor => actor.PuffIdentity.HasValue);
        pose.PuffIdentity = pose.PuffIdentity!.Value with { DamageSourceId = uint.MaxValue };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state)); Assert.Equal(before, Save(sim));
    }

    private static byte[] Save(AuthoritySimulation sim) => SimSavegame.Write(sim, true);
    private static uint Step(AuthoritySimulation sim) { sim.Tick(); return sim.Checksum; }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
