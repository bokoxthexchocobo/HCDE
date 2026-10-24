namespace HCDE.Gamedata.Tests;

public class VmDoomFontConstantsTests
{
    [Fact]
    public void HuFontSize_MatchesNativeRange()
    {
        Assert.Equal(0xFF - (byte)'!' + 1, VmDoomFontConstants.HuFontSize);
    }
}
