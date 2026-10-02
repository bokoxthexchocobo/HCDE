using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LineAttackPuffInflictorTests
{
    [Fact]
    public void ShooterForcePainDoesNotBecomePuffForcePain()
    {
        var sim = Room(); var source = sim.Players.Single(); var target = sim.AddBot(100, 0);
        source.ForcePain = true; target.PainChance = 0; target.PainThreshold = 100;
        var state = target.States.Current;
        Attack(sim, source);
        Assert.Equal(state, target.States.Current);
        Assert.Equal(source.Id, target.LastDamageSourceId);
        Assert.Same(source, Assert.Single(sim.Actors.OfType<PuffActor>()).DamageSource);
    }

    [Fact]
    public void ShooterPainlessDoesNotSuppressOrdinaryPuffPain()
    {
        var sim = Room(); var source = sim.Players.Single(); var target = sim.AddBot(100, 0);
        source.Painless = true; target.PainChance = 256;
        Attack(sim, source);
        Assert.Equal(target.PainState, target.States.Current);
    }

    [Fact]
    public void ShooterFoilBuddhaDoesNotBypassProtectionWithOrdinaryPuff()
    {
        var sim = Room(); var source = sim.Players.Single(); var target = sim.AddBot(100, 0);
        source.FoilBuddha = true; target.Buddha = true;
        Attack(sim, source, target.Health + 7);
        Assert.Equal(1, target.Health); Assert.False(target.IsDead);
        Assert.Equal(source.Id, target.LastDamageSourceId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PuffRetainsDamageSourceForActorAndGeometryHits(bool geometry)
    {
        var sim = Room(geometry); var source = sim.Players.Single();
        if (!geometry) sim.AddBot(100, 0);
        Attack(sim, source);
        var puff = Assert.Single(sim.Actors.OfType<PuffActor>());
        Assert.Same(source, puff.DamageSource);
        Assert.False(puff.ForcePain); Assert.False(puff.Painless); Assert.False(puff.FoilBuddha);
    }

    private static void Attack(AuthoritySimulation sim, Actor source, int damage = 7)
    {
        var stack = new List<int> { 0, 0, 0, damage };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = source },
            [], new AcsGlobalStrings(), AcsCallFunctions.LineAttack, 4, out var result));
        Assert.Equal(0, result); Assert.Empty(stack);
    }

    private static AuthoritySimulation Room(bool wall = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 0 }],
        Lines = wall ? [new LevelLine { X1 = 64, X2 = 64, Y1 = 128, Y2 = -128,
            SideFront = 0, SideBack = -1 }] : [],
        Things = [new LevelThing { Type = 1 }],
    });
}
