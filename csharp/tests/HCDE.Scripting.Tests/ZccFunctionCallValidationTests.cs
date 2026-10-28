using HCDE.Scripting;

namespace HCDE.Scripting.Tests;

public class ZccFunctionCallValidationTests
{
    [Fact]
    public void HasNullArgument_DetectsEmptySlot()
    {
        Assert.True(ZccFunctionCallValidation.HasNullArgument([1, null, 3], out var index));
        Assert.Equal(1, index);
    }

    [Fact]
    public void HasNullArgument_ReturnsFalseWhenFull()
        => Assert.False(ZccFunctionCallValidation.HasNullArgument([1, 2], out _));
}
