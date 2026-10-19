namespace HCDE.Playsim;

public static class ActorChaseActions
{
    /// <summary>Native <c>A_Chase</c> default (melee and missile).</summary>
    public static void Chase(Actor actor) => ExtChase(actor, useMelee: true, useMissile: true);

    public static void ExtChase(Actor actor, bool useMelee, bool useMissile, bool playActive = true,
        bool nightmareFast = false)
    {
        if (actor.Destroyed || actor.Brain is null) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Extended chase requires an attached simulation.");
        actor.Brain.Tick(sim, actor, playActive ? MonsterChaseFlags.None : MonsterChaseFlags.NoPlayActive,
            useMelee, useMissile, nightmareFast);
    }
}
