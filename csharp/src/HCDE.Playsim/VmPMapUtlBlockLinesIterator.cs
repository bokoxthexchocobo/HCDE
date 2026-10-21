namespace HCDE.Playsim;

/// <summary><c>FBlockLinesIterator</c> constructors, <c>StartBlock</c>, and <c>Next</c> from <c>p_maputl.cpp</c> (563–683).</summary>
public static class VmPMapUtlBlockLinesIterator
{
    public sealed class MapLine
    {
        public int Id { get; set; }
        public int ValidCount { get; set; }
    }

    public sealed class PolyObjLines
    {
        public int ValidCount { get; set; }
        public IReadOnlyList<int> LinedefIds { get; init; } = Array.Empty<int>();
    }

    public sealed class PolyBlockLink
    {
        public PolyObjLines? Poly { get; set; }
        public PolyBlockLink? Next { get; set; }
    }

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
        public PolyBlockLink? PolyLink { get; set; }
        public int PolyLineIndex { get; set; }
    }

    public static IteratorState Create(int minX, int minY, int maxX, int maxY, bool keepValidCount, int validCount)
    {
        if (!keepValidCount)
            validCount++;
        return new IteratorState
        {
            ValidCount = validCount,
            MinX = minX,
            MaxX = maxX,
            MinY = minY,
            MaxY = maxY,
            CurX = minX - 1,
            CurY = minY,
        };
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

    public static void StartBlock(
        IteratorState state,
        int x,
        int y,
        Func<int, int, bool> isValidBlock,
        Func<int, int, IReadOnlyList<int>> getLines,
        Func<int, int, PolyBlockLink?> getPolyLink)
    {
        state.CurX = x;
        state.CurY = y;
        state.ListIndex = 0;
        state.PolyLineIndex = 0;
        if (isValidBlock(x, y))
        {
            state.CurrentBlockLines = getLines(x, y);
            state.PolyLink = getPolyLink(x, y);
        }
        else
        {
            state.CurrentBlockLines = null;
            state.PolyLink = null;
        }
    }

    public static int? Next(
        IteratorState state,
        Func<int, int, bool> isValidBlock,
        Func<int, int, IReadOnlyList<int>> getLines,
        Func<int, int, PolyBlockLink?> getPolyLink,
        Func<int, MapLine?> resolveLine)
    {
        while (true)
        {
            while (state.PolyLink is not null)
            {
                var link = state.PolyLink;
                if (link.Poly is not null)
                {
                    if (state.PolyLineIndex == 0)
                    {
                        if (link.Poly.ValidCount == state.ValidCount)
                        {
                            state.PolyLink = link.Next;
                            continue;
                        }

                        link.Poly.ValidCount = state.ValidCount;
                    }

                    var ldId = link.Poly.LinedefIds[state.PolyLineIndex];
                    state.PolyLineIndex++;
                    if (state.PolyLineIndex >= link.Poly.LinedefIds.Count)
                    {
                        state.PolyLink = link.Next;
                        state.PolyLineIndex = 0;
                    }

                    var ld = resolveLine(ldId);
                    if (ld is null)
                        continue;
                    if (ld.ValidCount == state.ValidCount)
                        continue;

                    ld.ValidCount = state.ValidCount;
                    return ld.Id;
                }

                state.PolyLink = link.Next;
            }

            if (state.CurrentBlockLines is not null)
            {
                while (state.ListIndex < state.CurrentBlockLines.Count)
                {
                    var lineIndex = state.CurrentBlockLines[state.ListIndex++];
                    if (lineIndex < 0)
                        break;

                    var ld = resolveLine(lineIndex);
                    if (ld is null)
                        continue;
                    if (ld.ValidCount == state.ValidCount)
                        continue;

                    ld.ValidCount = state.ValidCount;
                    return ld.Id;
                }
            }

            if (++state.CurX > state.MaxX)
            {
                state.CurX = state.MinX;
                if (++state.CurY > state.MaxY)
                    return null;
            }

            StartBlock(state, state.CurX, state.CurY, isValidBlock, getLines, getPolyLink);
        }
    }
}
