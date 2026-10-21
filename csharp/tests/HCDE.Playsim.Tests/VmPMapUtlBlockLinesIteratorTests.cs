namespace HCDE.Playsim.Tests;

public class VmPMapUtlBlockLinesIteratorTests
{
    static Dictionary<int, VmPMapUtlBlockLinesIterator.MapLine> Lines(params VmPMapUtlBlockLinesIterator.MapLine[] lines) =>
        lines.ToDictionary(l => l.Id);

    [Fact]
    public void Next_WalksBlocksInOrder()
    {
        var state = VmPMapUtlBlockLinesIterator.Create(1, 1, 2, 1, keepValidCount: true, validCount: 1);
        var blockLines = new Dictionary<(int x, int y), IReadOnlyList<int>>
        {
            [(1, 1)] = new List<int> { 10, 11, -1 },
            [(2, 1)] = new List<int> { 20, -1 },
        };
        IReadOnlyList<int> GetLines(int x, int y) => blockLines.GetValueOrDefault((x, y)) ?? new List<int> { -1 };
        var map = Lines(
            new() { Id = 10 },
            new() { Id = 11 },
            new() { Id = 20 });
        var first = VmPMapUtlBlockLinesIterator.Next(state, (_, _) => true, GetLines, (_, _) => null, id => map[id]);
        var second = VmPMapUtlBlockLinesIterator.Next(state, (_, _) => true, GetLines, (_, _) => null, id => map[id]);
        Assert.Equal(10, first);
        Assert.Equal(11, second);
    }

    [Fact]
    public void Next_YieldsPolyLinedefsAndSkipsValidCountHits()
    {
        var state = VmPMapUtlBlockLinesIterator.Create(1, 1, 1, 1, keepValidCount: true, validCount: 5);
        var poly = new VmPMapUtlBlockLinesIterator.PolyObjLines { LinedefIds = new[] { 100, 101 } };
        var polyLink = new VmPMapUtlBlockLinesIterator.PolyBlockLink { Poly = poly };
        var map = Lines(
            new() { Id = 100, ValidCount = 4 },
            new() { Id = 101 },
            new() { Id = 200, ValidCount = 5 });

        IReadOnlyList<int> GetLines(int x, int y) => new List<int> { 200, -1 };
        VmPMapUtlBlockLinesIterator.PolyBlockLink? GetPoly(int x, int y) => polyLink;

        var first = VmPMapUtlBlockLinesIterator.Next(state, (_, _) => true, GetLines, GetPoly, id => map[id]);
        Assert.Equal(100, first);
        Assert.Equal(5, poly.ValidCount);
        Assert.Equal(5, map[100].ValidCount);

        var second = VmPMapUtlBlockLinesIterator.Next(state, (_, _) => true, GetLines, GetPoly, id => map[id]);
        Assert.Equal(101, second);

        var third = VmPMapUtlBlockLinesIterator.Next(state, (_, _) => true, GetLines, GetPoly, id => map[id]);
        Assert.Null(third);
    }

    [Fact]
    public void Next_SkipsPolyWhenValidCountAlreadyMatches()
    {
        var state = VmPMapUtlBlockLinesIterator.Create(1, 1, 1, 1, keepValidCount: true, validCount: 3);
        var poly = new VmPMapUtlBlockLinesIterator.PolyObjLines { ValidCount = 3, LinedefIds = new[] { 50 } };
        var polyLink = new VmPMapUtlBlockLinesIterator.PolyBlockLink { Poly = poly };
        var map = Lines(new() { Id = 60 });

        IReadOnlyList<int> GetLines(int x, int y) => new List<int> { 60, -1 };

        var lineId = VmPMapUtlBlockLinesIterator.Next(
            state,
            (_, _) => true,
            GetLines,
            (_, _) => polyLink,
            id => map[id]);
        Assert.Equal(60, lineId);
    }
}
