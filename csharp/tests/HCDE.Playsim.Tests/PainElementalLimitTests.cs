using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class PainElementalLimitTests
{
    [Theory]
    [InlineData(20, false, false, true)]
    [InlineData(21, false, false, false)]
    [InlineData(21, true, false, false)]
    [InlineData(21, false, true, false)]
    [InlineData(22, false, false, false)]
    public void CompatibilityLimitCountsExistingClassThinkers(int count, bool dead, bool remap, bool allowed)
    {
        var (sim, parent) = Room(true, remap);
        for (var i = 0; i < count; i++)
        {
            var soul = sim.AddBot(1000 + i * 50, 500, remap ? 4000 : 3006);
            if (dead) soul.Health = 0;
        }
        var before = sim.Actors.Count;
        var spawned = sim.SpawnLostSoul(parent, null, 0);
        Assert.Equal(allowed, spawned != null);
        Assert.Equal(before + (allowed ? 1 : 0), sim.Actors.Count);
    }
    [Fact]
    public void CompatibilityLimitExcludesDestroyedActors()
    {
        var (sim, parent) = Room(true, false);
        for (var i = 0; i < 21; i++) sim.AddBot(1000 + i * 50, 500, 3006);
        sim.Actors.Last().Destroy();
        Assert.NotNull(sim.SpawnLostSoul(parent, null, 0));
    }
    [Fact]
    public void DefaultAllowsSpawningPastCompatibilityThreshold()
    {
        var (sim, parent) = Room(false, false);
        for (var i = 0; i < 22; i++) sim.AddBot(1000 + i * 50, 500, 3006);
        Assert.NotNull(sim.SpawnLostSoul(parent, null, 0));
    }
    private static (AuthoritySimulation, Actor) Room(bool limited, bool remap)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 71 }] },
            compat: limited ? CompatSurface.LimitPain : CompatSurface.None,
            dehacked: remap ? DehackedPatch.Apply("Thing 19\nID # = 4000\n") : null);
        return (sim, Assert.Single(sim.Actors));
    }
}