namespace HCDE.Playsim;

/// <summary><c>FPathTraverse::AddThingIntercepts</c> from <c>p_maputl.cpp</c> (1194–1361).</summary>
public static class VmPMapUtlAddThingIntercepts
{
    public readonly record struct Divline(double X, double Y, double Dx, double Dy);

    public sealed class ThingIntercept
    {
        public double Frac { get; set; }
        public bool IsALine { get; set; }
        public bool Done { get; set; }
        public object? Thing { get; set; }
    }

    public sealed class AddThingInterceptsContext
    {
        public Divline Trace { get; set; }
        public double StartFrac { get; set; }
        public List<ThingIntercept> Intercepts { get; } = new();
        public IReadOnlyList<ThingBox> Things { get; set; } = Array.Empty<ThingBox>();

        public Func<double, double, Divline, int>? PointOnDivlineSide { get; init; }
        public Func<Divline, Divline, double>? InterceptVector { get; init; }
    }

    public readonly record struct ThingBox(double X, double Y, double Radius);

    public static bool TraceHasPositiveSlope(Divline trace) => trace.Dx * trace.Dy > 0;

    public static bool DivlineEndpointsOnSameTraceSide(AddThingInterceptsContext ctx, Divline line)
    {
        var sideA = ctx.PointOnDivlineSide?.Invoke(line.X, line.Y, ctx.Trace) ?? 0;
        var sideB = ctx.PointOnDivlineSide?.Invoke(line.X + line.Dx, line.Y + line.Dy, ctx.Trace) ?? 0;
        return sideA == sideB;
    }

    public static (double X1, double Y1, double X2, double Y2) CompatibleDiagonalCorners(ThingBox thing, bool tracePositive)
    {
        if (tracePositive)
            return (thing.X - thing.Radius, thing.Y + thing.Radius, thing.X + thing.Radius, thing.Y - thing.Radius);
        return (thing.X - thing.Radius, thing.Y - thing.Radius, thing.X + thing.Radius, thing.Y + thing.Radius);
    }

    public static bool TryBuildThingEdgeDivline(
        int edgeIndex,
        ThingBox thing,
        Divline trace,
        out Divline edge)
    {
        switch (edgeIndex)
        {
            case 0:
                var topY = thing.Y + thing.Radius;
                if (trace.Y < topY)
                {
                    edge = default;
                    return false;
                }

                edge = new Divline(thing.X + thing.Radius, topY, -thing.Radius * 2, 0);
                return true;
            case 1:
                var rightX = thing.X + thing.Radius;
                if (trace.X < rightX)
                {
                    edge = default;
                    return false;
                }

                edge = new Divline(rightX, thing.Y - thing.Radius, 0, thing.Radius * 2);
                return true;
            case 2:
                var bottomY = thing.Y - thing.Radius;
                if (trace.Y > bottomY)
                {
                    edge = default;
                    return false;
                }

                edge = new Divline(thing.X - thing.Radius, bottomY, thing.Radius * 2, 0);
                return true;
            default:
                var leftX = thing.X - thing.Radius;
                if (trace.X > leftX)
                {
                    edge = default;
                    return false;
                }

                edge = new Divline(leftX, thing.Y + thing.Radius, 0, -thing.Radius * 2);
                return true;
        }
    }

    public static bool TryAcceptBehindStartIntercept(
        AddThingInterceptsContext ctx,
        ref Divline line,
        int edgeIndex,
        ThingBox thing,
        double frac)
    {
        if (frac >= ctx.StartFrac)
            return true;
        if (ctx.StartFrac <= 0)
            return false;

        line = ShiftEdgeForInsideStart(line, thing, edgeIndex);
        var frac2 = ctx.InterceptVector?.Invoke(ctx.Trace, line) ?? 0;
        return frac2 >= ctx.StartFrac;
    }

    public static void AddThingIntercepts(AddThingInterceptsContext ctx, bool compatible)
    {
        foreach (var thing in ctx.Things)
        {
            if (!compatible)
                AddNonCompatibleThingIntercept(ctx, thing);
            else
                AddCompatibleThingIntercept(ctx, thing);
        }
    }

    static void AddNonCompatibleThingIntercept(AddThingInterceptsContext ctx, ThingBox thing)
    {
        var numfronts = 0;
        for (var i = 0; i < 4; i++)
        {
            if (!TryBuildThingEdgeDivline(i, thing, ctx.Trace, out var line))
                continue;

            numfronts++;
            if (DivlineEndpointsOnSameTraceSide(ctx, line))
                continue;

            var frac = ctx.InterceptVector?.Invoke(ctx.Trace, line) ?? 0;
            if (!TryAcceptBehindStartIntercept(ctx, ref line, i, thing, frac))
                continue;

            PushThingIntercept(ctx, frac, thing);
            return;
        }

        if (numfronts == 0)
            PushThingIntercept(ctx, 0, thing);
    }

    static void AddCompatibleThingIntercept(AddThingInterceptsContext ctx, ThingBox thing)
    {
        var (x1, y1, x2, y2) = CompatibleDiagonalCorners(thing, TraceHasPositiveSlope(ctx.Trace));
        var s1 = ctx.PointOnDivlineSide?.Invoke(x1, y1, ctx.Trace) ?? 0;
        var s2 = ctx.PointOnDivlineSide?.Invoke(x2, y2, ctx.Trace) ?? 0;
        if (s1 == s2)
            return;

        var dl = new Divline(x1, y1, x2 - x1, y2 - y1);
        var frac = ctx.InterceptVector?.Invoke(ctx.Trace, dl) ?? 0;
        if (frac >= ctx.StartFrac)
            PushThingIntercept(ctx, frac, thing);
    }

    static void PushThingIntercept(AddThingInterceptsContext ctx, double frac, ThingBox thing) =>
        ctx.Intercepts.Add(new ThingIntercept
        {
            Frac = frac,
            IsALine = false,
            Done = false,
            Thing = thing,
        });

    static Divline ShiftEdgeForInsideStart(Divline line, ThingBox thing, int edge) =>
        edge switch
        {
            0 => line with { Y = line.Y - 2 * thing.Radius },
            1 => line with { X = line.X - 2 * thing.Radius },
            2 => line with { Y = line.Y + 2 * thing.Radius },
            _ => line with { X = line.X + 2 * thing.Radius },
        };
}
