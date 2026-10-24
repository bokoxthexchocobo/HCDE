namespace HCDE.Playsim;

/// <summary><c>FPathTraverse::AddThingIntercepts</c> from <c>p_maputl.cpp</c> (1194–1361).</summary>
public static class VmPMapUtlAddThingIntercepts
{
    public readonly record struct Divline(double X, double Y, double Dx, double Dy);

    public sealed class ThingIntercept
    {
        public double Frac { get; set; }
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

    public static void AddThingIntercepts(AddThingInterceptsContext ctx, bool compatible)
    {
        foreach (var thing in ctx.Things)
        {
            if (!compatible)
            {
                var numfronts = 0;
                for (var i = 0; i < 4; i++)
                {
                    var line = EdgeDivline(thing, i, ctx.Trace);
                    if (line is null) continue;
                    numfronts++;
                    var sideA = ctx.PointOnDivlineSide?.Invoke(line.Value.X, line.Value.Y, ctx.Trace) ?? 0;
                    var sideB = ctx.PointOnDivlineSide?.Invoke(line.Value.X + line.Value.Dx, line.Value.Y + line.Value.Dy, ctx.Trace) ?? 0;
                    if (sideA == sideB) continue;

                    var frac = ctx.InterceptVector?.Invoke(ctx.Trace, line.Value) ?? 0;
                    if (frac < ctx.StartFrac)
                    {
                        if (ctx.StartFrac > 0 && TryInsideActorStart(ctx, thing, i, line.Value))
                            PushIntercept(ctx, 0, thing);
                        continue;
                    }

                    PushIntercept(ctx, frac, thing);
                    break;
                }

                if (numfronts == 0)
                    PushIntercept(ctx, 0, thing);
            }
            else
            {
                var tracePositive = ctx.Trace.Dx * ctx.Trace.Dy > 0;
                double x1, y1, x2, y2;
                if (tracePositive)
                {
                    x1 = thing.X - thing.Radius;
                    y1 = thing.Y + thing.Radius;
                    x2 = thing.X + thing.Radius;
                    y2 = thing.Y - thing.Radius;
                }
                else
                {
                    x1 = thing.X - thing.Radius;
                    y1 = thing.Y - thing.Radius;
                    x2 = thing.X + thing.Radius;
                    y2 = thing.Y + thing.Radius;
                }

                var s1 = ctx.PointOnDivlineSide?.Invoke(x1, y1, ctx.Trace) ?? 0;
                var s2 = ctx.PointOnDivlineSide?.Invoke(x2, y2, ctx.Trace) ?? 0;
                if (s1 == s2) continue;

                var dl = new Divline(x1, y1, x2 - x1, y2 - y1);
                var frac = ctx.InterceptVector?.Invoke(ctx.Trace, dl) ?? 0;
                if (frac >= ctx.StartFrac)
                    PushIntercept(ctx, frac, thing);
            }
        }
    }

    static void PushIntercept(AddThingInterceptsContext ctx, double frac, ThingBox thing) =>
        ctx.Intercepts.Add(new ThingIntercept { Frac = frac, Thing = thing });

    static bool TryInsideActorStart(AddThingInterceptsContext ctx, ThingBox thing, int edge, Divline line)
    {
        var shifted = ShiftEdge(line, thing, edge);
        var frac2 = ctx.InterceptVector?.Invoke(ctx.Trace, shifted) ?? 0;
        return frac2 >= ctx.StartFrac;
    }

    static Divline ShiftEdge(Divline line, ThingBox thing, int edge)
    {
        return edge switch
        {
            0 => line with { Y = line.Y - 2 * thing.Radius },
            1 => line with { X = line.X - 2 * thing.Radius },
            2 => line with { Y = line.Y + 2 * thing.Radius },
            _ => line with { X = line.X + 2 * thing.Radius },
        };
    }

    static Divline? EdgeDivline(ThingBox thing, int i, Divline trace)
    {
        return i switch
        {
            0 when trace.Y >= thing.Y + thing.Radius => new Divline(thing.X + thing.Radius, thing.Y + thing.Radius, -thing.Radius * 2, 0),
            1 when trace.X >= thing.X + thing.Radius => new Divline(thing.X + thing.Radius, thing.Y - thing.Radius, 0, thing.Radius * 2),
            2 when trace.Y <= thing.Y - thing.Radius => new Divline(thing.X - thing.Radius, thing.Y - thing.Radius, thing.Radius * 2, 0),
            3 when trace.X <= thing.X - thing.Radius => new Divline(thing.X - thing.Radius, thing.Y + thing.Radius, 0, -thing.Radius * 2),
            _ => null,
        };
    }
}
