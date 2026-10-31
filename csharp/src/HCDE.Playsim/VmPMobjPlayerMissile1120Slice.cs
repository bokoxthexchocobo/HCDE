namespace HCDE.Playsim;

/// <summary>
/// <c>P_SpawnSubMissile</c> pitch tail and <c>P_SpawnPlayerMissile</c> aim/spawn through the VM prologue
/// from <c>p_mobj.cpp</c> (**8201–8350**). Chunk **1120**.
/// </summary>
public static class VmPMobjPlayerMissile1120Slice
{
    public const int AlfIgnoreNoAutoAim = 1;
    public const int AlfNoWeaponCheck = 2;
    public const int WifNoAutoAim = 4;
    public const int Mf3FloorHugger = 8;
    public const int Mf3CeilingHugger = 16;
    public const int Mf4Spectral = 32;

    public static readonly double[] AimAngleOffsets = { -5.625, 5.625, 0 };

    public static double SubMissileVerticalSpeed(double speed, double pitchRadians) =>
        -speed * Math.Sin(pitchRadians);

    public static double AimRange(double maxTargetRange) =>
        maxTargetRange > 0 ? maxTargetRange * 64 : 16 * 64;

    public static double VerticalAimWindow(bool noFreeAim) => noFreeAim ? 35 : 0;

    public sealed class PlayerMissileContext
    {
        public bool HasSource { get; init; } = true;
        public bool HasType { get; init; } = true;
        public bool SuppressLocal { get; init; }
        public int AimFlags { get; set; }
        public bool HasPlayer { get; init; }
        public bool HasReadyWeapon { get; init; }
        public int WeaponFlags { get; init; }
        public bool NoAutoAim { get; init; }
        public bool NoFreeAim { get; init; }
        public bool FreelookAllowed { get; init; }
        public double AimDist { get; init; } = 1;
        public double Angle { get; init; }
        public double PlayerPitch { get; init; }
        public double MaxTargetRange { get; init; }
        public bool FoundLineTarget { get; set; }
        public int SweepSteps { get; set; }
        public double ChosenAngle { get; set; }
        public double ChosenPitch { get; set; }
        public bool ClearedLineTarget { get; set; }
        public double Z { get; set; }
        public bool ZIsFloorSentinel { get; init; }
        public bool ZIsCeilingSentinel { get; init; }
        public double Center { get; init; }
        public double FloorClip { get; init; }
        public double AttackOffset { get; init; }
        public double FloorZ { get; init; }
        public bool ClampedToFloor { get; set; }
        public int Flags3 { get; init; }
        public int Flags4 { get; init; }
        public bool UsePlanarVelocity { get; set; }
        public bool Use3DVelocity { get; set; }
        public bool SpectralFriend { get; set; }
        public bool SpawnCheckPassed { get; init; } = true;
        public bool Spawned { get; set; }
        public bool Aborted { get; set; }
    }

    public static bool RunSpawnPlayerMissile(PlayerMissileContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (!ctx.HasSource || !ctx.HasType || ctx.SuppressLocal)
        {
            ctx.Aborted = true;
            return false;
        }

        ctx.AimFlags &= ~AlfIgnoreNoAutoAim;
        var skipAutoAim = (ctx.AimFlags & AlfNoWeaponCheck) == 0
            && ctx.HasPlayer
            && ctx.HasReadyWeapon
            && ((ctx.WeaponFlags & WifNoAutoAim) != 0 || ctx.NoAutoAim);

        if (skipAutoAim)
        {
            ctx.ChosenAngle = ctx.Angle;
            ctx.ChosenPitch = ctx.PlayerPitch;
            ctx.ClearedLineTarget = true;
        }
        else
        {
            var steps = 0;
            for (var i = 2; i >= 0; i--)
            {
                steps++;
                ctx.ChosenAngle = ctx.Angle + AimAngleOffsets[i];
                ctx.FoundLineTarget = i == 0;
                if (ctx.HasPlayer && !ctx.NoFreeAim && ctx.FreelookAllowed && ctx.AimDist <= 0.5)
                    break;
                if (ctx.FoundLineTarget)
                    break;
            }

            ctx.SweepSteps = steps;
            if (!ctx.FoundLineTarget)
            {
                ctx.ChosenAngle = ctx.Angle;
                if (ctx.NoFreeAim || !ctx.FreelookAllowed)
                    ctx.ChosenPitch = 0;
            }
        }

        if (!ctx.ZIsFloorSentinel && !ctx.ZIsCeilingSentinel)
        {
            ctx.Z += ctx.Center - ctx.FloorClip + ctx.AttackOffset;
            if (ctx.Z < ctx.FloorZ)
            {
                ctx.Z = ctx.FloorZ;
                ctx.ClampedToFloor = true;
            }
        }

        if ((ctx.Flags3 & (Mf3FloorHugger | Mf3CeilingHugger)) != 0)
            ctx.UsePlanarVelocity = true;
        else
            ctx.Use3DVelocity = true;

        if ((ctx.Flags4 & Mf4Spectral) != 0)
            ctx.SpectralFriend = true;

        if (!ctx.SpawnCheckPassed)
        {
            ctx.Aborted = true;
            return false;
        }

        ctx.Spawned = true;
        return true;
    }
}
