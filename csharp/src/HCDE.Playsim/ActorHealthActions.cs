namespace HCDE.Playsim;

/// <summary>Native A_SetHealth/A_ResetHealth for managed actor state actions.</summary>
public static class ActorHealthActions
{
    public static void DamageSelf(Actor actor, int amount, string? damageType = null, int flags = 0,
        int sourceSelector = 0, int inflictorSelector = 0, string? classFilter = null, string? speciesFilter = null)
        => Damage(actor, actor, amount, damageType, flags, sourceSelector, inflictorSelector, classFilter, speciesFilter);

    public static void DamageTarget(Actor actor, int amount, string? damageType = null, int flags = 0,
        int sourceSelector = 0, int inflictorSelector = 0, string? classFilter = null, string? speciesFilter = null)
    {
        var target = Subject(actor, AcsActorPointer.Target);
        if (target is not null) Damage(actor, target, amount, damageType, flags, sourceSelector, inflictorSelector, classFilter, speciesFilter);
    }

    public static void KillTarget(Actor actor, string? damageType = null, int flags = 0,
        int sourceSelector = 0, int inflictorSelector = 0, string? classFilter = null, string? speciesFilter = null)
        => KillRelated(actor, AcsActorPointer.Target, false, false, damageType, flags, sourceSelector, inflictorSelector, classFilter, speciesFilter);

    public static void KillMaster(Actor actor, string? damageType = null, int flags = 0,
        int sourceSelector = 0, int inflictorSelector = 0, string? classFilter = null, string? speciesFilter = null)
        => KillRelated(actor, AcsActorPointer.Master, false, false, damageType, flags, sourceSelector, inflictorSelector, classFilter, speciesFilter);

    public static void KillTracer(Actor actor, string? damageType = null, int flags = 0,
        int sourceSelector = 0, int inflictorSelector = 0, string? classFilter = null, string? speciesFilter = null)
        => KillRelated(actor, AcsActorPointer.Tracer, false, false, damageType, flags, sourceSelector, inflictorSelector, classFilter, speciesFilter);

    public static void KillChildren(Actor actor, string? damageType = null, int flags = 0,
        int sourceSelector = 0, int inflictorSelector = 0, string? classFilter = null, string? speciesFilter = null)
        => KillRelated(actor, AcsActorPointer.Default, true, false, damageType, flags, sourceSelector, inflictorSelector, classFilter, speciesFilter);

    public static void KillSiblings(Actor actor, string? damageType = null, int flags = 0,
        int sourceSelector = 0, int inflictorSelector = 0, string? classFilter = null, string? speciesFilter = null)
        => KillRelated(actor, AcsActorPointer.Default, true, true, damageType, flags, sourceSelector, inflictorSelector, classFilter, speciesFilter);

