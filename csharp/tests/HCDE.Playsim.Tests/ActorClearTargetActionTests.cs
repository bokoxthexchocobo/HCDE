using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorClearTargetActionTests
{
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
