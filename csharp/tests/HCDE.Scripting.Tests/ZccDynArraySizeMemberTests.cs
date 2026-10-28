namespace HCDE.Scripting.Tests;

public class ZccDynArraySizeMemberTests
{
    [Fact]
    public void IsSizeMember_DetectsNativeBuiltin()
    {
        Assert.True(ZccDynArraySizeMember.IsSizeMember("Size"));
        Assert.False(ZccDynArraySizeMember.IsSizeMember("Push"));
    }
}
