namespace HCDE.Scripting.Tests;

public class ZccArrayElementResolveTests
{
    [Fact]
    public void ConstantIndexOutOfBounds_DetectsHighIndex() =>
        Assert.True(ZccArrayElementResolve.ConstantIndexOutOfBounds(5, 5));

    [Fact]
    public void CanFoldConstantField_WhenStaticMember() =>
        Assert.True(ZccArrayElementResolve.CanFoldConstantField(true, true, false, 1, 1, 2, 3, 4));

    [Fact]
    public void StaticArraySizeAddr_AddsPointerSize() =>
        Assert.Equal(12u, ZccArrayElementResolve.StaticArraySizeAddr(4, 8));

    [Fact]
    public void InvalidArraySubscript_WhenNotPointerToArray() =>
        Assert.True(ZccArrayElementResolve.InvalidArraySubscript(false, true));
}
