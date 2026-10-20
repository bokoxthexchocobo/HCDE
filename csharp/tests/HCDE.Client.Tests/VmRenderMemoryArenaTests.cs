namespace HCDE.Client.Tests;

public class VmRenderMemoryArenaTests
{
    [Fact]
    public void AllocateBytes_UsesSixteenByteAlignmentAndNewBlockAtOneMegabyte()
    {
        using var arena = new VmRenderMemoryArena();
        var first = arena.AllocateBytes(1);
        var second = arena.AllocateBytes(1);
        Assert.Equal(0, first % VmRenderMemoryArena.Alignment);
        Assert.Equal(VmRenderMemoryArena.Alignment, second - first);
        arena.AllocateBytes(VmRenderMemoryArena.BlockSize - VmRenderMemoryArena.Alignment * 2);
        var third = arena.AllocateBytes(VmRenderMemoryArena.Alignment);
        Assert.Equal(0, third % VmRenderMemoryArena.Alignment);
    }

    [Fact]
    public void Clear_RecyclesBlocksForReuse()
    {
        using var arena = new VmRenderMemoryArena();
        var first = arena.AllocateBytes(16);
        arena.AllocateBytes(VmRenderMemoryArena.BlockSize);
        arena.Clear();
        var recycled = arena.AllocateBytes(16);
        Assert.Equal(first, recycled);
    }
}
