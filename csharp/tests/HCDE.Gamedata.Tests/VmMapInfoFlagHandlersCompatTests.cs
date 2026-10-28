namespace HCDE.Gamedata.Tests;

public class VmMapInfoFlagHandlersCompatTests
{
    [Fact]
    public void TryGetCompatHandler_FindsNoId24()
    {
        Assert.True(VmMapInfoFlagHandlersCompat.TryGetCompatHandler("compat_noid24", out var h));
        Assert.Equal(VmMapInfoFlagHandlerType.CompatFlag, h.Type);
    }
}
