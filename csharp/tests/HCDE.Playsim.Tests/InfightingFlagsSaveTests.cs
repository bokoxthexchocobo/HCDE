using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InfightingFlagsSaveTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(4, false)]
    [InlineData(8, false)]
    [InlineData(15, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(4, true)]
    [InlineData(8, true)]
    [InlineData(15, true)]
    public void FlagsSurviveSaveAndIdentityReconstruction(int flags, bool reconstruct)
    {
        var sim = Room(); var actor = sim.Actors.Single(); Set(actor, flags);
        var saved = SimSavegame.Write(sim, true);
        if (reconstruct) { actor.Destroy(); sim.Tick(); } else Set(actor, flags ^ 15);
        SimSavegame.Apply(sim, saved); actor = sim.Actors.Single(a => !a.Destroyed);
        Assert.Equal(flags, Flags(actor)); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(false, false, -1)]
    [InlineData(false, true, 0)]
    [InlineData(true, true, -1)]
    public void RestoredFlagsRetainNativeInfightLevelPriority(bool noInfighting, bool forceInfighting, int expected)
    {
        var sim = Room(); var actor = sim.Actors.Single(); sim.Infighting = -1;
        actor.NoInfighting = noInfighting; actor.ForceInfighting = forceInfighting;
        var saved = SimSavegame.Write(sim); actor.NoInfighting = actor.ForceInfighting = false;
        SimSavegame.Apply(sim, saved); Assert.Equal(expected, sim.GetInfightLevel(actor));
    }

    [Fact]
    public void OlderSnapshotClearsLaterOverrides()
    {
        var sim = Room(); var actor = sim.Actors.Single(); var saved = SimSavegame.Write(sim);
        Set(actor, 15); SimSavegame.Apply(sim, saved); Assert.Equal(0, Flags(actor));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(16)]
    public void InvalidStructuredBitsRejectBeforeMutation(int flags)
    {
        var sim = Room(); var actor = sim.Actors.Single(); var state = sim.CaptureState();
        state.Actors[0].InfightingFlags = flags; actor.Health = 42;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(42, actor.Health);
    }

    [Theory]
    [InlineData(0, 238, "save-infighting-header")]
    [InlineData(4, int.MaxValue, "save-infighting-header")]
    [InlineData(8, 16, "save-infighting-value")]
    [InlineData(8, -2, "save-infighting-value")]
    public void MalformedWireFlagsAreRejected(int offset, int value, string expected)
    {
        var sim = Room(); Set(sim.Actors.Single(), 15); var saved = SimSavegame.Write(sim);
        var start = ArchiveLayerFixture.TrailerStart(saved, 238);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal(expected, error);
    }

    private static int Flags(Actor actor) => (actor.NoInfighting ? 1 : 0) | (actor.ForceInfighting ? 2 : 0)
        | (actor.NoInfightSpecies ? 4 : 0) | (actor.DoHarmSpecies ? 8 : 0);
    private static void Set(Actor actor, int flags)
    {
        actor.NoInfighting = (flags & 1) != 0; actor.ForceInfighting = (flags & 2) != 0;
        actor.NoInfightSpecies = (flags & 4) != 0; actor.DoHarmSpecies = (flags & 8) != 0;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }]
    });
}
