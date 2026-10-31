namespace HCDE.Playsim;

/// <summary>
/// <c>PlayBounceSound</c> and <c>FloorBounceMissile</c> through bounce-state selection from
/// <c>p_mobj.cpp</c> (**2101–2250**). Chunk **1111**.
/// </summary>
public static class VmPMobjFloorBounce1111Slice
{
    public const int BounceNoWallSound = 1;
    public const int BounceQuiet = 2;
    public const int BounceUseSeeSound = 4;
    public const int BounceExplodeOnWater = 8;
    public const int BounceCanBounceWater = 16;
    public const int BounceCeilings = 32;
    public const int BounceFloors = 64;
    public const int BounceNotOnSky = 128;
    public const int BounceHereticType = 256;
    public const int BounceMbf = 512;
    public const int BounceKeepAngle = 1024;
    public const int BounceModifyPitch = 2048;
    public const int BounceUseBounceState = 4096;
    public const int MfMissile = 0x100;
    public const int MfInBounce = 0x200;

    public enum BounceSoundChoice
    {
        None,
        See,
        Floor,
        Wall
    }

    public static BounceSoundChoice ResolveBounceSound(bool onFloor, int bounceFlags, bool wallSoundValid, ref double volume)
    {
        if (!onFloor && (bounceFlags & BounceNoWallSound) != 0)
            return BounceSoundChoice.None;

        if ((bounceFlags & BounceQuiet) != 0)
            return BounceSoundChoice.None;

        volume = Math.Clamp(volume, 0.0, 1.0);
        if ((bounceFlags & BounceUseSeeSound) != 0)
            return BounceSoundChoice.See;
        if (onFloor || !wallSoundValid)
            return BounceSoundChoice.Floor;
        return BounceSoundChoice.Wall;
    }

    /// <summary>Native special-bounce switch is inverted: case 1 continues, case 0 destroys.</summary>
    public static bool? SpecialBounceMissileOutcome(int specialResult) =>
        specialResult switch
        {
            1 => false,
            0 => true,
            _ => null
        };

    public sealed class FloorBounceContext
    {
        public int Flags { get; set; }
        public int BounceFlags { get; init; }
        public bool IsMissile { get; init; }
        public int? SpecialBounceResult { get; init; }
        public bool PlaneIsDamageable { get; init; }
        public int BounceCount { get; set; }
        public bool InLiquid { get; init; }
        public double NormalZ { get; init; }
        public bool Is3DFloor { get; init; }
        public bool CeilingIsSky { get; init; }
        public bool FloorIsSky { get; init; }
        public double BounceFactor { get; init; } = 0.7;
        public double MbfBounceFactor { get; init; } = 0.7;
        public bool Destroyed { get; set; }
        public bool Explode { get; set; }
        public bool CallDie { get; set; }
        public bool ReflectHeretic { get; set; }
        public bool ReflectMbf { get; set; }
        public bool ReflectDoom { get; set; }
        public bool EnterDeathState { get; set; }
        public bool UpdateAngle { get; set; }
        public bool NegatePitch { get; set; }
        public bool UseCeilingBounceState { get; set; }
        public bool UseFloorBounceState { get; set; }
        public double ReflectedScale { get; set; } = 1;
    }

    public static bool RunFloorBounceMissile(FloorBounceContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.IsMissile && ctx.SpecialBounceResult is int special)
        {
            var outcome = SpecialBounceMissileOutcome(special);
            if (outcome is bool destroyed)
            {
                ctx.Destroyed = destroyed;
                return destroyed;
            }
        }

        if (ctx.PlaneIsDamageable && ctx.BounceCount > 0)
        {
            ctx.BounceCount = 0;
            ctx.Destroyed = true;
            if (ctx.IsMissile)
                ctx.Explode = true;
            else
                ctx.CallDie = true;
            return true;
        }

        if (ctx.InLiquid)
        {
            if ((ctx.BounceFlags & BounceExplodeOnWater) != 0)
            {
                ctx.Destroyed = true;
                if (ctx.IsMissile)
                    ctx.Explode = true;
                else
                    ctx.CallDie = true;
                return true;
            }

            if ((ctx.BounceFlags & BounceCanBounceWater) == 0)
            {
                ctx.Destroyed = true;
                return true;
            }
        }

        var normalZ = ctx.Is3DFloor ? -ctx.NormalZ : ctx.NormalZ;
        var onCeiling = normalZ < 0;
        if (onCeiling)
        {
            if ((ctx.BounceFlags & BounceCeilings) == 0)
            {
                ctx.Destroyed = true;
                return true;
            }
        }
        else if ((ctx.BounceFlags & BounceFloors) == 0)
        {
            ctx.Destroyed = true;
            return true;
        }

        var onSky = onCeiling ? ctx.CeilingIsSky : ctx.FloorIsSky;
        if (onSky && (ctx.BounceFlags & BounceNotOnSky) != 0)
        {
            ctx.Destroyed = true;
            return true;
        }

        if (ctx.BounceCount > 0 && --ctx.BounceCount == 0)
        {
            ctx.Destroyed = true;
            if (ctx.IsMissile)
                ctx.Explode = true;
            else
                ctx.CallDie = true;
            return true;
        }

        var hereticFamily = (ctx.BounceFlags & (BounceHereticType | BounceMbf)) != 0;
        ctx.UpdateAngle = (ctx.BounceFlags & BounceKeepAngle) == 0;
        if (hereticFamily)
        {
            ctx.ReflectHeretic = (ctx.BounceFlags & BounceMbf) == 0;
            ctx.ReflectMbf = (ctx.BounceFlags & BounceMbf) != 0;
            if (ctx.ReflectHeretic)
            {
                ctx.Flags |= MfInBounce;
                ctx.EnterDeathState = true;
                ctx.Flags &= ~MfInBounce;
                ctx.ReflectedScale = 1;
                return false;
            }

            ctx.ReflectedScale = ctx.MbfBounceFactor;
        }
        else
        {
            ctx.ReflectDoom = true;
            ctx.ReflectedScale = ctx.BounceFactor;
        }

        if ((ctx.BounceFlags & BounceModifyPitch) != 0)
            ctx.NegatePitch = true;

        if ((ctx.BounceFlags & BounceUseBounceState) != 0)
        {
            ctx.UseCeilingBounceState = onCeiling;
            ctx.UseFloorBounceState = !onCeiling;
        }

        return false;
    }
}
