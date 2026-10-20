namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchMiscTests
{
    [Fact]
    public void TryApplyKnownKey_SetsStartHealth()
    {
        var info = new VmDehackedMiscInfo();
        Assert.True(VmDehackedPatchMisc.TryApplyKnownKey("Initial Health", 80, info));
        Assert.Equal(80, info.StartHealth);
    }
}