    private static void KillRelated(Actor origin, int selector, bool family, bool siblings,
        string? damageType, int flags, int sourceSelector, int inflictorSelector,
        string? classFilter, string? speciesFilter)
    {
        if ((flags & ~127) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
        var selected = Subject(origin, siblings ? AcsActorPointer.Master : selector);
        if (selected is null) return;
        var source = Subject(origin, sourceSelector); var inflictor = Subject(origin, inflictorSelector);
        IEnumerable<Actor> victims = [selected];
        if (family)
        {
            var sim = origin.Simulation ?? throw new InvalidOperationException("Family kill requires a simulation.");
            victims = sim.Actors.Where(target => !target.Destroyed && target.MasterId == selected.Id
                && (!siblings || target != origin)).ToArray();
        }
        foreach (var victim in victims)
        {
            if (victim.Destroyed || !ActorActionFilters.Matches(victim, classFilter, speciesFilter,
                (flags & 16) != 0, (flags & 32) != 0, (flags & 64) != 0)) continue;
            if (victim is ProjectileActor missile && (flags & 2) != 0 && !victim.NoDamage
                && (!victim.Invulnerable || (flags & 1) != 0) && (!victim.Buddha || (flags & 8) != 0))
                missile.ExplodeForAction();
            if ((flags & 4) != 0) continue;
            var damageFlags = DamageFlags.BypassArmor | DamageFlags.NoFactor;
            if ((flags & 1) != 0) damageFlags |= DamageFlags.BypassInvulnerability;
            if ((flags & 8) != 0) damageFlags |= DamageFlags.FoilBuddha;
            ActorDamage.Apply(victim, victim.Health, source, damageFlags, damageType, inflictor);
        }
    }

    public static void DamageTracer(Actor actor, int amount, string? damageType = null, int flags = 0,
        int sourceSelector = 0, int inflictorSelector = 0, string? classFilter = null, string? speciesFilter = null)
    {
        var tracer = Subject(actor, AcsActorPointer.Tracer);
        if (tracer is not null) Damage(actor, tracer, amount, damageType, flags, sourceSelector, inflictorSelector, classFilter, speciesFilter);
    }

    public static void DamageMaster(Actor actor, int amount, string? damageType = null, int flags = 0,
        int sourceSelector = 0, int inflictorSelector = 0, string? classFilter = null, string? speciesFilter = null)
    {
        var master = Subject(actor, AcsActorPointer.Master);
        if (master is not null) Damage(actor, master, amount, damageType, flags, sourceSelector, inflictorSelector, classFilter, speciesFilter);
    }

    private static void Damage(Actor origin, Actor actor, int amount, string? damageType, int flags,
        int sourceSelector, int inflictorSelector, string? classFilter, string? speciesFilter)
        => DamageResolved(actor, Subject(origin, sourceSelector), Subject(origin, inflictorSelector), amount,
            damageType, flags, classFilter, speciesFilter);

    public static void DamageChildren(Actor actor, int amount, string? damageType = null, int flags = 0,
        int sourceSelector = 0, int inflictorSelector = 0, string? classFilter = null, string? speciesFilter = null)
        => DamageFamily(actor, amount, damageType, flags, sourceSelector, inflictorSelector, classFilter, speciesFilter, false);

    public static void DamageSiblings(Actor actor, int amount, string? damageType = null, int flags = 0,
        int sourceSelector = 0, int inflictorSelector = 0, string? classFilter = null, string? speciesFilter = null)
        => DamageFamily(actor, amount, damageType, flags, sourceSelector, inflictorSelector, classFilter, speciesFilter, true);

    private static void DamageFamily(Actor actor, int amount, string? damageType, int flags,
        int sourceSelector, int inflictorSelector, string? classFilter, string? speciesFilter, bool siblings)
    {
        var sim = actor.Simulation ?? throw new InvalidOperationException("Family damage requires a simulation.");
        var master = siblings ? Subject(actor, AcsActorPointer.Master) : actor;
        if (master is null) return;
        var source = Subject(actor, sourceSelector); var inflictor = Subject(actor, inflictorSelector);
        foreach (var target in sim.Actors.Where(target => !target.Destroyed && target.MasterId == master.Id
            && (!siblings || target != actor)).ToArray())
        {
            if (!target.Destroyed) DamageResolved(target, source, inflictor, amount, damageType, flags, classFilter, speciesFilter);
        }
    }

    private static void DamageResolved(Actor actor, Actor? source, Actor? inflictor, int amount,
        string? damageType, int flags, string? classFilter, string? speciesFilter)
    {
        if ((flags & ~1023) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
        if (!ActorActionFilters.Matches(actor, classFilter, speciesFilter, (flags & 64) != 0, (flags & 128) != 0, (flags & 256) != 0)) return;
        var damageFlags = DamageFlags.None;
        if ((flags & 1) != 0) damageFlags |= DamageFlags.BypassInvulnerability;
        if ((flags & 16) != 0) damageFlags |= DamageFlags.FoilBuddha;
        if ((flags & (4 | 8)) != 0) damageFlags |= DamageFlags.NoFactor;
        if ((flags & 2) == 0 || (flags & 4) != 0) damageFlags |= DamageFlags.BypassArmor;
        if ((flags & 32) != 0) damageFlags |= DamageFlags.NoProtect;
        if ((flags & 4) != 0) amount = unchecked(amount + actor.Health);
        if (amount > 0)
        {
            if ((flags & 512) != 0 && inflictor != null) damageType = inflictor.DamageType;
            ActorDamage.Apply(actor, amount, source, damageFlags, damageType, inflictor);
        }
        else if (amount < 0) actor.GiveBody(unchecked(-amount));
    }

    public static void Die(Actor actor, string? damageType = null)
        => ActorDamage.Apply(actor, actor.Health, flags: DamageFlags.Forced, damageType: damageType);

    public static void SetHealth(Actor actor, int health, int pointerSelector = 0)
    {
        var subject = Subject(actor, pointerSelector);
        if (subject is null || subject.Destroyed) return;
        subject.RestoreHealth(Math.Max(1, health));
    }

    public static void ResetHealth(Actor actor, int pointerSelector = 0)
    {
        var subject = Subject(actor, pointerSelector);
        if (subject is null || subject.Destroyed || subject.Health <= 0) return;
        subject.RestoreHealth(subject is PlayerPawn && subject.ResurrectionHealth <= 0 ? 100 : subject.SpawnHealth());
    }

    private static Actor? Subject(Actor actor, int pointerSelector)
    {
        if (pointerSelector == AcsActorPointer.Default) return actor;
        if (pointerSelector == AcsActorPointer.Null) return null;
        var sim = actor.Simulation
            ?? throw new InvalidOperationException("Actor health pointer selection requires a simulation.");
        return AcsActorPointer.Resolve(sim, actor, pointerSelector);
    }
}
