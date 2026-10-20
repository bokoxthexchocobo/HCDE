namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchMiscTests
{
    [Fact]
    public void LineReady_WhenGetLineReturnsOne() =>
        Assert.True(VmDehackedPatchMisc.LineReady(1));

    [Fact]
    public void BfgCellsKey_Matches() =>
        Assert.True(VmDehackedPatchMisc.BfgCellsKey("BFG Cells/Shot"));

    [Fact]
    public void InfightingIgnore_FlipsSign() =>
        Assert.Equal(-1, VmDehackedPatchMisc.InfightingFromIgnore(true, 1));

    [Fact]
    public void PowerupPrefix_Detected() =>
        Assert.True(VmDehackedPatchMisc.PowerupColorPrefix("Powerup Color Invulnerability"));

    [Fact]
    public void ExplosionStyle_InvalidWhenNegative() =>
        Assert.False(VmDehackedPatchMisc.ExplosionStyleFound(-1));
}
