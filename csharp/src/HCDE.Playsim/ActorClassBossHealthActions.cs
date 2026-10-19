namespace HCDE.Playsim;

/// <summary>Native <c>p_enemy.cpp</c> <c>A_ClassBossHealth</c> cooperative boss scaling.</summary>
public static class ActorClassBossHealthActions
{
    public static void ClassBossHealth(Actor actor)
    {
        if (actor.Destroyed) return;
        var sim = actor.Simulation;
        if (sim is null || sim.GameMode == SpawnGameMode.Deathmatch || sim.Players.Count() < 2) return;
        if (actor.BossCoopHealthScaled) return;
        actor.Health *= 5;
        actor.BossCoopHealthScaled = true;
    }
}
