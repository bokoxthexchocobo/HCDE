using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPointerRearrangeTests
{
    [Fact]
    public void TargetAndMasterSwapUsesOriginalValuesAndRoundTrips()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(100, 0); var master = sim.AddBot(200, 0);
        actor.Brain!.SetSpecialTarget(target); actor.MasterId = master.Id; actor.LastHeardTargetId = target.Id;
        ActorPointerActions.RearrangePointers(actor, AcsActorPointer.Master, AcsActorPointer.Target);
        Assert.Equal(master.Id, actor.Brain.TargetId); Assert.Equal(target.Id, actor.MasterId);
        Assert.Equal(target.Id, actor.LastHeardTargetId);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Brain.SetSpecialTarget(null); actor.MasterId = null; sim.RestoreState(state);
        Assert.Equal(master.Id, actor.Brain.TargetId); Assert.Equal(target.Id, actor.MasterId);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(2, true)]
    public void MasterAssignmentAppliesCycleGuard(int flags, bool retained)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(100, 0);
        actor.Brain!.SetSpecialTarget(target); target.MasterId = actor.Id;
        ActorPointerActions.RearrangePointers(actor, 0, AcsActorPointer.Target, flags: flags);
        Assert.Equal(retained ? target.Id : (uint?)null, actor.MasterId);
    }

    [Fact]
    public void NullTargetDoesNotClearOtherTargetingMemory()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(100, 0);
        actor.Brain!.SetSpecialTarget(target); actor.LastHeardTargetId = target.Id; actor.MasterId = target.Id;
        ActorPointerActions.RearrangePointers(actor, AcsActorPointer.Null, AcsActorPointer.Null);
        Assert.Null(actor.Brain.TargetId); Assert.Null(actor.MasterId); Assert.Equal(target.Id, actor.LastHeardTargetId);
    }

    [Fact]
    public void UnsupportedTracerWriteFailsBeforeOtherWrites()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(100, 0); actor.Brain!.SetSpecialTarget(target);
        Assert.Throws<NotSupportedException>(() => ActorPointerActions.RearrangePointers(actor,
            AcsActorPointer.Null, AcsActorPointer.Target, AcsActorPointer.Target));
        Assert.Equal(target.Id, actor.Brain.TargetId); Assert.Null(actor.MasterId);
    }

    [Fact]
    public void ProjectileTracerAndMasterSwapUsesOriginalPointers()
    {
        var sim = Room(); var master = sim.AddBot(100, 0); var tracer = sim.AddBot(200, 0);
        var projectile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        projectile.MasterId = master.Id; projectile.RestoreTracerTarget(tracer.Id);
        ActorPointerActions.RearrangePointers(projectile, 0, AcsActorPointer.Tracer, AcsActorPointer.Master);
        Assert.Equal(tracer.Id, projectile.MasterId); Assert.Equal(master.Id, projectile.TracerTargetId);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        projectile.MasterId = null; projectile.RestoreTracerTarget(null); sim.RestoreState(state);
        Assert.Equal(tracer.Id, projectile.MasterId); Assert.Equal(master.Id, projectile.TracerTargetId);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void UnrecognizedAndUnchangedSelectorsDoNotWrite()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(100, 0);
        actor.Brain!.SetSpecialTarget(target); actor.MasterId = target.Id;
        var bytes = SimSavegame.Write(sim);
        ActorPointerActions.RearrangePointers(actor, AcsActorPointer.Target, AcsActorPointer.Master, AcsActorPointer.Tracer);
        ActorPointerActions.RearrangePointers(actor, 999, 999, 999);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
