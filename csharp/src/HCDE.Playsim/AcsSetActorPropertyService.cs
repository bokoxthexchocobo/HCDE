namespace HCDE.Playsim;

/// <summary>Native <c>DLevelScript::DoSetActorProperty</c> subset for ACS <c>APROP_*</c> opcodes.</summary>
public static class AcsSetActorPropertyService
{
    public enum AcsActorProperty
    {
        Health = 0,
        Speed = 1,
        Damage = 2,
        Alpha = 3,
        RenderStyle = 4,
        Ambush = 5,
        Dropped = 6,
        Invulnerable = 7,
        Notarget = 8,
        Notrigger = 9,
        JumpZ = 10,
        ChaseGoal = 11,
        Frightened = 12,
        Friendly = 13,
        SpawnHealth = 14,
        Gravity = 15,
    }

    public static bool TryApply(Actor actor, AcsActorProperty property, int value, out string? rejectReason)
    {
        rejectReason = null;
        if (actor.Destroyed)
        {
            rejectReason = "Actor destroyed.";
            return false;
        }

        switch (property)
        {
            case AcsActorProperty.Health:
                if (actor.IsDead)
                    return true;
                actor.Health = value;
                return true;
            case AcsActorProperty.Speed:
                actor.MovementSpeed = Fixed.FromDouble(value / 65536.0);
                return true;
            case AcsActorProperty.Damage:
                actor.Damage = value;
                return true;
            case AcsActorProperty.Alpha:
                actor.Alpha = value / 65536.0;
                return true;
            case AcsActorProperty.Ambush:
                actor.Ambush = value != 0;
                return true;
            case AcsActorProperty.Dropped:
                actor.Dropped = value != 0;
                return true;
            case AcsActorProperty.Invulnerable:
                actor.Invulnerable = value != 0;
                return true;
            case AcsActorProperty.Notarget:
                actor.NoTarget = value != 0;
                return true;
            case AcsActorProperty.ChaseGoal:
                actor.ChaseGoal = value != 0;
                return true;
            case AcsActorProperty.Frightened:
                actor.Frightened = value != 0;
                return true;
            case AcsActorProperty.Friendly:
                actor.Friendly = value != 0;
                return true;
            case AcsActorProperty.Gravity:
                actor.Gravity = Fixed.FromDouble(value / 65536.0);
                return true;
            default:
                rejectReason = $"Unsupported actor property {property}.";
                return false;
        }
    }
}
