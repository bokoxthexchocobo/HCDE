namespace HCDE.Playsim;

/// <summary>
/// <c>P_OldSpawnMissile</c> body, <c>P_SpawnMissileZAimed</c>, <c>P_SpawnMissileAngleZSpeed</c>, and the
/// <c>P_SpawnSubMissile</c> friend-player head from <c>p_mobj.cpp</c> (**8051–8200**). Chunk **1119**.
/// </summary>
public static class VmPMobjMissileAngles1119Slice
{
    public const int Mf4Spectral = 1;
    public const int MfMissile = 2;

    public static double OldMissileVerticalSpeed(double destZ, double sourceZ, double speed)
    {
        var dist = Math.Max(1.0, speed);
        return (destZ - sourceZ) / dist;
    }

    public static double AimedVerticalSpeed(double distance2D, double speed, double destZ, double sourceZ)
    {
        if (speed == 0)
            return 0;
        var dist = distance2D / speed;
        return dist != 0 ? (destZ - sourceZ) / dist : speed;
    }

    public sealed class AngleSpeedContext
    {
        public bool HasSource { get; init; }
        public bool HasType { get; init; }
        public double Z { get; set; }
        public bool ZIsFloorSentinel { get; init; }
        public bool ZIsCeilingSentinel { get; init; }
        public double FloorClip { get; init; }
        public bool HasOwner { get; init; }
        public bool OwnerDefaultsToSource { get; set; }
        public double Yaw { get; set; }
        public double AimAngle { get; init; }
        public double VerticalSpeed { get; set; }
        public double IncomingVz { get; init; }
        public int Flags4 { get; init; }
        public bool SpectralFriend { get; set; }
        public bool CheckSpawn { get; init; }
        public bool SpawnCheckPassed { get; init; } = true;
        public bool Aborted { get; set; }
    }

    public static bool RunSpawnMissileAngleZSpeed(AngleSpeedContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (!ctx.HasSource || !ctx.HasType)
        {
            ctx.Aborted = true;
            return false;
        }

        if (!ctx.ZIsFloorSentinel && !ctx.ZIsCeilingSentinel)
            ctx.Z -= ctx.FloorClip;

        if (!ctx.HasOwner)
            ctx.OwnerDefaultsToSource = true;

        ctx.Yaw = ctx.AimAngle;
        ctx.VerticalSpeed = ctx.IncomingVz;
        if ((ctx.Flags4 & Mf4Spectral) != 0)
            ctx.SpectralFriend = true;

        if (ctx.CheckSpawn && !ctx.SpawnCheckPassed)
        {
            ctx.Aborted = true;
            return false;
        }

        return true;
    }

    public static bool RunOldSpawnMissile(AngleSpeedContext ctx, double destZ, double sourceZ, double speed)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (!ctx.HasSource || !ctx.HasType)
        {
            ctx.Aborted = true;
            return false;
        }

        ctx.Z = sourceZ + 32;
        if (!ctx.HasOwner)
            ctx.OwnerDefaultsToSource = true;
        ctx.VerticalSpeed = OldMissileVerticalSpeed(destZ, sourceZ, speed);
        if ((ctx.Flags4 & Mf4Spectral) != 0)
            ctx.SpectralFriend = true;
        return true;
    }

    public sealed class SubMissileContext
    {
        public bool SourceIsMissile { get; init; }
        public bool SourceSpectral { get; init; }
        public bool MissileSpectral { get; init; }
        public int SourceFriendPlayer { get; init; }
        public int TargetFriendPlayer { get; init; }
        public int FriendPlayer { get; set; }
        public bool CopiedSourceFriend { get; set; }
        public bool UsedTargetPlayer { get; set; }
    }

    public static void ApplySubMissileFriend(SubMissileContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (!ctx.MissileSpectral)
            return;

        if (ctx.SourceIsMissile && ctx.SourceSpectral)
        {
            ctx.FriendPlayer = ctx.SourceFriendPlayer;
            ctx.CopiedSourceFriend = true;
            return;
        }

        ctx.FriendPlayer = ctx.TargetFriendPlayer;
        ctx.UsedTargetPlayer = true;
    }
}
