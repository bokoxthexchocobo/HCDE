using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GibHealthArchiveTests
{
    [Theory]
    [InlineData(37, false)]
    [InlineData(-37, false)]
    [InlineData(0, false)]
    [InlineData(int.MaxValue, false)]
    [InlineData(37, true)]
    [InlineData(-37, true)]
    public void OverrideSurvivesExistingAndReconstructedActors(int threshold, bool reconstruct)
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.IsMonster); actor.GibHealth = threshold;
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(229, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        if (reconstruct) { actor.Destroy(); sim.Tick(); } else actor.GibHealth = 999;
        SimSavegame.Apply(sim, saved); actor = sim.Actors.Single(a => a.IsMonster);
        Assert.Equal(threshold, actor.GibHealth); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void SentinelOverrideOnPuffIsDistinctFromAbsentMetadata()
    {
        var sim = Room(); var puff = sim.SpawnHitscanPuff(200, 0, 20); puff.GibHealth = int.MinValue;
        var saved = SimSavegame.Write(sim, true); puff.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal(int.MinValue, sim.Actors.OfType<PuffActor>().Single().GibHealth);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void OlderSaveResetsLaterThresholdToFactoryDefault()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.IsMonster); var saved = SimSavegame.Write(sim);
        actor.GibHealth = 37; SimSavegame.Apply(sim, saved);
        Assert.Equal(int.MinValue, actor.GibHealth); Assert.Equal(-20, actor.GetGibHealth());
        Assert.Equal(saved, SimSavegame.Write(sim));
    }

    [Fact]
    public void SavedThresholdChangesSubsequentDeathSelection()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.IsMonster); actor.GibHealth = 37;
        var saved = SimSavegame.Write(sim); actor.GibHealth = 1; SimSavegame.Apply(sim, saved);
        actor.ExtremeDeathState = ActorStateMachine.Corpse;
        ActorDamage.Apply(actor, 50); Assert.Equal(ActorStateMachine.Death, actor.States.Current);
    }

    [Fact]
    public void InvalidWireThresholdIsRejected()
    {
        var sim = Room(); sim.Players.Single().GibHealth = 37; var saved = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt64LittleEndian(saved.AsSpan(saved.Length - size + 8), (long)int.MaxValue + 1);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-gibhealth-value", error);
    }

    [Fact]
    public void OverrideWithoutGeometryRejectsWriteAndRestore()
    {
        var sim = Room(); var state = sim.CaptureState(); state.GeometryHealth = null;
        state.Actors[0].GibHealthOverride = 37;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = -400 }, new LevelThing { Type = 3004 }]
    });
}
