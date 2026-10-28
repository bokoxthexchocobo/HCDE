namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchThingCombatExtrasTests
{
    [Fact]
    public void TryApplyLine_SetsBloodColorIndex()
    {
        var extras = new VmDehackedThingCombatExtras();
        Assert.True(VmDehackedPatchThingCombatExtrasParser.TryApplyLine("blood color", "3", extras));
        Assert.Equal(unchecked((int)0xff6357af), extras.BloodColor);
    }

    [Fact]
    public void ParseMbf21Bits_CombinesMnemonicAndNumeric()
    {
        var flags = VmDehackedMbf21FlagNames.KnownFlags;
        var value = VmDehackedPatchThingCombatExtrasParser.ParseMbf21Bits("BOSS, 4", flags);
        Assert.NotEqual(0u, value);
    }
}
