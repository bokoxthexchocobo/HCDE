namespace HCDE.Playsim;

/// <summary><c>DPolyDoor::Tick</c>, <c>EV_OpenPolyDoor</c>, <c>EV_StopPoly</c> from <c>po_man.cpp</c> (527–682, 690–712).</summary>
public static class VmPoManPolyDoor
{
    public const int PodoorSlide = 0;
    public const int PodoorSwing = 1;

    public sealed class PolyDoorTickContext
    {
        public int DoorType { get; set; }
        public int Tics { get; set; }
        public bool Close { get; set; }
        public double Dist { get; set; }
        public double TotalDist { get; set; }
        public double Speed { get; set; }
        public double SpeedV { get; set; }
        public int WaitTics { get; set; }
        public int Direction { get; set; }
        public bool Crush { get; set; }
        public bool Destroyed { get; set; }
        public bool BlockedAfterOpen { get; set; }
        public bool SequenceStopped { get; set; }

        public Action? StartWaitSequence { get; init; }
        public Action? StopSequence { get; init; }
        public Action? StartOpenSequence { get; init; }
        public Func<bool>? MovePolyobj { get; init; }
        public Func<bool>? RotatePolyobj { get; init; }
        public Action? UpdateLinks { get; init; }
        public Action? Destroy { get; init; }
    }

    public static void PolyDoorTick(PolyDoorTickContext ctx)
    {
        if (ctx.Tics > 0)
        {
            ctx.Tics--;
            if (ctx.Tics == 0)
                ctx.StartWaitSequence?.Invoke();
            return;
        }

        switch (ctx.DoorType)
        {
            case PodoorSlide:
                TickSlide(ctx);
                break;
            case PodoorSwing:
                TickSwing(ctx);
                break;
        }
    }

    static void TickSlide(PolyDoorTickContext ctx)
    {
        if (ctx.Dist <= 0 || ctx.MovePolyobj?.Invoke() == true)
        {
            var absSpeed = Math.Abs(ctx.SpeedV);
            ctx.Dist -= absSpeed;
            if (ctx.Dist <= 0)
            {
                ctx.StopSequence?.Invoke();
                if (!ctx.Close && ctx.WaitTics >= 0)
                {
                    ctx.Dist = ctx.TotalDist;
                    ctx.Close = true;
                    ctx.Tics = ctx.WaitTics;
                    ctx.Direction = -ctx.Direction;
                    ctx.SpeedV = -ctx.SpeedV;
                }
                else
                {
                    if (!ctx.Close) ctx.BlockedAfterOpen = true;
                    ctx.Destroyed = true;
                    ctx.Destroy?.Invoke();
                }
            }

            ctx.UpdateLinks?.Invoke();
        }
        else if (!ctx.Crush && ctx.Close)
        {
            ctx.Dist = ctx.TotalDist - ctx.Dist;
            ctx.Direction = -ctx.Direction;
            ctx.SpeedV = -ctx.SpeedV;
            ctx.Close = false;
            ctx.StartOpenSequence?.Invoke();
        }
    }

    static void TickSwing(PolyDoorTickContext ctx)
    {
        if (ctx.Dist <= 0 || ctx.RotatePolyobj?.Invoke() == true)
        {
            var absSpeed = Math.Abs(ctx.Speed);
            ctx.Dist -= absSpeed;
            if (ctx.Dist <= 0)
            {
                ctx.StopSequence?.Invoke();
                if (!ctx.Close && ctx.WaitTics >= 0)
                {
                    ctx.Dist = ctx.TotalDist;
                    ctx.Close = true;
                    ctx.Tics = ctx.WaitTics;
                    ctx.Speed = -ctx.Speed;
                }
                else
                {
                    if (!ctx.Close) ctx.BlockedAfterOpen = true;
                    ctx.Destroyed = true;
                    ctx.Destroy?.Invoke();
                }
            }
        }
        else if (!ctx.Crush && ctx.Close)
        {
            ctx.Dist = ctx.TotalDist - ctx.Dist;
            ctx.Speed = -ctx.Speed;
            ctx.Close = false;
            ctx.StartOpenSequence?.Invoke();
        }
    }

    public sealed class OpenPolyDoorContext
    {
        public int DoorType { get; set; }
        public double Speed { get; set; }
        public int Delay { get; set; }
        public double Distance { get; set; }
        public double SwingDegrees { get; set; }
        public int SwingDir { get; set; } = 1;
        public bool Started { get; set; }

        public Func<int, PolyMirrorState?>? ResolveMirror { get; init; }
        public Action<PolyMirrorState, OpenPolyDoorContext>? StartDoorOnPoly { get; init; }
    }

    public sealed class PolyMirrorState
    {
        public int PolyId { get; set; }
        /// <summary>Next poly in the mirror chain (<c>GetMirror()</c>); 0 ends the chain.</summary>
        public int MirrorPolyId { get; set; }
        public bool Blocked { get; set; }
        public bool HasSpecialData { get; set; }
        public bool LinkedPortalBlocksSwing { get; set; }
        public int PortalLinkMode { get; set; }
    }

    public static bool OpenPolyDoor(OpenPolyDoorContext ctx, int polyNum)
    {
        if (ctx.ResolveMirror is null)
        {
            ctx.Started = false;
            return false;
        }

        var swingDir = 1;
        PolyMirrorState? lastStarted = null;
        foreach (var mirror in VmPoManMirrorChain.Enumerate(
                     polyNum,
                     ctx.ResolveMirror,
                     s => s.MirrorPolyId))
        {
            if (mirror.HasSpecialData || mirror.Blocked)
                break;
            if (mirror.PortalLinkMode == 2 && ctx.DoorType == PodoorSwing)
                break;

            ctx.StartDoorOnPoly?.Invoke(mirror, ctx);
            lastStarted = mirror;
            swingDir = -swingDir;
        }

        ctx.Started = lastStarted is not null;
        return ctx.Started;
    }

    public static bool StopPoly(bool polyFound, bool hasSpecialData, Action? stop)
    {
        if (!polyFound) return false;
        if (hasSpecialData)
            stop?.Invoke();
        return hasSpecialData;
    }
}
