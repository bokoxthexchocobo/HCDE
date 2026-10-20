namespace HCDE.Client.Tests;

public class VmRenderMemoryArenaTests
{
    [Fact]
    public void AllocateBytes_UsesSixteenByteAlignmentAndNewBlockAtOneMegabyte()
    {
        var arena = new VmRenderMemoryArena();
        var first = arena.AllocateBytes(1);
        Assert.Equal(0, first);
        var second = arena.AllocateBytes(1);
        Assert.Equal(16, second);
        arena.AllocateBytes(VmRenderMemoryArena.BlockSize - 32);
        var third = arena.AllocateBytes(32);
        Assert.Equal(0, third);
        Assert.Equal(1, arena.ActiveBlockIndex);
    }

    [Fact]
    public void Clear_RecyclesBlocksForReuse()
    {
        var arena = new VmRenderMemoryArena();
        arena.AllocateBytes(VmRenderMemoryArena.BlockSize);
        arena.Clear();
        Assert.Equal(0, arena.AllocateBytes(16));
    }
}
