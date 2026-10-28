namespace HCDE.Playsim;

/// <summary>Native <c>A_MonsterRefire</c> state jump helper.</summary>
public static class ActorRefireActions
{
    /// <returns>Next state index, or null to continue the current chain.</returns>
    public static int? MonsterRefire(Actor actor, int probability, int failState)
    {
        var sim = actor.Simulation ?? throw new InvalidOperationException("Monster refire requires a simulation.");
        ActorOrientationActions.FaceTarget(actor);
        if (RollContinue(sim, probability)) return null;
        var target = AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Target);
        if (target is null || target.Destroyed || target.Health <= 0
            || MonsterBrain.HitFriend(sim, actor, target)
            || !CombatTrace.CheckSight(sim, actor, target,
                ActorSightFlags.SeePastBlockEverything | ActorSightFlags.SeePastShootableLines))
            return failState;
        return null;
    }

    private static bool RollContinue(AuthoritySimulation sim, int probability) =>
        probability >= 256 || sim.NextMonsterRefireByte() < probability;
}
