using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PainChanceOverrideSaveTests
{
    [Theory]
    [InlineData(0, 128, 0)]
    [InlineData(0, 0, 256)]
    [InlineData(0, 128, 128)]
    [InlineData(1, 128, 0)]
    [InlineData(1, 0, 256)]
    [InlineData(1, 128, -1)]
    [InlineData(2, 128, 0)]
    [InlineData(2, 0, 256)]
    [InlineData(1, 128, int.MinValue)]
    [InlineData(1, 128, int.MaxValue)]
    public void CurrentValueRestoresIndependentlyOfClassDefault(int kind, int classDefault, int current)
    {
        var (sim, actor) = Room(kind, classDefault); actor.PainChance = current;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        var saved = SimSavegame.Write(sim, true); var (fresh, loaded) = Room(kind, classDefault);
        SimSavegame.Apply(fresh, saved); Assert.Equal(current, loaded.PainChance); Assert.Equal(classDefault, loaded.SpawnPainChance);
        Assert.Equal(checksum, fresh.Checksum); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        actor.PainChance = 17; SimSavegame.Apply(sim, saved); Assert.Equal(current, actor.PainChance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(128)]
    [InlineData(256)]
    public void RestoredOverrideControlsPainAndReplaysSameRandomDraw(int current)
    {
        var (sim, actor) = Room(1, 64); actor.PainChance = current; actor.PainState = 0;
        var saved = SimSavegame.Write(sim, true); actor.PainChance = 17;
        SimSavegame.Apply(sim, saved);
        var random = NativeStateRandom.Seed(42, 0xdae49b5au);
        Assert.Equal((NativeStateRandom.Next(ref random) & 255) < current, actor.TriggerPainChance());
        Assert.Equal(random, sim.CaptureState().PainChanceRandomState);
        SimSavegame.Apply(sim, saved); actor.TriggerPainChance(); Assert.Equal(random, sim.CaptureState().PainChanceRandomState);
    }

    [Fact]
    public void MissingOverrideRestoresPatchedDefaultInsteadOfNewerRuntimeValue()
    {
        var (sim, actor) = Room(1, 100); var saved = SimSavegame.Write(sim, true);
        Assert.Null(sim.CaptureState().Actors.Single(a => a.Id == actor.Id).PainChanceOverride);
        actor.PainChance = 0; SimSavegame.Apply(sim, saved); Assert.Equal(100, actor.PainChance);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void MalformedPresenceMarkerIsRejected(int present)
    {
        var (sim, actor) = Room(0, 100); actor.PainChance = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(355, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), present);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-painchanceoverride-value", error);
    }

    private static (AuthoritySimulation Sim, Actor Actor) Room(int kind, int chance)
    {
        var thing = kind == 1 ? 35 : kind == 2 ? 1 : 2;
        var patch = DehackedPatch.Apply($"Thing {thing}\nPain chance = {chance}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = (ushort)(kind == 0 ? 3004 : 1) }] }, rngSeed: 42, dehacked: patch);
        var owner = sim.Actors.Single(); return (sim, kind == 1 ? sim.SpawnProjectile(owner, ProjectileKind.Plasma) : owner);
    }
}
