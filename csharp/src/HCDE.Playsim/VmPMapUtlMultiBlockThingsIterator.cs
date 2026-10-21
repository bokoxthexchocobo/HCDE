namespace HCDE.Playsim;

/// <summary><c>FMultiBlockThingsIterator</c> and <c>P_RoughMonsterSearch</c> from <c>p_maputl.cpp</c> (1038–1131, 1821–1835).</summary>
public static class VmPMapUtlMultiBlockThingsIterator
{
    public const int FlatUpper = 1;
    public const int FlatLower = 2;
    public const int FfcfNoFloor = 1;
    public const int FfcfNoCeiling = 2;

    public sealed class CheckResult
    {
        public object? Thing { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public int PortalFlags { get; set; }
    }

    public sealed class IteratorState
    {
        public IReadOnlyList<int> Checklist { get; set; } = Array.Empty<int>();
        public double CheckpointX { get; set; }
        public double CheckpointY { get; set; }
        public double CheckpointZ { get; set; }
        public int BaseGroup { get; set; }
        public int Index { get; set; } = -1;
        public int PortalFlags { get; set; }
    }

    public sealed class MultiBlockThingsContext
    {
        public IteratorState State { get; } = new();
        public VmPMapUtlBlockThingsIterator.IteratorState BlockIterator { get; } = new();
        public Func<object?>? NextThing { get; init; }
        public Func<object?, int>? ThingPortalGroup { get; init; }
        public Func<int, int, (double X, double Y)>? DisplacementOffset { get; init; }
        public Action<double, double, double, bool>? InitBlockIteratorBox { get; init; }
        public Action? ClearBlockHash { get; init; }
    }

    public static void Reset(MultiBlockThingsContext ctx)
    {
        ctx.State.Index = -1;
        ctx.State.PortalFlags = 0;
        StartIteratorForGroup(ctx, ctx.State.BaseGroup);
        ctx.ClearBlockHash?.Invoke();
    }

    public static void StartIteratorForGroup(MultiBlockThingsContext ctx, int group)
    {
        var offset = ctx.DisplacementOffset?.Invoke(ctx.State.BaseGroup, group) ?? (0, 0);
        var x = offset.X + ctx.State.CheckpointX;
        var y = offset.Y + ctx.State.CheckpointY;
        ctx.InitBlockIteratorBox?.Invoke(x, y, ctx.State.CheckpointZ, false);
    }

    public static bool Next(MultiBlockThingsContext ctx, CheckResult item)
    {
        var thing = ctx.NextThing?.Invoke();
        if (thing is not null)
        {
            item.Thing = thing;
            var group = ctx.ThingPortalGroup?.Invoke(thing) ?? 0;
            var disp = ctx.DisplacementOffset?.Invoke(ctx.State.BaseGroup, group) ?? (0, 0);
            item.PosX = ctx.State.CheckpointX + disp.X;
            item.PosY = ctx.State.CheckpointY + disp.Y;
            item.PosZ = ctx.State.CheckpointZ;
            item.PortalFlags = ctx.State.PortalFlags;
            return true;
        }

        var st = ctx.State;
        var onLast = st.Index + 1 >= st.Checklist.Count;
        if (onLast)
            return false;

        var nextFlags = st.Checklist[st.Index + 1] & 3;
        st.Index++;
        StartIteratorForGroup(ctx, st.Checklist[st.Index] & ~3);
        st.PortalFlags = nextFlags switch
        {
            FlatUpper => FfcfNoFloor,
            FlatLower => FfcfNoCeiling,
            _ => 0,
        };
        return Next(ctx, item);
    }

    public sealed class RoughMonsterSearchContext
    {
        public bool OnlySeekable { get; set; }
        public bool FrontOnly { get; set; }
        public double Fov { get; set; }
        public double OriginX { get; set; }
        public double OriginY { get; set; }
        public double YawSin { get; set; }
        public double YawCos { get; set; }
        public Func<int, object?>? BlockmapSearch { get; init; }
    }

    public static object? RoughMonsterSearch(RoughMonsterSearchContext ctx, int distance)
    {
        var info = new VmPMapUtlPathPortalSearch.RoughBlockCheckContext
        {
            OnlySeekable = ctx.OnlySeekable,
            FrontOnly = ctx.FrontOnly,
            Fov = ctx.Fov,
            FrontLineX = ctx.OriginX,
            FrontLineY = ctx.OriginY,
            FrontLineDx = -ctx.YawSin,
            FrontLineDy = -ctx.YawCos,
        };
        return ctx.BlockmapSearch?.Invoke(distance);
    }
}
