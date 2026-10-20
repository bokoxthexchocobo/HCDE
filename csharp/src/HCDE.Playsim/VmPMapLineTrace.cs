namespace HCDE.Playsim;

/// <summary><c>P_LineTrace</c> and helpers from <c>p_map.cpp</c> (5052–5237).</summary>
public static class VmPMapLineTrace
{
    public const int TrfThrucpecies = 1;
    public const int TrfThruactors = 1 << 1;
    public const int TrfAllactors = 1 << 2;
    public const int TrfSolidactors = 1 << 3;
    public const int TrfBlockuse = 1 << 4;
    public const int TrfBlockself = 1 << 5;
    public const int TrfThrublock = 1 << 6;
    public const int TrfThruhitscan = 1 << 7;
    public const int TrfNosky = 1 << 8;
    public const int TrfAbsposition = 1 << 9;
    public const int TrfAbsoffset = 1 << 10;
    public const int MfMissile = 1;
    public const int Mf3Noblockmonst = 1;
    public const int MfFriendly = 1;
    public const int MfFloat = 1;
    public const int Mf8Blockasplayer = 1;
    public const int MlBlockeverything = 1;
    public const int MlBlockhitscan = 1 << 1;
    public const int MlBlockuse = 1 << 2;
    public const int MlBlockprojectile = 1 << 3;
    public const int MlBlocking = 1 << 4;
    public const int MlBlockmonsters = 1 << 5;
    public const int MlBlockPlayers = 1 << 6;
    public const int MlBlockFloaters = 1 << 7;
    public const int MfShootable = 1;
    public const int TraceCrossingPortal = 1;
    public const int TraceHitActor = 2;
    public const int TraceHitWall = 3;
    public const int TraceHitCeiling = 4;
    public const int TierMiddle = 1;
    public const int TierUpper = 0;
    public const int TierLower = 2;
    public const int TierFfloor = 3;
    public const int FfUppertexture = 1;
    public const int FfLowertexture = 1 << 1;

    public static bool PuffTypeMissing(bool found) => !found;

    public static ETraceStatus CheckLineTraceHit(int hitType, bool thruActors, bool thruSpecies, bool sameSpecies)
    {
        if (hitType == TraceCrossingPortal) return ETraceStatus.Skip;
        if (hitType != TraceHitActor) return ETraceStatus.Stop;
        if (thruActors || thruSpecies && sameSpecies) return ETraceStatus.Skip;
        return ETraceStatus.Stop;
    }

    public enum ETraceStatus { Stop, Skip }

    public static int BuildActorFlags(int flags)
    {
        if ((flags & TrfAllactors) != 0) return int.MaxValue;
        var aflags = MfShootable;
        if ((flags & TrfSolidactors) != 0) aflags |= 1 << 1;
        return aflags;
    }

    public static int BuildLineFlags(int flags, bool projectile, bool notBlocked, bool isPlayer)
    {
        var lflags = MlBlockeverything | MlBlockhitscan;
        if ((flags & TrfBlockuse) != 0) lflags |= MlBlockuse;
        if ((flags & TrfBlockself) != 0)
        {
            if (projectile) lflags |= MlBlockprojectile;
            if (!projectile || isPlayer) lflags |= MlBlocking;
            if (!notBlocked) lflags |= MlBlockmonsters;
            if (isPlayer) lflags |= MlBlocking | MlBlockPlayers;
        }
        if ((flags & TrfThrublock) != 0) lflags &= ~MlBlockeverything;
        if ((flags & TrfThruhitscan) != 0) lflags &= ~MlBlockhitscan;
        return lflags;
    }

    public static bool ProjectileCaller(bool missile, bool bounceMbf) => missile || bounceMbf;

    public static bool NotBlockedMonsters(bool noblockMonst, bool compatNoBlockFriends, bool friendly) =>
        noblockMonst || compatNoBlockFriends && friendly;

    public static int TraceFlagsBase(bool reportPortals, bool noSky)
    {
        var t = reportPortals ? 1 : 0;
        if (noSky) t |= 2;
        return t;
    }

    public readonly record struct StartPosContext(
        int Flags,
        double OffsetForward,
        double OffsetSide,
        double Sin,
        double Cos,
        double StartZ);

    public static bool UseAbsPosition(int flags) => (flags & TrfAbsposition) != 0;

    public static bool UseAbsOffset(int flags) => (flags & TrfAbsoffset) != 0;

    public static bool ZeroOffsets(double forward, double side) => forward == 0 && side == 0;

    public static (double X, double Y, double Z) ComputeTraceStart(StartPosContext ctx)
    {
        if (UseAbsPosition(ctx.Flags)) return (ctx.OffsetForward, ctx.OffsetSide, ctx.StartZ);
        if (UseAbsOffset(ctx.Flags)) return (ctx.OffsetForward, ctx.OffsetSide, ctx.StartZ);
        if (ZeroOffsets(ctx.OffsetForward, ctx.OffsetSide)) return (0, 0, ctx.StartZ);
        var x = ctx.OffsetForward * ctx.Cos + ctx.OffsetSide * ctx.Sin;
        var y = ctx.OffsetForward * ctx.Sin - ctx.OffsetSide * ctx.Cos;
        return (x, y, ctx.StartZ);
    }

    public static int PortalCountAdjust(int numPortals) => numPortals - 2;

    public static int WallLinePart(int tier) =>
        tier switch
        {
            TierUpper => 0,
            TierMiddle => 1,
            TierLower => 2,
            TierFfloor => 1,
            _ => 1,
        };

    public static int FfloorTexturePart(int ffFlags) =>
        (ffFlags & FfUppertexture) != 0 ? 0 : (ffFlags & FfLowertexture) != 0 ? 2 : 1;

    public static int CeilingPlaneFlag(int hitType) => hitType == TraceHitCeiling ? 1 : 0;

    public readonly record struct LinePickContext(
        double PitchCos,
        double PitchSin,
        double AngleCos,
        double AngleSin,
        double Center,
        double Floorclip,
        double AttackOffset);

    public static (double Dx, double Dy, double Dz) LinePickDirection(LinePickContext ctx) =>
        (ctx.PitchCos * ctx.AngleCos, ctx.PitchCos * ctx.AngleSin, -ctx.PitchSin);

    public static double LinePickShootZ(LinePickContext ctx) =>
        ctx.Center - ctx.Floorclip + ctx.AttackOffset;

    public static bool LinePickHitActor(int hitType) => hitType == TraceHitActor;
}
