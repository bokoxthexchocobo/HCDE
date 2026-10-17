using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileClassIdentityTests
{
    [Fact]
    public void CyberRocketDoesNotMatchItsRocketParentInExactFilters()
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.CyberRocket);
        ActorRemovalActions.Remove(missile, flags: 8, classFilter: "Rocket"); Assert.False(missile.Destroyed);
        ActorRemovalActions.Remove(missile, flags: 8, classFilter: "CyberRocket"); Assert.True(missile.Destroyed);
    }

    [Theory]
    [InlineData(ProjectileKind.Rocket, "Rocket")]
    [InlineData(ProjectileKind.CyberRocket, "CyberRocket")]
    [InlineData(ProjectileKind.Plasma, "PlasmaBall")]
    [InlineData(ProjectileKind.Bfg, "BFGBall")]
    [InlineData(ProjectileKind.ImpBall, "DoomImpBall")]
    [InlineData(ProjectileKind.BaronBall, "BaronBall")]
    [InlineData(ProjectileKind.CacodemonBall, "CacodemonBall")]
    [InlineData(ProjectileKind.ArachnotronPlasma, "ArachnotronPlasma")]
    [InlineData(ProjectileKind.MancubusBall, "FatShot")]
    [InlineData(ProjectileKind.RevenantTracer, "RevenantTracer")]
    public void NativeIdentityDrivesAcsQueriesAndExactRemovalAcrossFreshSave(ProjectileKind kind, string name)
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.Players.Single(), kind);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            var actor = world.Actors.Single(a => a.Id == missile.Id);
            var globals = new AcsGlobalStrings(); var stack = new List<int> { 0 };
            Assert.True(AcsCallFunctions.TryInvoke(world, stack, new() { Value = actor }, [], globals,
                AcsCallFunctions.GetActorClass, 1, out var result));
            Assert.Equal(name, globals.Get(result)); Assert.Empty(stack);
            stack.AddRange([0, 0]);
            Assert.True(AcsCallFunctions.TryInvoke(world, stack, new() { Value = actor }, [name.ToLowerInvariant()], globals,
                AcsCallFunctions.CheckActorClass, 2, out result));
            Assert.Equal(1, result); Assert.Empty(stack);
            ActorRemovalActions.Remove(actor, flags: 8, classFilter: "Actor"); Assert.False(actor.Destroyed);
            ActorRemovalActions.Remove(actor, flags: 8 | 16, classFilter: name); Assert.False(actor.Destroyed);
            ActorRemovalActions.Remove(actor, flags: 8, classFilter: name); Assert.True(actor.Destroyed);
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
    });
}
