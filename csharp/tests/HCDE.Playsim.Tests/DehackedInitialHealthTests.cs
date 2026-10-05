using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedInitialHealthTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void InitialHealthAppliesToEveryPlayerStart(int type)
    {
        var patch = DehackedPatch.Apply("Misc 0\nInitial Health = 150\n");
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = type }] }, dehacked: patch);
        var player = sim.Players.Single(); Assert.Equal(150, player.Health);
        Assert.Equal(150, player.SpawnHealth()); Assert.Equal(100, player.GetMaxHealth(false));
    }

    [Theory]
    [InlineData("Thing 1\nHit points = 80\n\nMisc 0\nInitial Health = 150\n", 150)]
    [InlineData("Misc 0\nInitial Health = 150\n\nThing 1\nHit points = 80\n", 80)]
    public void PlayerDefaultsRespectPatchOrder(string text, int expected) =>
        Assert.Equal(expected, DehackedPatch.Apply(text).Actors.Single(actor => actor.Index == 1).Health);

    [Fact]
    public void BaselinePreservesInitialHealthAndReportsInvalidInput()
    {
        var baseline = DehackedPatch.Apply("Misc 0\nInitial Health = 150\n");
        var patch = DehackedPatch.Apply("Misc 0\nInitial Health = invalid\n", baseline);
        Assert.NotEmpty(patch.Errors); Assert.Equal(150, patch.InitialHealth);
        Assert.Equal(150, patch.Actors.Single(actor => actor.Index == 1).Health);
    }
}
