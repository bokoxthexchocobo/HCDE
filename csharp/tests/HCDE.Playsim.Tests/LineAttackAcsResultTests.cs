using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LineAttackAcsResultTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void ZeroDamageTypeUsesDefaultWhileNonzeroResolvesString(int typeId, bool typed)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var target = sim.AddBot(100, 0);
        target.SetTypedDeath("Fire", ActorStateMachine.Spawn);
        var stack = new List<int> { 0, 0, 0, target.Health, 0, typeId };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = sim.Players.Single() },
            ["Fire", "Fire"], new AcsGlobalStrings(), AcsCallFunctions.LineAttack, 6, out var result));
        Assert.Equal(0, result);
        Assert.True(target.IsDead);
        Assert.Equal(typed ? ActorStateMachine.Spawn : target.DeathState, target.States.Current);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NativeCallReturnsZeroForActorHitOrMiss(bool hit)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var target = sim.AddBot(100, hit ? 0 : 100); var health = target.Health;
        var stack = new List<int> { 987, 0, 0, 0, 5 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = sim.Players.Single() },
            [], new AcsGlobalStrings(), AcsCallFunctions.LineAttack, 4, out var result));
        Assert.Equal(0, result);
        Assert.Equal(hit ? health - 5 : health, target.Health);
        Assert.Equal(new[] { 987 }, stack);
    }
}
