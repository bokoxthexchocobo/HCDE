namespace HCDE.Playsim.Tests;

public class VmAcsMarkWorldVarStringsTests
{
    [Fact]
    public void MarkWorldVarStringsNative_MarksVarsAndEachArray()
    {
        var arrays = new List<IntPtr>();
        VmAcsMarkWorldVarStrings.MarkWorldVarStringsNative(
            (_, size) => Assert.Equal(3, size),
            IntPtr.Zero,
            3,
            [new IntPtr(1), new IntPtr(2)],
            ptr => arrays.Add(ptr));
        Assert.Equal([new IntPtr(1), new IntPtr(2)], arrays);
    }
}
