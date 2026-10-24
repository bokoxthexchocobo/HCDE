namespace HCDE.Playsim;

/// <summary>Native <c>P_PoisonMobj</c> and <c>AActor::Tick</c> received-poison cadence (<c>p_interaction.cpp</c>, <c>p_mobj.cpp</c>).</summary>
internal static class ActorPoisonReceived
{
    /// <summary>Native <c>P_PoisonMobj</c> without net mirror short-circuit.</summary>
    public static void Apply(Actor target, Actor inflictor, Actor? source, int damage, int duration, int period, string? damageType)
    {
        if (!inflictor.PoisonAlways)
        {
            if (target.Invulnerable)
            {
                if (target is PlayerPawn)
                    return;
                if (!inflictor.FoilInvul)
                    return;
            }
        }

        target.Poisoner = source;
        target.PoisonDamageTypeReceived = damageType;
        target.PoisonPeriodReceived = period;
        if (inflictor.AdditivePoisonDamage)
            target.PoisonDamageReceived += damage;
        else
            target.PoisonDamageReceived = damage;
        if (inflictor.AdditivePoisonDuration)
            target.PoisonDurationReceived += duration;
        else
            target.PoisonDurationReceived = duration;
    }

    public static void Tick(AuthoritySimulation simulation, Actor actor)
    {
        if (actor.Destroyed || actor.IsDead || actor.PoisonDurationReceived <= 0)
            return;
        var period = actor.PoisonPeriodReceived > 0 ? actor.PoisonPeriodReceived : GameTicClock.TicRate;
        if (simulation.Thinkers.Clock.Tic % period != 0)
            return;
        var damageType = actor.PoisonDamageTypeReceived;
        if (string.IsNullOrEmpty(damageType) || string.Equals(damageType, "None", StringComparison.OrdinalIgnoreCase))
            damageType = "Poison";
        ActorDamage.Apply(actor, actor.PoisonDamageReceived, actor.Poisoner, damageType: damageType);
        if (actor.Destroyed)
            return;
        actor.PoisonDurationReceived--;
        if (actor.PoisonDurationReceived == 0)
            actor.PoisonDamageReceived = 0;
    }
}
