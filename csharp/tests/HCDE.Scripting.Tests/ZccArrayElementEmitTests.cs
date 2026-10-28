namespace HCDE.Scripting.Tests;

public class ZccArrayElementEmitTests
{
    [Fact]
    public void ShiftBitsForElementSize_FourBytes() =>
        Assert.Equal(2, ZccArrayElementEmit.ShiftBitsForElementSize(4));

    [Fact]
    public void CanUseShiftInsteadOfMul_WhenPowerOfTwo() =>
        Assert.True(ZccArrayElementEmit.CanUseShiftInsteadOfMul(8, 3));

    [Fact]
    public void StaticArrayInBounds_RejectsHighIndex() =>
        Assert.False(ZccArrayElementEmit.StaticArrayInBounds(10, 8, ZccArrayElementEmit.InvalidSizeAddr, false));

    [Fact]
    public void EmitBoundR_WhenResizable() =>
        Assert.True(ZccArrayElementEmit.EmitBoundR(12, false, false));
}
