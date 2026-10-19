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
        Score = 22,
        DamageFactor = 24,
        ScaleX = 29,
        ScaleY = 30,
        Mass = 32,
        Accuracy = 33,
        Stamina = 34,
        ReactionTime = 37,
        MeleeRange = 38,
        ViewHeight = 39,
        AttackZOffset = 40,
        StencilColor = 41,
        Friction = 42,
        DamageMultiplier = 43,
        MaxStepHeight = 44,
        MaxDropOffHeight = 45,
        FriendlySeeBlocks = 48,
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
            case AcsActorProperty.Score:
                actor.Score = value;
                return true;
            case AcsActorProperty.DamageFactor:
                actor.DamageFactor = new Fixed(value);
                return true;
            case AcsActorProperty.DamageMultiplier:
                actor.DamageMultiplier = new Fixed(value);
                return true;
            case AcsActorProperty.ScaleX:
                actor.ScaleX = (float)(value / 65536.0);
                return true;
            case AcsActorProperty.ScaleY:
                actor.ScaleY = (float)(value / 65536.0);
                return true;
            case AcsActorProperty.Mass:
                actor.Mass = value;
                return true;
            case AcsActorProperty.Accuracy:
                actor.Accuracy = value;
                return true;
            case AcsActorProperty.Stamina:
                actor.Stamina = value;
                return true;
            case AcsActorProperty.ReactionTime:
                actor.ReactionTime = value;
                return true;
            case AcsActorProperty.MeleeRange:
                actor.MeleeRange = new Fixed(value);
                return true;
            case AcsActorProperty.Friction:
                actor.Friction = new Fixed(value);
                return true;
            case AcsActorProperty.MaxStepHeight:
                actor.MaxStepHeight = Fixed.FromDouble(value / 65536.0);
                return true;
            case AcsActorProperty.MaxDropOffHeight:
                actor.MaxDropOffHeight = Fixed.FromDouble(value / 65536.0);
                return true;
            case AcsActorProperty.FriendlySeeBlocks:
                actor.FriendlySeeBlocks = value;
                return true;
            case AcsActorProperty.ViewHeight:
                if (actor is PlayerPawn viewPlayer)
                {
                    viewPlayer.DefaultViewHeight = new Fixed(value);
                    viewPlayer.ViewHeight = viewPlayer.DefaultViewHeight.ToDouble();
                }
                return true;
            case AcsActorProperty.AttackZOffset:
                if (actor is PlayerPawn attackPlayer)
                    attackPlayer.AttackZOffset = new Fixed(value);
                return true;
            case AcsActorProperty.StencilColor:
                actor.StencilFillColor = unchecked((uint)value);
                return true;
            default:
                rejectReason = $"Unsupported actor property {property}.";
                return false;
        }
    }
}
