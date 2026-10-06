using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorRelatedDistanceJumpTests
{
    [Theory]
    [InlineData(false, 100, false)]
    [InlineData(false, 101, true)]
    [InlineData(true, 100, false)]
    [InlineData(true, 101, true)]
    public void RelatedPointerUsesStrictHorizontalBoundary(bool tracer, double distance, bool expected)
    {
        var sim = Room(); var subject = sim.AddBot(100, 0, 3004);
        Actor actor = tracer ? sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket) : sim.AddBot(0, 0, 3004);
        actor.X = actor.Y = actor.Z = default;
        if (actor is ProjectileActor missile) missile.RestoreTracerTarget(subject.Id);
        else actor.MasterId = subject.Id;
        var result = tracer ? ActorJumpActions.JumpIfTracerCloser(actor, distance, 2)
            : ActorJumpActions.JumpIfMasterCloser(actor, distance, 2);
        Assert.Equal(expected ? 2 : (int?)null, result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void VerticalBodyGapAndNoZControlReturnedState(bool noZ)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004); var master = sim.AddBot(10, 0, 3004);
        actor.MasterId = master.Id; master.Z = Fixed.FromInt(200);
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.JumpIfMasterCloser(self, 100, 2, noZ)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(noZ ? 2 : 1, actor.States.Current);
    }

    [Fact]
    public void MissingAndDestroyedPointersDoNotJump()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var master = sim.AddBot(10, 0);
        Assert.Null(ActorJumpActions.JumpIfMasterCloser(actor, 100, 2));
        actor.MasterId = master.Id; master.Destroy();
        Assert.Null(ActorJumpActions.JumpIfMasterCloser(actor, 100, 2));
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        Assert.Null(ActorJumpActions.JumpIfTracerCloser(missile, 100, 2));
        missile.RestoreTracerTarget(master.Id);
        Assert.Null(ActorJumpActions.JumpIfTracerCloser(missile, 100, 2));
    }

    [Fact]
    public void RestoredMasterRelationshipStillSelectsDistanceSubject()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var master = sim.AddBot(10, 0); actor.MasterId = master.Id;
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.MasterId = null; sim.RestoreState(state);
        Assert.Equal(2, ActorJumpActions.JumpIfMasterCloser(actor, 100, 2));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 256 }] });
}
