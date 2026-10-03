namespace HCDE.Playsim;

/// <summary>Native <c>APROP_*</c> subset for ACS get/set actor property.</summary>
internal static class AcsActorProperties
{
    public const int Health = 0;
    public const int Ambush = 10;
    public const int Invulnerable = 11;
    public const int JumpZ = 12;
    public const int Gravity = 15;
    public const int Friendly = 16;
    public const int SpawnHealth = 17;
    public const int NoTarget = 19;
    public const int DamageFactor = 24;
    public const int TargetTid = 26;
    public const int Mass = 32;
    public const int Height = 35;
    public const int Radius = 36;
    public const int ViewHeight = 39;
    public const int AttackZOffset = 40;
    public const int DamageMultiplier = 43;
    public const int MaxStepHeight = 44;
    public const int MaxDropOffHeight = 45;

    public static void Set(AuthoritySimulation sim, Actor? activator, int tid, int property, int value)
    {
        if (tid == 0)
        {
            ApplySet(activator, property, value);
            return;
        }

        foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
            ApplySet(actor, property, value);
    }

    public static int Get(AuthoritySimulation sim, Actor? activator, int tid, int property)
    {
        var actor = tid == 0 ? activator : AcsActorTid.SingleFromTid(sim, tid);
        return actor is null || actor.Destroyed ? 0 : Read(actor, property);
    }

    /// <summary>Native <c>CheckActorProperty</c> for integer and boolean <c>APROP_*</c> we support.</summary>
    public static bool Check(AuthoritySimulation sim, Actor? activator, int tid, int property, int value)
    {
        var actor = tid == 0 ? activator : AcsActorTid.SingleFromTid(sim, tid);
        if (actor is null || actor.Destroyed)
            return false;
        // Native CheckActorProperty rejects unknown properties, even when Get returns zero.
        if (property is not (Health or Ambush or Invulnerable or JumpZ or Gravity or Friendly
            or SpawnHealth or NoTarget or TargetTid or Mass or Height or Radius or ViewHeight or AttackZOffset
            or MaxStepHeight or MaxDropOffHeight or DamageFactor or DamageMultiplier))
            return false;
        var actual = Read(actor, property);
        return IsBoolean(property) ? actual == (value != 0 ? 1 : 0) : actual == value;
    }

    private static bool IsBoolean(int property) => property is Ambush or Invulnerable or Friendly or NoTarget;

    private static void ApplySet(Actor? actor, int property, int value)
    {
        if (actor is null || actor.Destroyed)
            return;

        switch (property)
        {
            case Health:
                if (actor.Health <= 0)
                    return;
                actor.Health = value;
                break;
            case Ambush:
                actor.Ambush = value != 0;
                break;
            case Invulnerable:
                actor.Invulnerable = value != 0;
                break;
            case JumpZ:
                if (actor is PlayerPawn jumpPlayer) jumpPlayer.JumpZ = new Fixed(value);
                break;
            case Friendly:
                actor.Friendly = value != 0;
                break;
            case Gravity:
                actor.Gravity = new Fixed(value);
                break;
            case DamageFactor:
                actor.DamageFactor = new Fixed(value);
                break;
            case DamageMultiplier:
                actor.DamageMultiplier = new Fixed(value);
                break;
            case ViewHeight:
                if (actor is PlayerPawn viewPlayer)
                {
                    viewPlayer.DefaultViewHeight = new Fixed(value);
                    viewPlayer.ViewHeight = viewPlayer.DefaultViewHeight.ToDouble();
                }
                break;
            case NoTarget:
                actor.NoTarget = value != 0;
                break;
            case AttackZOffset:
                if (actor is PlayerPawn attackPlayer) attackPlayer.AttackZOffset = new Fixed(value);
                break;
            case SpawnHealth:
                actor.ResurrectionHealth = value;
                break;
            case Mass:
                actor.Mass = value;
                break;
            case MaxStepHeight:
                actor.MaxStepHeight = Fixed.FromDouble(value / 65536.0);
                break;
            case MaxDropOffHeight:
                actor.MaxDropOffHeight = Fixed.FromDouble(value / 65536.0);
                break;
            case TargetTid:
                if (actor.Brain is MonsterBrain brain && actor.Simulation is { } sim)
                    brain.SetTargetThingId(sim, value);
                break;
        }
    }

    private static int Read(Actor actor, int property) => property switch
    {
        Health => actor.Health,
        Ambush => actor.Ambush ? 1 : 0,
        Invulnerable => actor.Invulnerable ? 1 : 0,
        JumpZ => actor is PlayerPawn jumpPlayer ? jumpPlayer.JumpZ.Raw : 0,
        Gravity => actor.Gravity.Raw,
        DamageFactor => actor.DamageFactor.Raw,
        DamageMultiplier => actor.DamageMultiplier.Raw,
        Friendly => actor.Friendly ? 1 : 0,
        NoTarget => actor.NoTarget ? 1 : 0,
        SpawnHealth => actor.ResurrectionHealth,
        Mass => actor.Mass,
        Height => actor.Height.Raw,
        Radius => actor.Radius.Raw,
        ViewHeight => actor is PlayerPawn viewPlayer ? viewPlayer.DefaultViewHeight.Raw : 0,
        AttackZOffset => actor is PlayerPawn attackPlayer ? attackPlayer.AttackZOffset.Raw : 0,
        MaxStepHeight => actor.MaxStepHeight.Raw,
        MaxDropOffHeight => actor.MaxDropOffHeight.Raw,
        TargetTid => TargetThingId(actor),
        _ => 0,
    };

    private static int TargetThingId(Actor actor)
    {
        if (actor.Brain is not MonsterBrain brain || brain.TargetId is not { } targetId)
            return 0;
        var target = actor.Simulation?.Actors.FirstOrDefault(candidate => candidate.Id == targetId && !candidate.Destroyed);
        return target?.ThingId ?? 0;
    }
}
