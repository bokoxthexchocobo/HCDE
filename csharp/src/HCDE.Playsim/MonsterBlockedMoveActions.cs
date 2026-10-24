using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>p_enemy.cpp</c> <c>P_Move</c> spechit door/push activation on blocked monster steps.</summary>
public static class MonsterBlockedMoveActions
{
    public static bool TryClearBlockedMove(AuthoritySimulation sim, Actor actor, LevelLine? blockingLine)
    {
        if (blockingLine is null || actor.NoTrigger || actor.Destroyed) return false;
        var good = 0;
        if (actor.CanUseWalls && TryUseWall(sim, actor, blockingLine)) good |= 1;
        if (actor.PushWall && TryPushWall(sim, actor, blockingLine)) good |= 2;
        if (good == 0) return false;
        actor.MoveDirection = 8;
        return (sim.NextMonsterOpenDoorByte() >= 203) ^ (good & 1) != 0;
    }

    private static bool TryUseWall(AuthoritySimulation sim, Actor actor, LevelLine line)
    {
        if (line.Special == 0) return false;
        return line.Special is LineSpecials.DoorRaise or LineSpecials.DoorOpen
            && LineSpecials.Execute(sim, actor, line.Special, line.Tag, line);
    }

    private static bool TryPushWall(AuthoritySimulation sim, Actor actor, LevelLine line)
        => line.Special != 0 && LineSpecials.Execute(sim, actor, line.Special, line.Tag, line);
}
