using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseDirectionTurnFlagTests
{
    [Theory]
    [InlineData(64, true)]
    [InlineData(66, true)]
    [InlineData(80, true)]
    [InlineData(0, false)]
    public void NativeFlagPreservesFacingWithoutDisablingTargetSelection(int flags, bool preserve)
    {
        var (sim, actor) = Room(); var before = actor.Angle;
        actor.Brain!.Tick(sim, actor, (MonsterChaseFlags)flags, false, false);
        Assert.Equal(sim.Players.Single().Id, actor.Brain.TargetId);
        if (preserve) Assert.Equal(before, actor.Angle);
        else Assert.Equal(BamAngle.FromDegrees(0), actor.Angle);
        if ((flags & 16) == 0) Assert.True(actor.VelocityX.Raw > 0);
        else Assert.Equal(0, actor.VelocityX.Raw);
    }

    [Fact]
    public void DirectionSuppressionDoesNotPersistToNextChaseCall()
    {
        var (sim, actor) = Room();
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoDirectionTurn, false, false);
        Assert.Equal(BamAngle.FromDegrees(90), actor.Angle);
        actor.Brain.Tick(sim, actor, useMelee: false, useMissile: false);
        Assert.Equal(BamAngle.FromDegrees(0), actor.Angle);
    }

    private static (AuthoritySimulation, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 200 }] });
        var actor = new Actor { Simulation = sim, Brain = new MonsterBrain(MonsterAttack.Melee), ReactionTime = 0, Angle = BamAngle.FromDegrees(90) };
        actor.States.Configure(actor, [new(-1, 0)], 0);
        return (sim, actor);
    }
}
