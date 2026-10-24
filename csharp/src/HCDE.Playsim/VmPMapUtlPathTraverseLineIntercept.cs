namespace HCDE.Playsim;

/// <summary><c>FPathTraverse::AddLineIntercepts</c> and <c>FPathTraverse::Next</c> from <c>p_maputl.cpp</c> (1154–1185, 1370–1388).</summary>
public static class VmPMapUtlPathTraverseLineIntercept
{
    public readonly record struct Divline(double X, double Y, double Dx, double Dy);

    public sealed class LineIntercept
    {
        public double Frac { get; set; }
        public int LineId { get; set; }
        public bool Done { get; set; }
    }

    public sealed class AddLineInterceptsContext
    {
        public Divline Trace { get; set; }
        public double StartFrac { get; set; }
        public List<LineIntercept> Intercepts { get; } = new();
        public IReadOnlyList<LineSegment> Lines { get; set; } = Array.Empty<LineSegment>();

        public Func<double, double, Divline, int>? PointOnDivlineSide { get; init; }
        public Func<Divline, Divline, double>? InterceptVector { get; init; }
        public Func<LineSegment, Divline>? MakeDivline { get; init; }
    }

    public readonly record struct LineSegment(int Id, double V1X, double V1Y, double V2X, double V2Y);

    public static void AddLineIntercepts(AddLineInterceptsContext ctx)
    {
        foreach (var ld in ctx.Lines)
        {
            var s1 = ctx.PointOnDivlineSide?.Invoke(ld.V1X, ld.V1Y, ctx.Trace) ?? 0;
            var s2 = ctx.PointOnDivlineSide?.Invoke(ld.V2X, ld.V2Y, ctx.Trace) ?? 0;
            if (s1 == s2)
                continue;

            var dl = ctx.MakeDivline?.Invoke(ld) ?? new Divline(ld.V1X, ld.V1Y, ld.V2X - ld.V1X, ld.V2Y - ld.V1Y);
            var frac = ctx.InterceptVector?.Invoke(ctx.Trace, dl) ?? 0;
            if (frac < ctx.StartFrac || frac > 1)
                continue;

            ctx.Intercepts.Add(new LineIntercept { Frac = frac, LineId = ld.Id });
        }
    }

    public static LineIntercept? Next(IReadOnlyList<LineIntercept> intercepts, ref int scanIndex)
    {
        LineIntercept? best = null;
        var dist = double.MaxValue;
        for (var scanpos = scanIndex; scanpos < intercepts.Count; scanpos++)
        {
            var scan = intercepts[scanpos];
            if (scan.Frac < dist && !scan.Done)
            {
                dist = scan.Frac;
                best = scan;
            }
        }

        if (dist > 1 || best is null)
            return null;

        best.Done = true;
        return best;
    }
}
