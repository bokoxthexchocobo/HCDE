namespace HCDE.Gamedata.Tests;

public class VmMapInfoFlagHandlersMidTests
{
    [Fact]
    public void TryGetHandler_FindsStrifeFallingDamage()
    {
        Assert.True(VmMapInfoFlagHandlers.TryGetHandler("strifefallingdamage", out var h));
        Assert.Equal(VmMapInfoFlagHandlerType.SetFlag, h.Type);
    }

    [Fact]
    public void TryGetHandler_FindsCompatNoid24()
    {
        Assert.True(VmMapInfoFlagHandlers.TryGetHandler("compat_noid24", out _));
    }
}
