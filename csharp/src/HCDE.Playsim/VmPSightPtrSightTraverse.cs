namespace HCDE.Playsim;

/// <summary><c>SightCheck::PTR_SightTraverse</c> from <c>p_sight.cpp</c> (210–384).</summary>
public static class VmPSightPtrSightTraverse
{
    public const double LineOpenMin = -32768;
    public const double LineOpenMax = 32768;

    public sealed class SightTraverseContext
    {
        public double TraceX { get; set; }
        public double TraceDx { get; set; }
        public double TraceY { get; set; }
        public double TraceDy { get; set; }
        public double SightStartZ { get; set; }
        public double TopSlope { get; set; }
        public double BottomSlope { get; set; }
        public int PortalDir { get; set; }
        public bool CompatTrace { get; set; }
        public bool SameSectorLine { get; set; }
        public bool LineBlocksSight { get; set; }
        public double InterceptFrac { get; set; }
        public double OpenBottom { get; set; }
        public double OpenTop { get; set; }
        public double OpenRange { get; set; }
        public int OpenPortalFlags { get; set; }
        public bool HasLinkedPortal { get; set; }
        public int LinkedPortalGroup { get; set; }
        public bool HasBacksector { get; set; }
        public int FrontFlag { get; set; } = -1;
        public bool Front3dFloors { get; set; }
        public bool Back3dFloors { get; set; }
        public List<(double Frac, double Top, double Bottom, int Plane, int Group)> PortalQueue { get; } = new();

        public Func<double, double, double, (double Bottom, double Top, double Range, int PortalFlags)>? SightOpening { get; init; }
        public Func<double, double, int>? PointOnLineSidePrecise { get; init; }
        public Func<int, int, int>? OppositePortalGroup { get; init; }
        public Func<int, int, bool>? Evaluate3dFloors { get; init; }
    }

    public static bool PtrSightTraverse(SightTraverseContext ctx)
    {
        if (ctx.CompatTrace && ctx.SameSectorLine)
            return true;

        var trX = ctx.TraceX + ctx.TraceDx * ctx.InterceptFrac;
        var trY = ctx.TraceY + ctx.TraceDy * ctx.InterceptFrac;

        if (ctx.SightOpening is not null)
        {
            var open = ctx.SightOpening(trX, trY, ctx.InterceptFrac);
            ctx.OpenBottom = open.Bottom;
            ctx.OpenTop = open.Top;
            ctx.OpenRange = open.Range;
            ctx.OpenPortalFlags = open.PortalFlags;
        }

        if (ctx.LineBlocksSight)
        {
            ctx.OpenRange = 0;
            if (ctx.OpenBottom != LineOpenMin)
                ctx.OpenBottom = ctx.OpenTop;
        }

        if (ctx.OpenRange == 0 && ctx.OpenPortalFlags == 0 && !ctx.HasLinkedPortal)
            return false;

        if (ctx.InterceptFrac == 0)
            return true;

        if (ctx.OpenBottom > LineOpenMin)
        {
            var slope = (ctx.OpenBottom - ctx.SightStartZ) / ctx.InterceptFrac;
            if (slope > ctx.BottomSlope)
                ctx.BottomSlope = slope;
        }

        if (ctx.OpenTop < LineOpenMax)
        {
            var slope = (ctx.OpenTop - ctx.SightStartZ) / ctx.InterceptFrac;
            if (slope < ctx.TopSlope)
                ctx.TopSlope = slope;
        }

        if (ctx.OpenPortalFlags != 0)
        {
            ctx.FrontFlag = ctx.PointOnLineSidePrecise?.Invoke(trX, trY) ?? 0;
            var backGroupCeiling = ctx.OppositePortalGroup?.Invoke(1, 0) ?? 0;
            var backGroupFloor = ctx.OppositePortalGroup?.Invoke(0, 0) ?? 0;
            if (ctx.PortalDir != 0 && (ctx.OpenPortalFlags & 2) != 0 && (ctx.OpenPortalFlags & 1) == 0)
                ctx.PortalQueue.Add((ctx.InterceptFrac, ctx.TopSlope, ctx.BottomSlope, 1, backGroupCeiling));
            if (ctx.PortalDir != 1 && (ctx.OpenPortalFlags & 8) != 0 && (ctx.OpenPortalFlags & 4) == 0)
                ctx.PortalQueue.Add((ctx.InterceptFrac, ctx.TopSlope, ctx.BottomSlope, 0, backGroupFloor));
        }

        if (ctx.HasLinkedPortal)
        {
            ctx.PortalQueue.Add((ctx.InterceptFrac, ctx.TopSlope, ctx.BottomSlope, ctx.PortalDir, ctx.LinkedPortalGroup));
            return false;
        }

        if (ctx.TopSlope <= ctx.BottomSlope || ctx.OpenRange == 0)
            return false;

        if (ctx.Front3dFloors || ctx.Back3dFloors)
        {
            if (ctx.FrontFlag == -1)
                ctx.FrontFlag = ctx.PointOnLineSidePrecise?.Invoke(trX, trY) ?? 0;
            if (ctx.Evaluate3dFloors?.Invoke(ctx.FrontFlag, ctx.OpenPortalFlags) == true)
                return false;
        }

        return true;
    }
}
