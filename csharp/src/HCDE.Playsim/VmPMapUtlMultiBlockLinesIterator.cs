namespace HCDE.Playsim;

/// <summary><c>FMultiBlockLinesIterator</c> from <c>p_maputl.cpp</c> (693–856).</summary>
public static class VmPMapUtlMultiBlockLinesIterator
{
    public const int FlatUpper = 1;
    public const int FlatLower = 2;
    public const int FfcfNoFloor = 1;
    public const int FfcfNoCeiling = 2;

    public sealed class CheckResult
    {
        public object? Line { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public int PortalFlags { get; set; }
    }

    public sealed class IteratorState
    {
        public IReadOnlyList<int> Checklist { get; set; } = Array.Empty<int>();
        public bool ChecklistInited { get; set; }
        public double CheckpointX { get; set; }
        public double CheckpointY { get; set; }
        public double CheckpointZ { get; set; }
        public int BaseGroup { get; set; }
        public object? StartSector { get; set; }
        public object? CurSector { get; set; }
        public int Index { get; set; } = -1;
        public int PortalFlags { get; set; }
        public bool ContinueUp { get; set; } = true;
        public bool ContinueDown { get; set; } = true;
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
    }

    public sealed class MultiBlockLinesContext
    {
        public IteratorState State { get; } = new();
        public Func<object?>? NextBlockLine { get; init; }
        public Func<object?, int, bool>? PortalBlocksMovement { get; init; }
        public Func<object?, int, int>? OppositePortalGroup { get; init; }
        public Func<int, int, (double X, double Y)>? DisplacementOffset { get; init; }
        public Func<double, double, object?>? PointInSector { get; init; }
        public Func<object?, int>? SectorPortalGroup { get; init; }
        public Action<double, double, double>? InitBlockIteratorBox { get; init; }
    }

    public static void Reset(MultiBlockLinesContext ctx)
    {
        var st = ctx.State;
        st.ContinueUp = st.ContinueDown = true;
        st.Index = -1;
        st.PortalFlags = 0;
        StartIteratorForGroup(ctx, st.BaseGroup);
    }

    public static void StartIteratorForGroup(MultiBlockLinesContext ctx, int group)
    {
        var st = ctx.State;
        var offset = ctx.DisplacementOffset?.Invoke(st.BaseGroup, group) ?? (0, 0);
        st.OffsetX = offset.X + st.CheckpointX;
        st.OffsetY = offset.Y + st.CheckpointY;
        st.CurSector = group == ctx.SectorPortalGroup?.Invoke(st.StartSector)
            ? st.StartSector
            : ctx.PointInSector?.Invoke(st.OffsetX, st.OffsetY);
        if (ctx.SectorPortalGroup?.Invoke(st.CurSector) != group)
            return;
        ctx.InitBlockIteratorBox?.Invoke(st.OffsetX, st.OffsetY, st.CheckpointZ);
    }

    public static bool GoUp(MultiBlockLinesContext ctx, double x, double y)
    {
        var st = ctx.State;
        if (!st.ContinueUp)
            return false;
        if (ctx.PortalBlocksMovement?.Invoke(st.CurSector, 1) == true)
        {
            st.ContinueUp = false;
            return false;
        }

        var group = ctx.OppositePortalGroup?.Invoke(st.CurSector, 1) ?? 0;
        if (group == 0)
        {
            st.ContinueUp = false;
            return false;
        }

        if (!StartIteratorForGroupReturning(ctx, group))
        {
            st.ContinueUp = false;
            return false;
        }

        st.PortalFlags = FfcfNoFloor;
        return true;
    }

    public static bool GoDown(MultiBlockLinesContext ctx, double x, double y)
    {
        var st = ctx.State;
        if (!st.ContinueDown)
            return false;
        if (ctx.PortalBlocksMovement?.Invoke(st.CurSector, 0) == true)
        {
            st.ContinueDown = false;
            return false;
        }

        var group = ctx.OppositePortalGroup?.Invoke(st.CurSector, 0) ?? 0;
        if (group == 0)
        {
            st.ContinueDown = false;
            return false;
        }

        if (!StartIteratorForGroupReturning(ctx, group))
        {
            st.ContinueDown = false;
            return false;
        }

        st.PortalFlags = FfcfNoCeiling;
        return true;
    }

    static bool StartIteratorForGroupReturning(MultiBlockLinesContext ctx, int group)
    {
        StartIteratorForGroup(ctx, group);
        return ctx.SectorPortalGroup?.Invoke(ctx.State.CurSector) == group;
    }

    public static bool Next(MultiBlockLinesContext ctx, CheckResult item)
    {
        var line = ctx.NextBlockLine?.Invoke();
        if (line is not null)
        {
            item.Line = line;
            item.PosX = ctx.State.OffsetX;
            item.PosY = ctx.State.OffsetY;
            item.PortalFlags = ctx.State.PortalFlags;
            return true;
        }

        var st = ctx.State;
        var onLast = st.Index + 1 >= st.Checklist.Count;
        var nextFlags = onLast ? 0 : st.Checklist[st.Index + 1] & 3;

        if (st.PortalFlags == FfcfNoFloor && nextFlags != FlatUpper && GoUp(ctx, st.OffsetX, st.OffsetY))
            return Next(ctx, item);
        if (st.PortalFlags == FfcfNoCeiling && nextFlags != FlatLower && GoDown(ctx, st.OffsetX, st.OffsetY))
            return Next(ctx, item);

        if (onLast)
        {
            st.CurSector = st.StartSector;
            if (GoUp(ctx, st.CheckpointX, st.CheckpointY) || GoDown(ctx, st.CheckpointX, st.CheckpointY))
                return Next(ctx, item);
            return false;
        }

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
}
