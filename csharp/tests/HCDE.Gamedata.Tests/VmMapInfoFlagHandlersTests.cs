namespace HCDE.Gamedata.Tests;

public class VmMapInfoFlagHandlersTests
{
    [Fact]
    public void TryGetHandler_FindsNoJump()
    {
        Assert.True(VmMapInfoFlagHandlers.TryGetHandler("nojump", out var handler));
        Assert.Equal(VmMapInfoFlagHandlerType.SetFlag, handler.Type);
    }

    [Fact]
    public void TryGetHandler_FindsE1M8Special()
    {
        Assert.True(VmMapInfoFlagHandlers.TryGetHandler("e1m8special", out _));
    }
}
