namespace HCDE.Playsim;

/// <summary><c>P_LineAttack</c> puff lookup, <c>P_LineTrace</c>, and <c>P_LinePickActor</c> from <c>p_map.cpp</c> (5052–5237).</summary>
public static class VmPMapLineTracePick
{
    public enum ETraceStatus { Stop, Skip }

    public sealed class LineTraceData
    {
        public object? HitActor { get; set; }
        public object? HitLine { get; set; }
        public object? HitSector { get; set; }
        public object? Hit3DFloor { get; set; }
        public int SectorPlane { get; set; }
        public int LineSide { get; set; }
        public int LinePart { get; set; }
        public int HitTexture { get; set; }
        public (double X, double Y, double Z) HitLocation { get; set; }
        public (double X, double Y, double Z) HitDir { get; set; }
        public double Distance { get; set; }
        public int NumPortals { get; set; }
        public int HitType { get; set; }
    }

    public sealed class TraceResults
    {
        public int HitType { get; set; }
        public object? Actor { get; set; }
        public object? Line { get; set; }
        public object? Sector { get; set; }
        public object? Ffloor { get; set; }
        public int Side { get; set; }
        public int Tier { get; set; }
        public int HitTexture { get; set; }
        public (double X, double Y, double Z) HitPos { get; set; }
        public (double X, double Y, double Z) HitVector { get; set; }
        public double Distance { get; set; }
        public int FfloorFlags { get; set; }
        public object? FfloorMasterSidedefTextures { get; set; }
    }

    public sealed class CheckLineData
    {
        public object? Caller { get; set; }
        public bool ThruSpecies { get; set; }
        public bool ThruActors { get; set; }
        public int NumPortals { get; set; }
        public Func<object?, object?, bool>? SameSpecies { get; init; }
    }

    public sealed class LineAttackContext
    {
        public string PuffTypeName { get; init; } = "";
        public Func<string, object?>? FindActorClass { get; init; }
        public Action<string>? PrintUnknown { get; init; }
        public Func<LineAttackContext, object?>? LineAttackWithClass { get; init; }
        public object? VictimOut { get; set; }
    }

    public static object? LineAttackByPuffName(LineAttackContext ctx)
    {
        var type = ctx.FindActorClass?.Invoke(ctx.PuffTypeName);
        if (type is null)
        {
            ctx.VictimOut = new object();
            ctx.PrintUnknown?.Invoke(ctx.PuffTypeName);
            return null;
        }

        return ctx.LineAttackWithClass?.Invoke(ctx);
    }

    public static ETraceStatus CheckLineTrace(TraceResults res, CheckLineData data)
    {
        if (res.HitType == VmPMapLineTrace.TraceCrossingPortal)
        {
            data.NumPortals++;
            return ETraceStatus.Skip;
        }

        if (res.HitType != VmPMapLineTrace.TraceHitActor)
            return ETraceStatus.Stop;

        if (data.ThruActors || data.ThruSpecies && data.SameSpecies?.Invoke(res.Actor, data.Caller) == true)
            return ETraceStatus.Skip;

        return ETraceStatus.Stop;
    }

    public sealed class LineTraceContext
    {
        public int Flags { get; init; }
        public double Distance { get; init; }
        public double Sz { get; init; }
        public double OffsetForward { get; init; }
        public double OffsetSide { get; init; }
        public double PitchCos { get; init; }
        public double PitchSin { get; init; }
        public double AngleCos { get; init; }
        public double AngleSin { get; init; }
        public double StartZ { get; init; }
        public double PosX { get; init; }
        public double PosY { get; init; }
        public bool Missile { get; init; }
        public bool BounceMbf { get; init; }
        public bool NoblockMonst { get; init; }
        public bool CompatNoBlockFriends { get; init; }
        public bool Friendly { get; init; }
        public bool Float { get; init; }
        public bool BlockAsPlayer { get; init; }
        public bool IsPlayer { get; init; }
        public object? Caller { get; init; }
        public object? StartSector { get; init; }
        public Func<LineTraceTraceRequest, bool>? Trace { get; init; }
    }

    public sealed class LineTraceTraceRequest
    {
        public double StartX { get; init; }
        public double StartY { get; init; }
        public double StartZ { get; init; }
        public object? StartSector { get; init; }
        public (double X, double Y, double Z) Direction { get; init; }
        public double Distance { get; init; }
        public int ActorFlags { get; init; }
        public int LineFlags { get; init; }
        public object? Caller { get; init; }
        public TraceResults Trace { get; init; } = new();
        public int TraceFlags { get; init; }
        public CheckLineData CallbackData { get; init; } = new();
    }

