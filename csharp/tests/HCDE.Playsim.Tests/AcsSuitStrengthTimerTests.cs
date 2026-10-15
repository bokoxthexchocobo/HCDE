using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsSuitStrengthTimerTests
{
    [Theory]
    [InlineData("PowerIronFeet", 2100)]
    [InlineData("PowerMask", 2800)]
    [InlineData("PowerStrength", 1)]
    public void QueriesReportActivePowerTimersForModuleAndPooledNames(string name, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); AcsPlayerInventory.Give(player, name, 1);
        var pool = new AcsGlobalStrings();
        foreach (var pooled in new[] { false, true })
        {
            var id = pooled ? pool.Add(name.ToLowerInvariant()) : 0;
            Assert.Equal(expected, Query(sim, player, 0, id, [name], pool));
        }
    }

    [Fact]
    public void TimersFollowTheirNativeDirections()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerIronFeet(5); player.GivePowerMask(7); player.GivePowerStrength();
        sim.Tick(); Assert.Equal(4, AcsActorPowerups.RemainingTics(player, "PowerIronFeet"));
        Assert.Equal(6, AcsActorPowerups.RemainingTics(player, "PowerMask"));
        Assert.Equal(2, AcsActorPowerups.RemainingTics(player, "PowerStrength"));
    }

    [Theory]
    [InlineData("PowerIronFeet")]
    [InlineData("PowerMask")]
    [InlineData("PowerStrength")]
    public void TimerQueriesFollowRemovalAndSavedRestoration(string name)
    {
        var sim = Room(); var player = sim.Players.Single(); AcsPlayerInventory.Give(player, name, 1);
        var expected = AcsActorPowerups.RemainingTics(player, name); var saved = SimSavegame.Write(sim, true);
        AcsPlayerInventory.Take(player, name, 1); Assert.Equal(0, AcsActorPowerups.RemainingTics(player, name));
        SimSavegame.Apply(sim, saved); Assert.Equal(expected, AcsActorPowerups.RemainingTics(player, name));
    }

    [Fact]
    public void QueryUsesRequestedTidAndKeepsPowerClassesSeparate()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ThingId = 9; player.GivePowerMask(77);
        Assert.Equal(77, Query(sim, null, 9, 0, ["PowerMask"], new()));
        Assert.Equal(0, Query(sim, null, 9, 0, ["PowerIronFeet"], new()));
        Assert.Equal(0, Query(sim, player, 999, 0, ["PowerMask"], new()));
    }

    [Fact]
    public void ExpiredSuitAndGiverNameReturnZero()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerIronFeet(1); sim.Tick();
        Assert.Equal(0, AcsActorPowerups.RemainingTics(player, "PowerIronFeet"));
        Assert.Equal(0, AcsActorPowerups.RemainingTics(player, "RadSuit"));
    }

    private static int Query(AuthoritySimulation sim, Actor? actor, int tid, int id, string[] module, AcsGlobalStrings pool)
    {
        var stack = new List<int> { 77, tid, id };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, module, pool,
            AcsCallFunctions.GetActorPowerupTics, 2, out var result)); Assert.Equal(new[] { 77 }, stack); return result;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
