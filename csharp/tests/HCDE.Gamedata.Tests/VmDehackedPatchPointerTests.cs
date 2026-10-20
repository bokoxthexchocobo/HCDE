namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchPointerTests
{
    [Fact]
    public void ResolveBoomPointerZero_FindsIndex()
    {
        var conv = new List<int> { 5, 9, 12 };
        var idx = VmDehackedPatchPointer.ResolveBoomPointerZero("Pointer 0 (x 9)", conv);
        Assert.Equal(1, idx);
    }
}
