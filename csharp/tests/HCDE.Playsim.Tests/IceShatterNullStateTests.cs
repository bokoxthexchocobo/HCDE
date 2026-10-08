using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceShatterNullStateTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void ConfiguredNullLabelRunsAfterUnblockingAndUsesItsDuration(int tics)
    {
        var sim = Room(); var corpse = sim.Actors.Single(); corpse.Brain = null;
        corpse.NullState = 1; var calls = 0;
        corpse.States.Configure(corpse, [new(-1, 0), new(tics, -1, self =>
        {
            calls++; Assert.False(self.Solid); Assert.True(self.Shootable);
            Assert.NotEmpty(sim.Actors.OfType<IceChunkActor>());
        })], 0);
        sim.SpawnIceChunks(corpse);
        Assert.False(corpse.Destroyed); Assert.Equal(1, calls);
        Assert.Equal(1, corpse.States.Current); Assert.Equal(tics, corpse.States.RemainingTics);
        corpse.States.Tick(corpse); Assert.False(corpse.Destroyed);
        corpse.States.Tick(corpse); Assert.Equal(tics != -1, corpse.Destroyed);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void NullLabelActionCanRedirectBeforeRemoval()
    {
        var sim = Room(); var corpse = sim.Actors.Single(); corpse.Brain = null; corpse.NullState = 1;
        corpse.States.Configure(corpse, [new(-1, 0), new(0, -1, StateAction: _ => 2), new(3, -1)], 0);
        sim.SpawnIceChunks(corpse);
        Assert.False(corpse.Destroyed); Assert.Equal(2, corpse.States.Current);
        Assert.Equal(3, corpse.States.RemainingTics);
    }

    [Fact]
    public void MissingNullLabelRetainsImmediateRemoval()
    {
        var sim = Room(); var corpse = sim.Actors.Single();
        Assert.Equal(-1, corpse.NullState);
        sim.SpawnIceChunks(corpse);
        Assert.True(corpse.Destroyed); Assert.Equal(-1, corpse.States.Current);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 3004 }] }, rngSeed: 42);
}
