namespace HCDE.Playsim;

/// <summary><c>FBlockLinesIterator</c> constructors, <c>StartBlock</c>, and <c>Next</c> from <c>p_maputl.cpp</c> (563–683).</summary>
public static class VmPMapUtlBlockLinesIterator
{
    public sealed class IteratorState
    {
        public int ValidCount { get; set; }
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }
        public int CurX { get; set; }
        public int CurY { get; set; }
        public IReadOnlyList<int>? CurrentBlockLines { get; set; }
        public int ListIndex { get; set; }
        public HashSet<int> ReturnedLineIds { get; } = new();
    }

    public static IteratorState Create(int minX, int minY, int maxX, int maxY, bool keepValidCount, int validCount)
    {
        if (!keepValidCount)
            validCount++;
        var state = new IteratorState
        {
            ValidCount = validCount,
            MinX = minX,
            MaxX = maxX,
            MinY = minY,
            MaxY = maxY,
            CurX = minX - 1,
            CurY = minY,
        };
        return state;
    }

    public static void InitFromBox(IteratorState state, double left, double bottom, double right, double top, Func<double, int> blockX, Func<double, int> blockY)
    {
        state.ValidCount++;
        state.MaxY = blockY(top);
        state.MinY = blockY(bottom);
        state.MaxX = blockX(right);
        state.MinX = blockX(left);
        state.CurX = state.MinX - 1;
        state.CurY = state.MinY;
    }

    public static void StartBlock(IteratorState state, int x, int y, Func<int, int, bool> isValidBlock, Func<int, int, IReadOnlyList<int>> getLines)
    {
        state.CurX = x;
        state.CurY = y;
        state.ListIndex = 0;
        if (isValidBlock(x, y))
            state.CurrentBlockLines = getLines(x, y);
        else
            state.CurrentBlockLines = null;
    }

    public static int? Next(IteratorState state, Func<int, int, bool> isValidBlock, Func<int, int, IReadOnlyList<int>> getLines)
    {
        while (true)
        {
            if (state.CurrentBlockLines is not null)
            {
                while (state.ListIndex < state.CurrentBlockLines.Count)
                {
                    var lineId = state.CurrentBlockLines[state.ListIndex++];
                    if (lineId < 0)
                        break;
                    if (state.ReturnedLineIds.Add(lineId))
                        return lineId;
                }
            }

            if (++state.CurX > state.MaxX)
            {
                state.CurX = state.MinX;
                if (++state.CurY > state.MaxY)
                    return null;
            }

            StartBlock(state, state.CurX, state.CurY, isValidBlock, getLines);
        }
    }
}
