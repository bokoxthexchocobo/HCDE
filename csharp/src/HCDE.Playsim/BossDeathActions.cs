using HCDE.Gamedata;

namespace HCDE.Playsim;

/// <summary>Native <c>p_enemy.cpp</c> <c>CheckBossDeath</c> / <c>A_BossDeath</c> exit subset.</summary>
public static class BossDeathActions
{
    public static bool CheckBossDeath(AuthoritySimulation sim, Actor dyingBoss)
    {
        if (!sim.Players.Any(player => !player.Destroyed && player.Health > 0)) return false;
        var bossClass = dyingBoss.ClassDoomEdNum;
        foreach (var other in sim.Actors)
        {
            if (ReferenceEquals(other, dyingBoss) || other.Destroyed) continue;
            if (other.Health > 0 || other.IceCorpse)
            {
                if (other.ClassDoomEdNum == bossClass
                    || ClassName(bossClass) is { } name && DoomActorCatalog.IsKindOfClassName(other.ClassDoomEdNum, name))
                    return false;
            }
        }
        return true;
    }

    public static void BossDeath(Actor actor)
    {
        if (actor.Destroyed) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Boss death requires a simulation.");
        if (!CheckBossDeath(sim, actor)) return;
        if (sim.Level.SpeckillMonstersOnBossDeath) sim.Massacre(baddies: true);
        if (sim.Level.ExitOnAllBossesDead) sim.MarkExited(sim.Level.ExitOnAllBossesDeadSecret);
    }

    private static string? ClassName(int editor) =>
        DoomActorCatalog.TrySpawnClassName(editor, out var name) ? name : null;
}
