namespace HCDE.Playsim;

/// <summary><c>SightCheck::init</c> and <c>P_ResetSightCounters</c> from <c>p_sight.cpp</c> (126–145, 962–972).</summary>
public static class VmPSightInit
{
    public sealed class SightInitContext
    {
        public double SourceX { get; set; }
        public double SourceY { get; set; }
        public double SourceZ { get; set; }
        public double SourceHeight { get; set; }
        public double TargetX { get; set; }
        public double TargetY { get; set; }
        public double TargetZ { get; set; }
        public double TargetHeight { get; set; }
        public int PortalGroup { get; set; }
        public double StartFrac { get; set; }
        public double TopSlope { get; set; }
        public double BottomSlope { get; set; }
        public int Flags { get; set; }
        public int PortalDirection { get; set; }
        public bool PortalFound { get; set; }
        public uint SeeThroughFlags { get; set; } = 1;

        public double TraceDx { get; set; }
        public double TraceDy { get; set; }
        public double LastZTop { get; set; }
        public double LastZBottom { get; set; }
    }

    public static void InitSightCheck(SightInitContext ctx)
    {
        var eyeZ = ctx.SourceZ + ctx.SourceHeight * 0.75;
        ctx.SourceZ = eyeZ;

        ctx.TraceDx = ctx.TargetX - ctx.SourceX;
        ctx.TraceDy = ctx.TargetY - ctx.SourceY;
        ctx.LastZTop = ctx.LastZBottom = eyeZ;
        ctx.PortalFound = false;
        ctx.SeeThroughFlags = 1;
    }

    public sealed class SightCounterState
    {
        public double CycleMs { get; set; }
        public double MaxCycleMs { get; set; }
    }

    public static void ResetSightCounters(SightCounterState state, bool full)
    {
        if (full)
            state.MaxCycleMs = 0;

        if (state.CycleMs > state.MaxCycleMs)
            state.MaxCycleMs = state.CycleMs;
    }
}
