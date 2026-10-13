using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorClearTargetActionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ClearTargetHandlesGeneralAndProjectilePointersAndSavedClears(int kind)
    {
        var sim = Room(); var player = sim.Players.Single(); var target = sim.AddBot(100, 0);
        Actor actor = kind switch
        {
            0 => player,
            1 => sim.AddBot(0, 0),
            _ => sim.SpawnProjectile(player, ProjectileKind.RevenantTracer, target)
        };
        if (kind == 1) actor.Brain = null;
        actor.SetTargetPointer(target); actor.MasterId = target.Id; actor.TracerId = target.Id;
        actor.LastHeardTargetId = target.Id;
        var before = SimSavegame.Write(sim);
        ActorPropertyActions.ClearTarget(actor);
        Assert.Null(actor.TargetId); Assert.Null(actor.LastHeardTargetId);
        Assert.Equal(target.Id, actor.MasterId); Assert.Equal(target.Id, actor.TracerId);
        var cleared = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(cleared, out var state, out var error), error);
        actor.SetTargetPointer(target); actor.LastHeardTargetId = target.Id;
        sim.RestoreState(state);
        Assert.Null(actor.TargetId); Assert.Null(actor.LastHeardTargetId);
        Assert.Equal(cleared, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, before);
        Assert.Equal(target.Id, actor.TargetId); Assert.Equal(target.Id, actor.LastHeardTargetId);
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClearLastHeardPreservesGeneralAndProjectileTargets(bool projectile)
    {
        var sim = Room(); var player = sim.Players.Single(); var target = sim.AddBot(100, 0);
        Actor actor = projectile ? sim.SpawnProjectile(player, ProjectileKind.Plasma) : player;
        actor.SetTargetPointer(target); actor.LastHeardTargetId = target.Id;
        ActorPropertyActions.ClearLastHeard(actor);
        Assert.Equal(target.Id, actor.TargetId); Assert.Null(actor.LastHeardTargetId);
    }

    [Fact]
    public void ClearTargetClearsCurrentRememberedAndHeardActors()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 64); var enemy = sim.AddBot(20, 0, thingId: 7);
        caller.Brain!.SetTargetThingId(sim, 7);
        caller.Brain.WakeOnDamage(caller, sim.Players.Single(), 1, false);
        Assert.Equal(enemy.Id, caller.Brain.LastEnemyId);
        caller.LastHeardTargetId = enemy.Id;
        ActorPropertyActions.ClearTarget(caller);
        Assert.Null(caller.Brain.TargetId); Assert.Null(caller.Brain.LastEnemyId); Assert.Null(caller.LastHeardTargetId);
    }

    [Fact]
    public void ClearLastHeardPreservesBothCombatPointers()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 64); var enemy = sim.AddBot(20, 0, thingId: 7);
        caller.Brain!.SetTargetThingId(sim, 7);
        caller.Brain.WakeOnDamage(caller, sim.Players.Single(), 1, false); caller.LastHeardTargetId = enemy.Id;
        ActorPropertyActions.ClearLastHeard(caller);
        Assert.Equal(sim.Players.Single().Id, caller.Brain.TargetId);
        Assert.Equal(enemy.Id, caller.Brain.LastEnemyId); Assert.Null(caller.LastHeardTargetId);
    }

    [Fact]
    public void FrameClearPreventsSoundMemoryReacquisition()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var caller = sim.AddBot(0, 0); var enemy = sim.AddBot(100, 0);
        caller.LastHeardTargetId = enemy.Id; caller.Brain!.SetSpecialTarget(enemy);
        caller.ReactionTime = 0;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0, Action: ActorPropertyActions.ClearTarget)], 0);
        caller.States.Enter(caller, 1); sim.Tick();
        Assert.Null(caller.Brain.TargetId); Assert.Null(caller.LastHeardTargetId);
    }

    [Fact]
    public void ActorsWithoutBrainsCanClearSoundMemory()
    {
        var actor = new Actor { LastHeardTargetId = 42 };
        ActorPropertyActions.ClearLastHeard(actor); Assert.Null(actor.LastHeardTargetId);
        actor.LastHeardTargetId = 42; ActorPropertyActions.ClearTarget(actor); Assert.Null(actor.LastHeardTargetId);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }] });
}
