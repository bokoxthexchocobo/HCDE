using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VileGhostCompatibilityTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void CompatibilityRetainsCorpseRadiusAndQuadruplesHeight(int height)
    {
        var sim = Room(CompatSurface.VileGhosts); var corpse = Prepare(sim, height);
        Assert.True(ArchvileActions.TryRaise(sim, new Actor { X = Fixed.FromInt(-5) }, sim.Players.Single()));
        Assert.Equal(Fixed.FromInt(height * 4), corpse.Height);
        Assert.Equal(default(Fixed), corpse.Radius);
        Assert.False(corpse.IsDead);
    }

    [Fact]
    public void CompatibilityDoesNotChangeThingRaiseDimensions()
    {
        var sim = Room(CompatSurface.VileGhosts); var corpse = Prepare(sim, 0);
        Assert.True(ThingRaise.Execute(sim, 17, corpse, 0, 2));
        Assert.Equal(corpse.ResurrectionHeight, corpse.Height);
        Assert.Equal(corpse.ResurrectionRadius, corpse.Radius);
    }

    private static Actor Prepare(AuthoritySimulation sim, int height)
    {
        var corpse = sim.Actors[1]; corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Height = Fixed.FromInt(height); corpse.Radius = default;
        return corpse;
    }

    private static AuthoritySimulation Room(CompatSurface compat) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 10 }],
    }, compat: compat);
}
