using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsTeleFogPolicyTests
{
    [Theory]
    [InlineData("", "NewDest", "OldSource", "NewDest")]
    [InlineData("NewSource", "", "NewSource", "OldDest")]
    [InlineData("", "", "OldSource", "OldDest")]
    public void EmptyNamesPreserveExistingSetting(string source, string dest, string expectedSource, string expectedDest)
    {
        var sim = Room(); var player = sim.Players.Single(); player.TeleFogSource = "OldSource"; player.TeleFogDest = "OldDest";
        var pool = new AcsGlobalStrings(); var stack = new List<int> { 0, pool.Add(source), pool.Add(dest) };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = player }, [], pool,
            AcsCallFunctions.SetActorTeleFog, 3, out var result));
        Assert.Equal(0, result); Assert.Equal(expectedSource, player.TeleFogSource); Assert.Equal(expectedDest, player.TeleFogDest);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("SameFog", "samefog")]
    public void IdenticalTypesDoNotCountAsSwaps(string? source, string? dest)
    {
        var sim = Room(); var player = sim.Players.Single(); player.TeleFogSource = source; player.TeleFogDest = dest;
        Assert.Equal(0, AcsActorTeleFog.Swap(sim, 0, player)); Assert.False(player.HasTeleFogOverride);
    }

    [Fact]
    public void TidSwapCountsOnlyActorsWhoseTypesDiffer()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ThingId = 9;
        var actor = sim.AddBot(80, 0, thingId: 9); actor.TeleFogSource = "Source"; actor.TeleFogDest = "Dest";
        Assert.Equal(1, AcsActorTeleFog.Swap(sim, 9, null)); Assert.Equal("Dest", actor.TeleFogSource);
        Assert.Equal("Source", actor.TeleFogDest); Assert.False(player.HasTeleFogOverride);
        var saved = SimSavegame.Write(sim, true); actor.TeleFogSource = "Changed"; SimSavegame.Apply(sim, saved);
        Assert.Equal("Dest", actor.TeleFogSource);
    }

    [Fact]
    public void EmptySetterDoesNotCreateOverrideOnDefaultActor()
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.Equal(0, AcsActorTeleFog.Set(sim, 0, player, "", "")); Assert.False(player.HasTeleFogOverride);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
