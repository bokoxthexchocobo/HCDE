namespace HCDE.Playsim;

public static class ActorChaseActions
{
    /// <summary>Native <c>A_Chase</c> default (melee and missile).</summary>
    public static void Chase(Actor actor) => ExtChase(actor, useMelee: true, useMissile: true);

    public static void ExtChase(Actor actor, bool useMelee, bool useMissile, bool playActive = true,
        bool nightmareFast = false, MonsterChaseFlags flags = MonsterChaseFlags.None)
    {
        if (actor.Destroyed || actor.Brain is null) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Extended chase requires an attached simulation.");
        if (!playActive) flags |= MonsterChaseFlags.NoPlayActive;
        if (nightmareFast) flags |= MonsterChaseFlags.NightmareFast;
        actor.Brain.Tick(sim, actor, flags, useMelee, useMissile, nightmareFast, sim.NativeMonsterMovement);
    }

    /// <summary>Native <c>A_ChaseNative</c> with <c>CHF_*</c> flag bits.</summary>
    public static void ChaseNative(Actor actor, int meleeLabel, int missileLabel, int flags)
        => ExtChase(actor, useMelee: true, useMissile: true, playActive: (flags & 2) == 0,
            nightmareFast: (flags & 4) != 0, ConvertFlags(flags));

    private static MonsterChaseFlags ConvertFlags(int flags) => (MonsterChaseFlags)(flags & 0x3ff);

    public static void DoChaseNative(Actor actor, int flags) => ChaseNative(actor, 0, 0, flags);
}