    public static bool LineTrace(LineTraceContext ctx, LineTraceData? outdata)
    {
        var data = new CheckLineData
        {
            Caller = ctx.Caller,
            ThruSpecies = (ctx.Flags & VmPMapLineTrace.TrfThrucpecies) != 0,
            ThruActors = (ctx.Flags & VmPMapLineTrace.TrfThruactors) != 0,
        };

        var direction = (ctx.PitchCos * ctx.AngleCos, ctx.PitchCos * ctx.AngleSin, -ctx.PitchSin);
        var start = VmPMapLineTrace.ComputeTraceStart(new VmPMapLineTrace.StartPosContext(
            ctx.Flags, ctx.OffsetForward, ctx.OffsetSide, ctx.AngleSin, ctx.AngleCos, ctx.StartZ));
        var absPos = VmPMapLineTrace.UseAbsPosition(ctx.Flags);
        var startX = absPos ? start.X : ctx.PosX + start.X;
        var startY = absPos ? start.Y : ctx.PosY + start.Y;
        var startZ = absPos ? ctx.Sz : start.Z;

        var aflags = VmPMapLineTrace.BuildActorFlags(ctx.Flags);

        var projectile = VmPMapLineTrace.ProjectileCaller(ctx.Missile, ctx.BounceMbf);
        var notBlocked = VmPMapLineTrace.NotBlockedMonsters(ctx.NoblockMonst, ctx.CompatNoBlockFriends, ctx.Friendly);
        var lflags = VmPMapLineTrace.BuildLineFlags(ctx.Flags, projectile, notBlocked, ctx.IsPlayer || ctx.BlockAsPlayer);
        if ((ctx.Flags & VmPMapLineTrace.TrfBlockself) != 0 && ctx.Float)
            lflags |= VmPMapLineTrace.MlBlockFloaters;

        var tflags = VmPMapLineTrace.TraceFlagsBase(true, (ctx.Flags & VmPMapLineTrace.TrfNosky) != 0);
        var trace = new TraceResults();
        var ret = ctx.Trace?.Invoke(new LineTraceTraceRequest
        {
            StartX = startX,
            StartY = startY,
            StartZ = startZ,
            StartSector = ctx.StartSector,
            Direction = direction,
            Distance = ctx.Distance,
            ActorFlags = aflags,
            LineFlags = lflags,
            Caller = ctx.Caller,
            Trace = trace,
            TraceFlags = tflags,
            CallbackData = data,
        }) == true;

        if (outdata is not null)
        {
            outdata.HitActor = trace.Actor;
            outdata.HitLine = trace.Line;
            outdata.HitSector = trace.Sector;
            outdata.Hit3DFloor = trace.Ffloor;
            outdata.SectorPlane = VmPMapLineTrace.CeilingPlaneFlag(trace.HitType);
            if (trace.HitType == VmPMapLineTrace.TraceHitWall)
            {
                outdata.LineSide = trace.Side;
                outdata.LinePart = VmPMapLineTrace.WallLinePart(trace.Tier);
                outdata.HitTexture = trace.HitTexture;
            }
            else
                outdata.HitTexture = trace.HitTexture;

            outdata.HitLocation = trace.HitPos;
            outdata.HitDir = trace.HitVector;
            outdata.Distance = trace.Distance;
            outdata.NumPortals = VmPMapLineTrace.PortalCountAdjust(data.NumPortals);
            outdata.HitType = trace.HitType;
        }

        return ret;
    }

    public sealed class LinePickContext
    {
        public double PitchCos { get; init; }
        public double PitchSin { get; init; }
        public double AngleCos { get; init; }
        public double AngleSin { get; init; }
        public double Center { get; init; }
        public double Floorclip { get; init; }
        public double AttackOffset { get; init; }
        public double PosX { get; init; }
        public double PosY { get; init; }
        public object? Sector { get; init; }
        public double Distance { get; init; }
        public int ActorMask { get; init; }
        public uint WallMask { get; init; }
        public object? Caller { get; init; }
        public Func<double, double, double, object?, (double X, double Y, double Z), double, int, uint, object?, TraceResults, int, Func<TraceResults, object?, bool>, object?, bool>? Trace { get; init; }
    }

    public static object? LinePickActor(LinePickContext ctx)
    {
        var dir = VmPMapLineTrace.LinePickDirection(new VmPMapLineTrace.LinePickContext(
            ctx.PitchCos, ctx.PitchSin, ctx.AngleCos, ctx.AngleSin, ctx.Center, ctx.Floorclip, ctx.AttackOffset));
        var shootZ = VmPMapLineTrace.LinePickShootZ(new VmPMapLineTrace.LinePickContext(
            ctx.PitchCos, ctx.PitchSin, ctx.AngleCos, ctx.AngleSin, ctx.Center, ctx.Floorclip, ctx.AttackOffset));

        var trace = new TraceResults();
        if (ctx.Trace?.Invoke(ctx.PosX, ctx.PosY, shootZ, ctx.Sector, dir, ctx.Distance, ctx.ActorMask, ctx.WallMask, ctx.Caller, trace, 2, (_, _) => true, null) == true &&
            VmPMapLineTrace.LinePickHitActor(trace.HitType))
            return trace.Actor;
        return null;
    }
}
