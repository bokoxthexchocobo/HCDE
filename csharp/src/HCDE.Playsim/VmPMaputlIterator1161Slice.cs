namespace HCDE.Playsim;

/// <summary>
/// Multi-block thing iterator and line intercepts from <c>p_maputl.cpp</c>
/// (<b>1027–1193</b>). Chunk <b>1161</b>.
/// </summary>
public static class VmPMaputlIterator1161Slice
{
    public const int PortalLower = 0x4000;
    public const int PortalUpper = 0x8000;
    public const int PortalFlat = 0xC000;
    public const int NoFloor = 32;
    public const int NoCeiling = 64;
    public const double Epsilon = 1d / 65536d;
    public const int InterceptCapacity = 128;

    public readonly record struct Thing(int Id, int Group);

    public readonly record struct Divline(double X, double Y, double Dx, double Dy);

    public readonly record struct BlockLine(int Id, double X, double Y, double Dx, double Dy);

    public readonly record struct Hit(
        bool Found,
        int ThingId,
        double X,
        double Y,
        double Z,
        int PortalFlags);

    public readonly record struct Intercept(double Frac, bool IsALine, bool Done, int LineId);

    public sealed class State
    {
        public List<int> Checklist { get; } = new();
        public double X;
        public double Y;
        public double Z;
        public int BaseGroup;
        public int Index;
        public int PortalFlags;
        public double BoxTop;
        public double BoxLeft;
        public double BoxRight;
        public double BoxBottom;
        public bool InitClearsHash;
        public int InitCount;
        public bool HashCleared;
        public List<Thing> Queue { get; } = new();
        public int Cursor;
        public Func<int, int, (double X, double Y)>? Offset;
        public Dictionary<int, List<Thing>> Groups { get; } = new();
    }

    public sealed class Traverse
    {
        public Divline Trace;
        public double StartFrac;
        public List<BlockLine> Lines { get; } = new();
        public List<Intercept> Intercepts { get; } = new(InterceptCapacity);
        public int BlockX;
        public int BlockY;
    }

    public static State FromActor(
        bool inited,
        double x,
        double y,
        double z,
        double top,
        double radius,
        double checkRadius,
        int portalGroup,
        IReadOnlyList<int>? existing,
        Func<int, int, (double X, double Y)>? offset,
        Dictionary<int, List<Thing>>? groups,
        Action<State, double, double>? collect)
    {
        var state = new State
        {
            X = x,
            Y = y,
            Z = z,
            BaseGroup = portalGroup,
            Offset = offset,
        };
        CopyGroups(state, groups);
        CopyChecklist(state, existing);
        if (!inited && collect != null)
            collect(state, top, checkRadius);
        if (checkRadius == -1d)
            state.Z = radius;
        else
            state.Z = checkRadius;
        Reset(state);
        return state;
    }

    public static State FromPoint(
        bool inited,
        double x,
        double y,
        double z,
        double height,
        double checkRadius,
        bool hasSector,
        int sectorGroup,
        Func<double, double, int> pointInSector,
        IReadOnlyList<int>? existing,
        Func<int, int, (double X, double Y)>? offset,
        Dictionary<int, List<Thing>>? groups,
        Action<State, double>? collect)
    {
        var state = new State
        {
            X = x,
            Y = y,
            Z = z,
            Offset = offset,
        };
        if (hasSector)
            state.BaseGroup = sectorGroup;
        else
            state.BaseGroup = pointInSector(x, y);
        CopyGroups(state, groups);
        CopyChecklist(state, existing);
        if (!inited && collect != null)
            collect(state, z + height);
        state.Z = checkRadius;
        Reset(state);
        return state;
    }

    public static void Reset(State state)
    {
        state.Index = -1;
        state.PortalFlags = 0;
        StartGroup(state, state.BaseGroup);
        state.HashCleared = true;
    }

    public static void StartGroup(State state, int group)
    {
        var offset = OffsetOf(state, group);
        double centerX = offset.X + state.X;
        double centerY = offset.Y + state.Y;
        state.BoxTop = centerY + state.Z;
        state.BoxLeft = centerX - state.Z;
        state.BoxRight = centerX + state.Z;
        state.BoxBottom = centerY - state.Z;
        state.InitClearsHash = false;
        state.InitCount++;
        state.Queue.Clear();
        state.Cursor = 0;
        if (!state.Groups.TryGetValue(group, out var hits))
            return;
        for (int i = 0; i < hits.Count; i++)
            state.Queue.Add(hits[i]);
    }

    public static Hit Next(State state)
    {
        if (state.Cursor < state.Queue.Count)
        {
            var thing = state.Queue[state.Cursor];
            state.Cursor++;
            var disp = OffsetOf(state, thing.Group);
            return new Hit(
                true,
                thing.Id,
                state.X + disp.X,
                state.Y + disp.Y,
                state.Z,
                state.PortalFlags);
        }

        uint nextIndex = (uint)(state.Index + 1);
        bool onLast = nextIndex >= (uint)state.Checklist.Count;
        int nextFlags = 0;
        if (!onLast)
            nextFlags = state.Checklist[state.Index + 1] & PortalFlat;
        if (onLast)
            return new Hit(false, 0, 0, 0, 0, 0);

        state.Index++;
        int group = state.Checklist[state.Index] & ~PortalFlat;
        StartGroup(state, group);
        if (nextFlags == PortalUpper)
            state.PortalFlags = NoFloor;
        else if (nextFlags == PortalLower)
            state.PortalFlags = NoCeiling;
        else
            state.PortalFlags = 0;
        return Next(state);
    }

    public static Traverse CreateTraverse(Divline trace, double startFrac)
    {
        return new Traverse
        {
            Trace = trace,
            StartFrac = startFrac,
        };
    }

    public static void AddLineIntercepts(Traverse traverse, int blockX, int blockY)
    {
        traverse.BlockX = blockX;
        traverse.BlockY = blockY;
        for (int i = 0; i < traverse.Lines.Count; i++)
        {
            var line = traverse.Lines[i];
            int s1 = PointOnDivlineSide(line.X, line.Y, traverse.Trace);
            int s2 = PointOnDivlineSide(line.X + line.Dx, line.Y + line.Dy, traverse.Trace);
            if (s1 == s2)
                continue;

            var div = new Divline(line.X, line.Y, line.Dx, line.Dy);
            double frac = InterceptAlongTrace(traverse.Trace, div);
            if (frac < traverse.StartFrac)
                continue;
            if (frac > 1d)
                continue;

            traverse.Intercepts.Add(new Intercept(frac, true, false, line.Id));
        }
    }

    public static int PointOnDivlineSide(double x, double y, Divline line)
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

    static (double X, double Y) OffsetOf(State state, int group)
    {
        if (state.BaseGroup == group)
            return (0d, 0d);
        if (state.Offset == null)
            return (0d, 0d);
        return state.Offset(state.BaseGroup, group);
    }

    static void CopyGroups(State state, Dictionary<int, List<Thing>>? groups)
    {
        if (groups == null)
            return;
        foreach (var pair in groups)
            state.Groups[pair.Key] = pair.Value;
    }

    static void CopyChecklist(State state, IReadOnlyList<int>? existing)
    {
        if (existing == null)
            return;
        for (int i = 0; i < existing.Count; i++)
            state.Checklist.Add(existing[i]);
    }
}
