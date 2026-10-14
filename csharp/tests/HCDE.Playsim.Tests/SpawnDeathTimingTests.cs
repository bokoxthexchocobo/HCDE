using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpawnDeathTimingTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(8)]
    public void SetTicsControlsPendingDeathCountdown(int tics)
    {
        var sim = Room(); var actor = Target(sim); actor.States.SetTics(tics);
        for (var i = 1; i < Math.Max(1, tics); i++)
        {
            sim.Tick(); Assert.True(actor.PendingSpawnDeath); Assert.Equal(0, actor.DeathCount);
            Assert.Equal(tics - i, actor.States.RemainingTics);
        }
        sim.Tick(); Assert.False(actor.PendingSpawnDeath); Assert.Equal(1, actor.DeathCount);
        Assert.Equal(6, actor.States.RemainingTics);
    }

    [Fact]
    public void MinusOneHoldsPainElementalWithoutPrematureDeathAttack()
    {
        var sim = Room(71); var actor = Target(sim); actor.States.SetTics(-1);
        for (var i = 0; i < 40; i++) sim.Tick();
        Assert.True(actor.PendingSpawnDeath); Assert.Equal(0, actor.DeathCount);
        Assert.DoesNotContain(sim.Actors, candidate => candidate.SpawnedLostSoul);
        actor.States.SetTics(1); sim.Tick(); Assert.Equal(1, actor.DeathCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void PendingTimerSurvivesReconstructionAndReplay(int tics)
    {
        var sim = Room(); var actor = Target(sim); actor.States.SetTics(tics);
        var before = SimSavegame.Write(sim, true);
        var expected = Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray();
        actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, before);
        Assert.True(Target(sim).PendingSpawnDeath); Assert.Equal(tics, Target(sim).States.RemainingTics);
        Assert.Equal(before, SimSavegame.Write(sim, true));
        Assert.Equal(expected, Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray());
    }

    [Fact]
    public void Legacy225PendingStateStillDiesOnFirstTickAndUpgradesLosslessly()
    {
        var sim = Room(); Target(sim).States.SetTics(-1); var bytes = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 225);
        Assert.True(SimSavegame.TryRead(bytes, out var old, out var error), error);
        Assert.True(old.Actors.Single().LegacyImmediateSpawnDeath);
        var upgraded = SimSavegame.Write(old);
        Assert.Equal(226, BinaryPrimitives.ReadUInt16LittleEndian(upgraded.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(upgraded, out var current, out error), error);
        Assert.False(current.Actors.Single().LegacyImmediateSpawnDeath); Assert.Equal(1, current.Actors.Single().StateTics);
        sim.RestoreState(old); sim.Tick(); Assert.Equal(1, Target(sim).DeathCount);
        sim.RestoreState(current); sim.Tick(); Assert.Equal(1, Target(sim).DeathCount);
    }

    [Fact]
    public void ZeroHealthBrightDecorationStartsUnlitWithOneTic()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 44, Health = 0 }]
        });
        var actor = sim.Actors.Single(); Assert.True(actor.PendingSpawnDeath);
        Assert.Equal(1, actor.States.RemainingTics); Assert.False(actor.FullBright);
        var before = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, before);
        Assert.False(sim.Actors.Single().FullBright); Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    private static uint Step(AuthoritySimulation sim) { sim.Tick(); return sim.Checksum; }
    private static Actor Target(AuthoritySimulation sim) => sim.Actors.Single(actor => actor.MapMonsterThingIndex.HasValue);
    private static AuthoritySimulation Room(int type = 3004) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = type, Health = 0 }] });
}
