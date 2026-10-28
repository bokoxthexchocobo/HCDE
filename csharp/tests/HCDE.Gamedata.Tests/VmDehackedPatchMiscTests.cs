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

    [Fact]
    public void ProcessMiscLine_AppliesStructKey_InitialHealth()
    {
        var deh = new VmDehackedPatchMisc.DehMiscInfo();
        var infighting = 0;
        VmDehackedPatchMisc.ProcessMiscLine(
            deh,
            ref infighting,
            "Initial Health",
            "150",
            _ => -1,
            null,
            null);
        Assert.Equal(150, deh.StartHealth);
    }

    [Fact]
    public void ProcessMiscLine_BfgCells_WhenNotInStructTable()
    {
        var deh = new VmDehackedPatchMisc.DehMiscInfo();
        var infighting = 0;
        VmDehackedPatchMisc.ProcessMiscLine(
            deh,
            ref infighting,
            "BFG Cells/Shot",
            "40",
            _ => -1,
            null,
            null);
        Assert.Equal(40, deh.BfgCells);
    }

    [Fact]
    public void RunPatchMiscLoop_StopsWhenGetLineNotOne()
    {
        var deh = new VmDehackedPatchMisc.DehMiscInfo();
        var infighting = 0;
        var calls = 0;
        var result = VmDehackedPatchMisc.RunPatchMiscLoop(
            () => calls++ == 0 ? 1 : 0,
            () => "Initial Bullets",
            () => "50",
            deh,
            ref infighting,
            _ => -1);
        Assert.Equal(0, result);
        Assert.Equal(50, deh.StartBullets);
    }

    [Fact]
    public void FinalizeMiscInfighting_NormalizesDdAlias()
    {
        Assert.Equal(1, VmDehackedPatchMisc.FinalizeMiscInfighting(0xDD));
        Assert.Equal(0, VmDehackedPatchMisc.FinalizeMiscInfighting(2));
    }
}
