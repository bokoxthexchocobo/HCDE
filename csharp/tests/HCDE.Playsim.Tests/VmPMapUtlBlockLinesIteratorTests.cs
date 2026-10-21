namespace HCDE.Playsim.Tests;

public class VmPMapUtlBlockLinesIteratorTests
{
    [Fact]
    public void Next_WalksBlocksInOrder()
    {
        var state = VmPMapUtlBlockLinesIterator.Create(1, 1, 2, 1, keepValidCount: true, validCount: 1);
        var lines = new Dictionary<(int x, int y), IReadOnlyList<int>>
        {
            [(1, 1)] = new List<int> { 10, 11, -1 },
            [(2, 1)] = new List<int> { 20, -1 },
        };
        IReadOnlyList<int> Get(int x, int y) => lines.GetValueOrDefault((x, y)) ?? new List<int> { -1 };
        var first = VmPMapUtlBlockLinesIterator.Next(state, (_, _) => true, Get);
        var second = VmPMapUtlBlockLinesIterator.Next(state, (_, _) => true, Get);
        Assert.Equal(10, first);
        Assert.Equal(11, second);
    }
}
