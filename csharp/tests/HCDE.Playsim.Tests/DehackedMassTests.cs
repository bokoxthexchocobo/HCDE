using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedMassTests
{
    [Theory]
    [InlineData(250)]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(int.MaxValue)]
    public void ExplicitMassInitializesActorWithoutClamping(int mass)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nMass = {mass}\n");
        Assert.True(patch.Actors.Single(actor => actor.Index == 2).MassPatched);
        Assert.Equal(mass, Spawn(patch).Mass);
    }

    [Fact]
    public void UnrelatedPatchRetainsCatalogMass()
    {
        var patch = DehackedPatch.Apply("Thing 2\nHit points = 88\n");
        Assert.False(patch.Actors.Single(actor => actor.Index == 2).MassPatched);
        Assert.Equal(DoomActorCatalog.MassOf(3004), Spawn(patch).Mass);
    }

    [Fact]
    public void ChainingPreservesAssignmentAndDoesNotMutateBaseline()
    {
        var first = DehackedPatch.Apply("Thing 2\nMass = 250\n");
        var second = DehackedPatch.Apply("Thing 2\nHit points = 88\n", first);
        var third = DehackedPatch.Apply("Thing 2\nMass = 0\n", second);
        Assert.Equal(250, Spawn(first).Mass);
        Assert.Equal(250, Spawn(second).Mass);
        Assert.Equal(0, Spawn(third).Mass);
    }

    private static Actor Spawn(DehackedPatchResult patch) => Assert.Single(AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch).Actors);
}
