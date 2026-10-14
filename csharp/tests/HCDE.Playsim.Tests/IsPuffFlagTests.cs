using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IsPuffFlagTests
{
    [Theory]
    [InlineData("ISPUFF")]
    [InlineData("ispuff")]
    [InlineData("Actor.IsPuff")]
    public void LegacyFlagHandlerReadsSetsAndClearsMarker(string name)
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TryGet(actor, name, out var initial)); Assert.False(initial);
        Assert.True(AcsActorFlags.TrySet(actor, name, true)); Assert.True(actor.IsPuff);
        Assert.True(AcsActorFlags.TryGet(actor, name, out var marked)); Assert.True(marked);
        Assert.True(AcsActorFlags.TrySet(actor, name, false)); Assert.False(actor.IsPuff);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void OrdinaryAndIdentitySavesRestorePlainActorMarker(bool marker, bool identities)
    {
        var sim = Room(); var actor = sim.Players.Single(); AcsActorFlags.TrySet(actor, "ISPUFF", marker);
        var before = SimSavegame.Write(sim, identities);
        if (marker) Assert.Equal(224, BinaryPrimitives.ReadUInt16LittleEndian(before.AsSpan(4)));
        AcsActorFlags.TrySet(actor, "ISPUFF", !marker); SimSavegame.Apply(sim, before);
        Assert.Equal(marker, actor.IsPuff); Assert.Equal(before, SimSavegame.Write(sim, identities));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClearedSpawnedPuffMarkerRestoresOnExistingOrRebuiltActor(bool reconstruct)
    {
        var sim = Room(); var puff = sim.SpawnHitscanPuff(100, 0, 24);
        Assert.True(AcsActorFlags.TrySet(puff, "ISPUFF", false)); var before = SimSavegame.Write(sim, true);
        if (reconstruct) { puff.Destroy(); sim.Tick(); } else AcsActorFlags.TrySet(puff, "ISPUFF", true);
        SimSavegame.Apply(sim, before); var restored = sim.Actors.OfType<PuffActor>().Single();
        Assert.False(restored.IsPuff); Assert.True(restored.DefaultIsPuff); Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void MarkedMonsterReconstructsWithMarkerOverride()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0); AcsActorFlags.TrySet(actor, "ISPUFF", true);
        var before = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, before);
        var restored = sim.Actors.Single(candidate => candidate.Id == actor.Id);
        Assert.True(restored.IsPuff); Assert.False(restored.DefaultIsPuff); Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void SnapshotWithoutOverridesRestoresBothEngineBaselines()
    {
        var sim = Room(); var actor = sim.Players.Single(); var puff = sim.SpawnHitscanPuff(100, 0, 24);
        var before = SimSavegame.Write(sim, true);
        AcsActorFlags.TrySet(actor, "ISPUFF", true); AcsActorFlags.TrySet(puff, "ISPUFF", false);
        SimSavegame.Apply(sim, before);
        Assert.False(actor.IsPuff); Assert.True(puff.IsPuff); Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScriptMutationControlsSpectralAllyFilter(bool spawnAsPuff)
    {
        var sim = Room(); var inflictor = spawnAsPuff ? sim.SpawnHitscanPuff(100, 0, 24) : new Actor();
        inflictor.Spectral = true; var target = new Actor { Health = 100, Spectral = true };
        AcsActorFlags.TrySet(inflictor, "ISPUFF", true);
        Assert.Equal(20, ActorDamage.Apply(target, 20, inflictor: inflictor).HealthLost);
        AcsActorFlags.TrySet(inflictor, "ISPUFF", false);
        Assert.Equal(0, ActorDamage.Apply(target, 20, inflictor: inflictor).HealthLost);
    }

    [Theory]
    [InlineData(0, 224, "save-ispuff-header")]
    [InlineData(4, -1, "save-ispuff-header")]
    [InlineData(8, 2, "save-ispuff-value")]
    public void RejectsMalformedMarkerArchive(int offset, int value, string expected)
    {
        var sim = Room(); AcsActorFlags.TrySet(sim.Players.Single(), "ISPUFF", true);
        var bytes = SimSavegame.Write(sim, true); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
    }

    [Fact]
    public void InvalidStructuredMarkerIsRejectedBeforeMutation()
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true); var state = sim.CaptureState(true);
        state.Actors.Single().IsPuffFlag = 2;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ClearedMarkerReplaysAfterReconstruction()
    {
        var sim = Room(); var puff = sim.SpawnHitscanPuff(100, 0, 24); AcsActorFlags.TrySet(puff, "ISPUFF", false);
        var before = SimSavegame.Write(sim, true); var expected = Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray();
        SimSavegame.Apply(sim, before); Assert.False(sim.Actors.OfType<PuffActor>().Single().IsPuff);
        Assert.Equal(expected, Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray());
    }

    private static uint Step(AuthoritySimulation sim) { sim.Tick(); return sim.Checksum; }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
