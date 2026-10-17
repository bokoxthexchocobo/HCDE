using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedFloatBobDefaultsTests
{
    [Theory]
    [InlineData("FLOATBOB", true)]
    [InlineData("FLOATBOB+PASSMOBJ", true)]
    [InlineData("PASSMOBJ", false)]
    public void MapAndDynamicActorsApplySecondaryBobDefault(string bits, bool enabled)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n");
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }],
        }, dehacked: patch);
        var map = sim.Actors.Single(); var dynamic = sim.AddBot(300, 0);
        foreach (var actor in new[] { map, dynamic })
        {
            Assert.Equal(enabled, actor.FloatBob);
            actor.FloatBobPhase = 16;
            Assert.Equal(enabled ? 8 : 0, actor.GetBobOffset(), 8);
        }
        var saved = SimSavegame.Write(sim, true);
        map.FloatBob = dynamic.FloatBob = !enabled;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(enabled, map.FloatBob); Assert.Equal(enabled, dynamic.FloatBob);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData("FLOATBOB", true)]
    [InlineData("FLOATBOB\nBits = PASSMOBJ", false)]
    public void ProjectileAppliesInheritedOrReplacedBobDefault(string bits, bool enabled)
    {
        var baseline = DehackedPatch.Apply($"Thing 35\nBits = {bits}\n");
        var patch = DehackedPatch.Apply("Thing 35\nSpeed = 12\n", baseline);
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] }, dehacked: patch);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        Assert.Equal(enabled, missile.FloatBob);
        var saved = SimSavegame.Write(sim, true); missile.FloatBob = !enabled;
        SimSavegame.Apply(sim, saved); Assert.Equal(enabled, missile.FloatBob);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void PrimaryOnlyReplacementPreservesSecondaryBobFlag()
    {
        var baseline = DehackedPatch.Apply("Thing 2\nBits = FLOATBOB\n");
        var patch = DehackedPatch.Apply("Thing 2\nBits = SHOOTABLE\n", baseline);
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: patch);
        Assert.True(sim.AddBot(0, 0).FloatBob);
    }
}
