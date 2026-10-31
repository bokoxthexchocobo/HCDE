namespace HCDE.Playsim;

/// <summary>
/// Sight blockers, block iteration, and intercept ordering from <c>p_sight.cpp</c>
/// (<b>385–618</b>). Chunk <b>1163</b>.
/// </summary>
public static class VmPSightBlock1163Slice
{
    public const int TwoSided = 0x00000004;
    public const int BlockEverything = 0x00008000;
    public const int BlockSight = 0x04000000;
    public const int SeePastShootable = 2;
    public const int SeePastBlockEverything = 4;
    public const int IgnoreWater = 8;
    public const int Impact = 1 << 3;
    public const int AcsExecute = 80;
    public const int AcsExecuteAlways = 226;
    public const int FloorExists = 0x1;
    public const int FloorSolid = 0x2;
    public const int SeeThrough = 0x2000000;
    public const double Epsilon = 1d / 65536d;
    public const double FarFrac = 2147483647d;

    public readonly record struct Divline(double X, double Y, double Dx, double Dy);

    public sealed class Line
    {
        public int Id;
        public int Flags;
        public int Special;
        public int Activation;
        public int Arg1;
        public bool HasBack;
        public double X;
        public double Y;
        public double Dx;
        public double Dy;
        public int ValidCount;
    }

    public sealed class PolyObject
    {
        public int ValidCount;
        public List<Line> Lines { get; } = new();
    }

    public sealed class PolyLink
    {
        public PolyObject? Poly;
        public PolyLink? Next;
    }

    public sealed class Floor
    {
        public int Flags;
        public double Bottom;
        public double Top;
    }

    public sealed class Intercept
    {
        public double Frac;
        public bool IsALine;
        public Line? Line;
    }

    public sealed class Check
    {
        public Divline Trace;
        public double StartFrac;
        public int ValidCount;
        public bool PortalFound;
        public int Flags;
        public int LevelNum;
        public int SeeThroughMask = SeeThrough;
        public double LastZTop;
        public double LastZBottom;
        public double TopSlope;
        public double BottomSlope;
        public double EyeZ;
        public int LastSector = -1;
        public int TargetSector = -2;
        public List<Floor> Floors { get; } = new();
        public List<Intercept> Intercepts { get; } = new();
        public int[] Counts { get; } = new int[6];
        public Func<Intercept, bool>? Visit;
        public int LastOffset;
    }

    public static bool Blocks(Check check, Line line)
    {
        if (!line.HasBack || (line.Flags & TwoSided) == 0 || (line.Flags & BlockSight) != 0)
            return true;

        if ((line.Flags & BlockEverything) == 0)
            return false;

        if ((check.Flags & SeePastBlockEverything) == 0)
            return true;

        if ((check.Flags & SeePastShootable) != 0)
            return false;

        if ((line.Activation & Impact) == 0)
            return true;
        if (line.Special != AcsExecute && line.Special != AcsExecuteAlways)
            return true;
        if (line.Arg1 != 0 && line.Arg1 != check.LevelNum)
            return true;
        return false;
    }

    public static bool CheckLine(Check check, Line line)
    {
        if (line.ValidCount == check.ValidCount)
            return true;

        line.ValidCount = check.ValidCount;
        int s1 = Side(line.X, line.Y, check.Trace);
        int s2 = Side(line.X + line.Dx, line.Y + line.Dy, check.Trace);
        if (s1 == s2)
            return true;

        var div = new Divline(line.X, line.Y, line.Dx, line.Dy);
        int t1 = Side(check.Trace.X, check.Trace.Y, div);
        int t2 = Side(check.Trace.X + check.Trace.Dx, check.Trace.Y + check.Trace.Dy, div);
        if (t1 == t2)
            return true;

        if (!check.PortalFound && Blocks(check, line))
            return false;

        check.Counts[3]++;
        check.Intercepts.Add(new Intercept
        {
            IsALine = true,
            Line = line,
        });
        return true;
    }

