namespace HCDE.Playsim;

/// <summary>Native <c>CheckActorFlag</c> / <c>ModActorFlag</c> subset for ACS CallFunc.</summary>
internal static class AcsActorFlags
{
    private enum Kind
    {
        Invulnerable,
        NoTarget,
        Ambush,
        Friendly,
        Solid,
        Shootable,
        Floating,
        NoGravity,
        NoPain,
        Pickup,
        Special,
    }

    public static bool TryGet(Actor actor, string? flagName, out bool value)
    {
        value = false;
        if (actor.Destroyed || string.IsNullOrEmpty(flagName) || !TryMap(flagName, out var kind))
            return false;
        value = Read(actor, kind);
        return true;
    }

    public static bool TrySet(Actor actor, string? flagName, bool set)
    {
        if (actor.Destroyed || string.IsNullOrEmpty(flagName) || !TryMap(flagName, out var kind))
            return false;
        Write(actor, kind, set);
        return true;
    }

    private static bool TryMap(string flagName, out Kind kind)
    {
        if (flagName.Equals("INVULNERABLE", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Invulnerable;
            return true;
        }
        if (flagName.Equals("NOTARGET", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.NoTarget;
            return true;
        }
        if (flagName.Equals("AMBUSH", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Ambush;
            return true;
        }
        if (flagName.Equals("FRIENDLY", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Friendly;
            return true;
        }
        if (flagName.Equals("SOLID", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Solid;
            return true;
        }
        if (flagName.Equals("SHOOTABLE", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Shootable;
            return true;
        }
        if (flagName.Equals("FLOAT", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Floating;
            return true;
        }
        if (flagName.Equals("NOGRAVITY", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.NoGravity;
            return true;
        }
        if (flagName.Equals("NOPAIN", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.NoPain;
            return true;
        }

        if (flagName.Equals("PICKUP", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Pickup;
            return true;
        }
        if (flagName.Equals("SPECIAL", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Special;
            return true;
        }
        kind = default;
        return false;
    }

    private static bool Read(Actor actor, Kind kind) => kind switch
    {
        Kind.Invulnerable => actor.Invulnerable,
        Kind.NoTarget => actor.NoTarget,
        Kind.Ambush => actor.Ambush,
        Kind.Friendly => actor.Friendly,
        Kind.Solid => actor.Solid,
        Kind.Shootable => actor.Shootable,
        Kind.Floating => actor.Floating,
        Kind.NoGravity => actor.NoGravity,
        Kind.NoPain => actor.NoPain,
        Kind.Pickup => actor.CanPickupItems,
        Kind.Special => actor.SpecialPickup,
        _ => false,
    };

    private static void Write(Actor actor, Kind kind, bool value)
    {
        switch (kind)
        {
            case Kind.Invulnerable: actor.Invulnerable = value; break;
            case Kind.NoTarget: actor.NoTarget = value; break;
            case Kind.Ambush: actor.Ambush = value; break;
            case Kind.Friendly: actor.Friendly = value; break;
            case Kind.Solid: actor.Solid = value; break;
            case Kind.Shootable: actor.Shootable = value; break;
            case Kind.Floating: actor.Floating = value; break;
            case Kind.NoGravity: actor.NoGravity = value; break;
            case Kind.NoPain: actor.NoPain = value; break;
            case Kind.Pickup: actor.CanPickupItems = value; break;
            case Kind.Special: actor.SpecialPickup = value; break;
        }
    }
}
