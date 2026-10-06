using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPointerTransferTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(2, true)]
    public void MasterCycleGuardAndUnsafeFlag(int flags, bool retained)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var source = sim.AddBot(100, 0);
        actor.Brain!.SetSpecialTarget(source); source.MasterId = actor.Id;
        ActorPointerActions.TransferPointer(actor, sourceField: AcsActorPointer.Target, recipientField: AcsActorPointer.Master, flags: flags);
        Assert.Equal(retained ? source.Id : (uint?)null, actor.MasterId);
    }

    [Fact]
    public void MasterGuardRejectsExistingDownstreamCycle()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var first = sim.AddBot(100, 0); var second = sim.AddBot(200, 0);
        first.MasterId = second.Id; second.MasterId = first.Id; actor.Brain!.SetSpecialTarget(first);
        ActorPointerActions.TransferPointer(actor, sourceField: AcsActorPointer.Target, recipientField: AcsActorPointer.Master);
        Assert.Null(actor.MasterId); Assert.Equal(second.Id, first.MasterId);
    }

    [Fact]
    public void DefaultRecipientFieldCopiesTargetWithoutClearingOtherMemory()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var source = sim.AddBot(100, 0); var target = sim.AddBot(200, 0);
        actor.MasterId = source.Id; source.Brain!.SetSpecialTarget(target); actor.LastHeardTargetId = source.Id;
        actor.Health = 7;
        ActorPointerActions.TransferPointer(actor, sourceSelector: AcsActorPointer.Master);
        Assert.Equal(target.Id, actor.Brain!.TargetId); Assert.Equal(source.Id, actor.LastHeardTargetId); Assert.Equal(7, actor.Health);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Brain.SetSpecialTarget(null); sim.RestoreState(state);
        Assert.Equal(target.Id, actor.Brain.TargetId); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void SelfReferenceClearsEvenWithUnsafeFlag()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var source = sim.AddBot(100, 0);
        actor.MasterId = source.Id; source.MasterId = actor.Id;
        ActorPointerActions.TransferPointer(actor, sourceSelector: AcsActorPointer.Master, sourceField: AcsActorPointer.Master, flags: 2);
        Assert.Null(actor.MasterId);
    }

    [Fact]
    public void MissingSourceClearsFieldAndMissingRecipientDoesNothing()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var master = sim.AddBot(100, 0); actor.MasterId = master.Id;
        ActorPointerActions.TransferPointer(actor, sourceSelector: AcsActorPointer.Null, sourceField: AcsActorPointer.Master);
        Assert.Null(actor.MasterId);
        ActorPointerActions.TransferPointer(actor, recipientSelector: AcsActorPointer.Null);
        Assert.False(actor.Destroyed);
    }

    [Fact]
    public void ProjectileTracerAssignmentAndUnsupportedOwnerAssignment()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var target = sim.AddBot(200, 0);
        var projectile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        caller.MasterId = projectile.Id; caller.Brain!.SetSpecialTarget(target);
        ActorPointerActions.TransferPointer(caller, recipientSelector: AcsActorPointer.Master, recipientField: AcsActorPointer.Tracer);
        Assert.Equal(target.Id, projectile.TracerTargetId);
        Assert.Throws<NotSupportedException>(() => ActorPointerActions.TransferPointer(caller, recipientSelector: AcsActorPointer.Master));
        Assert.Equal(target.Id, projectile.TracerTargetId);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
