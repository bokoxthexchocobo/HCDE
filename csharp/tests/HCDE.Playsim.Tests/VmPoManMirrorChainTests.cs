namespace HCDE.Playsim.Tests;

public class VmPoManMirrorChainTests
{
    sealed class MirrorState
    {
        public int PolyId { get; init; }
        public int MirrorPolyId { get; init; }
    }

    [Fact]
    public void Enumerate_WalksTwoMirrorChain()
    {
        var table = new Dictionary<int, MirrorState>
        {
            [1] = new() { PolyId = 1, MirrorPolyId = 2 },
            [2] = new() { PolyId = 2, MirrorPolyId = 0 },
        };
        var ids = VmPoManMirrorChain.Enumerate(
                1,
                id => table.GetValueOrDefault(id),
                s => s.MirrorPolyId)
            .Select(s => s.PolyId)
            .ToList();
        Assert.Equal([1, 2], ids);
    }

    [Fact]
    public void Enumerate_StopsOnMirrorCycle()
    {
        var table = new Dictionary<int, MirrorState>
        {
            [1] = new() { PolyId = 1, MirrorPolyId = 2 },
            [2] = new() { PolyId = 2, MirrorPolyId = 1 },
        };
        var ids = VmPoManMirrorChain.Enumerate(
                1,
                id => table.GetValueOrDefault(id),
                s => s.MirrorPolyId)
            .Select(s => s.PolyId)
            .ToList();
        Assert.Equal([1, 2], ids);
    }
}
