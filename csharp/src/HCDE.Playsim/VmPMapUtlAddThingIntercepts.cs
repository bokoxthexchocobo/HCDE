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

    public static void AddThingIntercepts(AddThingInterceptsContext ctx, bool compatible)
    {
        foreach (var thing in ctx.Things)
        {
            if (!compatible)
            {
                // [RH] bounding-box edges instead of corner-to-corner when not compatibility mode.
                var numfronts = 0;
                for (var i = 0; i < 4; i++)
                {
                    Divline line;
                    switch (i)
                    {
                        case 0:
                            var topY = thing.Y + thing.Radius;
                            if (ctx.Trace.Y < topY)
                                continue;
                            line = new Divline(thing.X + thing.Radius, topY, -thing.Radius * 2, 0);
                            break;
                        case 1:
                            var rightX = thing.X + thing.Radius;
                            if (ctx.Trace.X < rightX)
                                continue;
                            line = new Divline(rightX, thing.Y - thing.Radius, 0, thing.Radius * 2);
                            break;
                        case 2:
                            var bottomY = thing.Y - thing.Radius;
                            if (ctx.Trace.Y > bottomY)
                                continue;
                            line = new Divline(thing.X - thing.Radius, bottomY, thing.Radius * 2, 0);
                            break;
                        default:
                            var leftX = thing.X - thing.Radius;
                            if (ctx.Trace.X > leftX)
                                continue;
                            line = new Divline(leftX, thing.Y + thing.Radius, 0, -thing.Radius * 2);
                            break;
                    }

                    numfronts++;
                    var sideA = ctx.PointOnDivlineSide?.Invoke(line.X, line.Y, ctx.Trace) ?? 0;
                    var sideB = ctx.PointOnDivlineSide?.Invoke(line.X + line.Dx, line.Y + line.Dy, ctx.Trace) ?? 0;
                    if (sideA == sideB)
                        continue;

                    var frac = ctx.InterceptVector?.Invoke(ctx.Trace, line) ?? 0;
                    if (frac < ctx.StartFrac)
                    {
                        if (ctx.StartFrac > 0)
                        {
                            line = ShiftEdgeForInsideStart(line, thing, i);
                            var frac2 = ctx.InterceptVector?.Invoke(ctx.Trace, line) ?? 0;
                            if (frac2 >= ctx.StartFrac)
                                goto addIntercept;
                        }

                        continue;
                    }

                addIntercept:
                    PushThingIntercept(ctx, frac, thing);
                    break;
                }

                if (numfronts == 0)
                    PushThingIntercept(ctx, 0, thing);
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
                if (s1 == s2)
                    continue;

                var dl = new Divline(x1, y1, x2 - x1, y2 - y1);
                var frac = ctx.InterceptVector?.Invoke(ctx.Trace, dl) ?? 0;
                if (frac >= ctx.StartFrac)
                    PushThingIntercept(ctx, frac, thing);
            }
        }
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
