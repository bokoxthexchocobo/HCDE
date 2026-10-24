namespace HCDE.Playsim.Tests;

public class VmPLnspecFindExecuteSpecialTests
{
    [Fact]
    public void BinarySearchMid_Even() =>
        Assert.Equal(5, VmPLnspecFindExecuteSpecial.BinarySearchMid(0, 10));

    [Fact]
    public void TeleportAlias_Returns70() =>
        Assert.Equal(70, VmPLnspecFindExecuteSpecial.TeleportAliasNumber());

    [Fact]
    public void ExecuteInRange_Valid() =>
        Assert.True(VmPLnspecFindExecuteSpecial.ExecuteInRange(5, 300));

    [Fact]
    public void SpecialInTable_Bounds() =>
        Assert.True(VmPLnspecFindExecuteSpecial.SpecialInTable(10, 285));

    [Fact]
    public void TeleportSpecialAlias_Name() =>
        Assert.True(VmPLnspecFindExecuteSpecial.TeleportSpecialAlias("TeleportSpecial"));

    [Fact]
    public void MaxLineSpecialIndex() =>
        Assert.Equal(284, VmPLnspecFindExecuteSpecial.MaxLineSpecialIndex(285));
}
