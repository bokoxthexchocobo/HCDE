using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPointerLifetimeTests
{
    [Theory]
    [InlineData(2, false)]
    [InlineData(2, true)]
    [InlineData(4, false)]
    [InlineData(4, true)]
    [InlineData(8, false)]
    [InlineData(8, true)]
    public void DestroyedPointerIsClearedBeforeNotificationAndAfterRemoval(int selector, bool remove)
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 3001); caller.Brain = null;
        var referenced = sim.AddBot(200, 0, 3001); referenced.Brain = null;
        if (selector == AcsActorPointer.Target) caller.SetTargetPointer(referenced);
        else if (selector == AcsActorPointer.Master) caller.MasterId = referenced.Id;
        else caller.TracerId = referenced.Id;
        uint? Read() => selector == AcsActorPointer.Target ? caller.TargetId
            : selector == AcsActorPointer.Master ? caller.MasterId : caller.TracerId;
        Assert.Equal(referenced.Id, Read());
        var notifications = 0;
        sim.ActorDestroyed += actor =>
        {
            if (!ReferenceEquals(actor, referenced)) return;
            Assert.Null(Read()); notifications++;
        };
        referenced.Destroy(); if (remove) sim.Tick();
        Assert.Equal(1, notifications); Assert.Null(Read());
        Assert.Null(AcsActorPointer.Resolve(sim, caller, selector));
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        Assert.Null(Read()); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void ClearingOneDestroyedActorPreservesOtherReferences()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 3001); caller.Brain = null;
        var destroyed = sim.AddBot(200, 0, 3001); destroyed.Brain = null;
        var retained = sim.AddBot(400, 0, 3001); retained.Brain = null;
        caller.SetTargetPointer(retained); caller.MasterId = destroyed.Id; caller.TracerId = retained.Id;
        destroyed.Destroy();
        Assert.Null(caller.MasterId); Assert.Equal(retained.Id, caller.TargetId); Assert.Equal(retained.Id, caller.TracerId);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void LateAssignmentsToDestroyedIdsClearOnRead(bool tracer, bool remove)
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 3001); caller.Brain = null;
        var target = sim.AddBot(200, 0, 3001); target.Brain = null;
        var id = target.Id; target.Destroy(); if (remove) sim.Tick();
        if (tracer) caller.TracerId = id; else caller.MasterId = id;
        Assert.Null(tracer ? caller.TracerId : caller.MasterId);
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        Assert.Null(tracer ? caller.TracerId : caller.MasterId);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ValidPointersSurviveReconstructionOfLaterActor(bool tracer)
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 3001); caller.Brain = null;
        var target = sim.SpawnProjectile(caller, ProjectileKind.Plasma);
        if (tracer) caller.TracerId = target.Id; else caller.MasterId = target.Id;
        var bytes = SimSavegame.Write(sim); var targetId = target.Id;
        target.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(targetId, tracer ? caller.TracerId : caller.MasterId);
        Assert.Same(sim.Actors.Single(actor => actor.Id == targetId), AcsActorPointer.Resolve(sim, caller,
            tracer ? AcsActorPointer.Tracer : AcsActorPointer.Master));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void LateTargetAssignmentsClearForStoredAndBrainTargets(bool brain, bool remove)
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 3001);
        if (brain) caller.Brain!.Enabled = false; else caller.Brain = null;
        var target = sim.AddBot(200, 0, 3001); target.Brain = null;
        target.Destroy(); if (remove) sim.Tick();
        caller.SetTargetPointer(target);
        Assert.Null(caller.TargetId); Assert.Null(caller.Brain?.TargetId);
        Assert.Null(AcsActorPointer.Resolve(sim, caller, AcsActorPointer.Target));
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        Assert.Null(caller.TargetId); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TargetPointersSurviveSupportedProjectileReconstruction(bool brain)
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 3001);
        if (brain) caller.Brain!.Enabled = false; else caller.Brain = null;
        var target = sim.SpawnProjectile(caller, ProjectileKind.Plasma);
        caller.SetTargetPointer(target); var id = target.Id;
        var bytes = SimSavegame.Write(sim);
        target.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(id, caller.TargetId);
        if (brain) Assert.Equal(id, caller.Brain!.TargetId);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoredStaleLastEnemyClearsOnRead(bool remove)
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 3001); caller.Brain!.Enabled = false;
        var target = sim.AddBot(200, 0, 3001); target.Brain = null; var id = target.Id;
        target.Destroy(); if (remove) sim.Tick();
        caller.Brain.RestoreTargetMemory(new(null, id, null));
        Assert.Null(caller.Brain.LastEnemyId);
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        Assert.Null(caller.Brain.LastEnemyId); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
