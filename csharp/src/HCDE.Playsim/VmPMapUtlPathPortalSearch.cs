namespace HCDE.Playsim;

/// <summary>Path portal relocation, FOV check, and blockmap search from <c>p_maputl.cpp</c> (1612–1819).</summary>
public static class VmPMapUtlPathPortalSearch
{
    public sealed class TraceState
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Dx { get; set; }
        public double Dy { get; set; }
    }

    public sealed class LineIntercept
    {
        public bool IsLine { get; set; }
        public object? Line { get; set; }
        public double Frac { get; set; }
    }

    public sealed class PortalRelocateContext
    {
        public TraceState Trace { get; } = new();
        public LineIntercept Intercept { get; } = new();
        public int InterceptIndex { get; set; }
        public List<LineIntercept> Intercepts { get; } = new();
        public Func<object?, double, double, bool>? IsLinePortal { get; init; }
        public Func<object?, double, double, int>? PointOnLineSide { get; init; }
        public Action<object?, ref double, ref double>? TranslatePortalXy { get; init; }
        public Action<object?, ref double>? TranslatePortalZ { get; init; }
        public Action<double, double, double, double, int, double>? ReinitTrace { get; init; }
        public Func<object?, int>? PortalType { get; init; }
    }

    public static int PortalRelocate(PortalRelocateContext ctx, int flags, ref double optZ, bool hasOptPos)
    {
        var inLine = ctx.Intercept;
        if (!inLine.IsLine || ctx.IsLinePortal?.Invoke(inLine.Line, ctx.Trace.X, ctx.Trace.Y) != true)
            return 0;
        if (ctx.PointOnLineSide?.Invoke(inLine.Line, ctx.Trace.X, ctx.Trace.Y) == 1)
            return 0;

        var hitX = ctx.Trace.X;
        var hitY = ctx.Trace.Y;
        var endX = ctx.Trace.X + ctx.Trace.Dx;
        var endY = ctx.Trace.Y + ctx.Trace.Dy;
        ctx.TranslatePortalXy?.Invoke(inLine.Line, ref hitX, ref hitY);
        ctx.TranslatePortalXy?.Invoke(inLine.Line, ref endX, ref endY);
        if (hasOptPos)
        {
            var optX = hitX;
            var optY = hitY;
            var z = optZ;
            ctx.TranslatePortalXy?.Invoke(inLine.Line, ref optX, ref optY);
            ctx.TranslatePortalZ?.Invoke(inLine.Line, ref z);
            optZ = z;
        }

        var saved = inLine.Line;
        ctx.Intercepts.RemoveRange(ctx.InterceptIndex, ctx.Intercepts.Count - ctx.InterceptIndex);
        const double epsilon = 1e-6;
        ctx.ReinitTrace?.Invoke(hitX, hitY, endX, endY, flags, inLine.Frac + epsilon);
        return ctx.PortalType?.Invoke(saved) == 1 ? 1 : -1;
    }

    public static void PortalRelocateByDisplacement(TraceState trace, List<LineIntercept> intercepts, int interceptIndex, double dispX, double dispY, int flags, double hitFrac, Action<double, double, double, double, int, double> reinit)
    {
        var hitX = trace.X + dispX;
        var hitY = trace.Y + dispY;
        var endX = hitX + trace.Dx;
        var endY = hitY + trace.Dy;
        intercepts.RemoveRange(interceptIndex, intercepts.Count - interceptIndex);
        reinit(hitX, hitY, endX, endY, flags, hitFrac);
    }

    public static void TrimIntercepts(List<LineIntercept> intercepts, int interceptIndex) =>
        intercepts.RemoveRange(interceptIndex, intercepts.Count - interceptIndex);

    public static bool CheckFov(double yawDegrees, double angleToTargetDegrees, double fovDegrees)
    {
        var delta = Math.Abs(NormalizeAngle(angleToTargetDegrees - yawDegrees));
        return delta <= fovDegrees;
    }

    static double NormalizeAngle(double deg)
    {
        while (deg > 180) deg -= 360;
        while (deg < -180) deg += 360;
        return deg;
    }

    public static object? BlockmapSearch(
        int startBlockX,
        int startBlockY,
        int bmapWidth,
        int bmapHeight,
        int distance,
        Func<int, int, object?> checkCenter,
        Func<int, object?> checkIndex)
    {
        if (checkCenter(startBlockX, startBlockY) is { } immediate)
            return immediate;

        for (var count = 1; count <= distance; count++)
        {
            var blockX = Math.Clamp(startBlockX - count, 0, bmapWidth - 1);
            var blockY = Math.Clamp(startBlockY - count, 0, bmapHeight - 1);
            var blockIndex = blockY * bmapWidth + blockX;
            var firstStop = startBlockX + count;
            if (firstStop < 0) continue;
            if (firstStop >= bmapWidth) firstStop = bmapWidth - 1;
            var secondStop = startBlockY + count;
            if (secondStop < 0) continue;
            if (secondStop >= bmapHeight) secondStop = bmapHeight - 1;
            var thirdStop = secondStop * bmapWidth + blockX;
            secondStop = secondStop * bmapWidth + firstStop;
            firstStop += blockY * bmapWidth;
            var finalStop = blockIndex;

            for (; blockIndex <= firstStop; blockIndex++)
                if (checkIndex(blockIndex) is { } t1) return t1;
            for (blockIndex--; blockIndex <= secondStop; blockIndex += bmapWidth)
                if (checkIndex(blockIndex) is { } t2) return t2;
            for (blockIndex -= bmapWidth; blockIndex >= thirdStop; blockIndex--)
                if (checkIndex(blockIndex) is { } t3) return t3;
            for (blockIndex++; blockIndex > finalStop; blockIndex -= bmapWidth)
                if (checkIndex(blockIndex) is { } t4) return t4;
        }

        return null;
    }

    public sealed class RoughBlockCheckContext
    {
        public bool OnlySeekable { get; set; }
        public bool FrontOnly { get; set; }
        public double Fov { get; set; }
        public double ViewerYaw { get; set; }
        public IReadOnlyList<object> ActorsInBlock { get; init; } = Array.Empty<object>();
        public Func<object, object, bool>? CanSeek { get; init; }
        public Func<object, double, double, int>? PointOnDivlineSide { get; init; }
        public Func<object, object, double, bool>? CheckFov { get; init; }
        public Func<object, object, bool>? IsOkayToAttack { get; init; }
        public Func<object, double>? ActorX { get; init; }
        public Func<object, double>? ActorY { get; init; }
        public double FrontLineX { get; set; }
        public double FrontLineY { get; set; }
        public double FrontLineDx { get; set; }
        public double FrontLineDy { get; set; }
        public object Viewer { get; set; } = new();
    }

    public static object? RoughBlockCheck(RoughBlockCheckContext ctx)
    {
        foreach (var other in ctx.ActorsInBlock)
        {
            if (ReferenceEquals(other, ctx.Viewer))
                continue;
            if (ctx.OnlySeekable && ctx.CanSeek?.Invoke(ctx.Viewer, other) != true)
                continue;
            if (ctx.FrontOnly && ctx.PointOnDivlineSide?.Invoke(other, ctx.FrontLineX, ctx.FrontLineY) != 0)
                continue;
            if (ctx.Fov > 0 && ctx.CheckFov?.Invoke(ctx.Viewer, other, ctx.Fov) == false)
                continue;
            if (ctx.IsOkayToAttack?.Invoke(ctx.Viewer, other) == true)
                return other;
        }

        return null;
    }
}
