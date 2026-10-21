namespace HCDE.Playsim.Tests;

public class VmAcsBehaviorInitArraysTests
{
    [Fact]
    public void InitArrayFromChunkNative_CapsByArraySize()
    {
        var elems = new int[3];
        var written = VmAcsBehaviorInitArrays.InitArrayFromChunkNative(3, 5, elems, [1, 2, 3, 4, 5]);
        Assert.Equal(3, written);
        Assert.Equal([1, 2, 3], elems);
    }

    [Fact]
    public void TotalArrayCountNative_AddsImportTotal() =>
        Assert.Equal(7, VmAcsBehaviorInitArrays.TotalArrayCountNative(5, 2));

    [Fact]
    public void LoadLibraryTokenNative_ParsesNullTerminatedToken()
    {
        var next = 0;
        var start = VmAcsBehaviorInitArrays.LoadLibraryTokenNative("acs\0next", 0, out next);
        Assert.Equal(0, start);
        Assert.Equal(4, next);
    }
}