    public static int BlockLines(
        Check check,
        int x,
        int y,
        int width,
        bool linkedPortals,
        bool linkedPolyPortals,
        IReadOnlyList<PolyLink?> polyMap,
        IReadOnlyList<IReadOnlyList<Line>> lineMap)
    {
        int offset = y * width + x;
        check.LastOffset = offset;
        if (linkedPortals)
            check.PortalFound = true;

        int result = 1;
        PolyLink? poly = polyMap[offset];
        if (poly != null && linkedPolyPortals)
            check.PortalFound = true;

        while (poly != null)
        {
            var obj = poly.Poly;
            if (obj != null && obj.ValidCount != check.ValidCount)
            {
                obj.ValidCount = check.ValidCount;
                for (int i = 0; i < obj.Lines.Count; i++)
                {
                    if (!CheckLine(check, obj.Lines[i]))
                    {
                        if (!check.PortalFound)
                            return 0;
                        result = -1;
                    }
                }
            }

            poly = poly.Next;
        }

        var lines = lineMap[offset];
        for (int i = 0; i < lines.Count; i++)
        {
            if (!CheckLine(check, lines[i]))
            {
                if (!check.PortalFound)
                    return 0;
                result = -1;
            }
        }

        return result;
    }

    public static bool Traverse(Check check)
    {
        int count = check.Intercepts.Count;
        for (int i = 0; i < check.Intercepts.Count; i++)
        {
            var scan = check.Intercepts[i];
            var line = scan.Line;
            var div = line == null
                ? new Divline(0, 0, 0, 0)
                : new Divline(line.X, line.Y, line.Dx, line.Dy);
            scan.Frac = InterceptAlongTrace(check.Trace, div);
            if (scan.Frac < check.StartFrac)
            {
                scan.Frac = FarFrac;
                count--;
            }
        }

        Intercept? chosen = null;
        while (count > 0)
        {
            count--;
            double dist = FarFrac;
            for (int i = 0; i < check.Intercepts.Count; i++)
            {
                var scan = check.Intercepts[i];
                if (scan.Frac < dist)
                {
                    dist = scan.Frac;
                    chosen = scan;
                }
            }

            if (chosen != null)
            {
                bool keep = check.Visit == null || check.Visit(chosen);
                if (!keep)
                    return false;
                chosen.Frac = FarFrac;
            }
        }

        if (check.LastSector != check.TargetSector || check.Floors.Count == 0)
            return true;

        double topZ = check.TopSlope + check.EyeZ;
        double bottomZ = check.BottomSlope + check.EyeZ;
        for (int i = 0; i < check.Floors.Count; i++)
        {
            var rover = check.Floors[i];
            if ((rover.Flags & SeeThrough) == check.SeeThroughMask || (rover.Flags & FloorExists) == 0)
                continue;
            if ((check.Flags & IgnoreWater) != 0 && (rover.Flags & FloorSolid) == 0)
                continue;

            if (check.LastZTop <= rover.Bottom && topZ > rover.Bottom
                && check.LastZBottom <= rover.Bottom && bottomZ > rover.Bottom)
                return false;
            if (check.LastZBottom >= rover.Top && bottomZ < rover.Top
                && check.LastZTop >= rover.Top && topZ < rover.Top)
                return false;
        }

        return true;
    }

    public static int Side(double x, double y, Divline line)
    {
        double cross = (y - line.Y) * line.Dx + (line.X - x) * line.Dy;
        if (cross > Epsilon)
            return 1;
        return 0;
    }

    public static double InterceptAlongTrace(Divline trace, Divline line)
    {
        double den = line.Dy * trace.Dx - line.Dx * trace.Dy;
        if (den == 0d)
            return 0d;
        double num = (line.X - trace.X) * line.Dy + (trace.Y - line.Y) * line.Dx;
        return num / den;
    }
}
