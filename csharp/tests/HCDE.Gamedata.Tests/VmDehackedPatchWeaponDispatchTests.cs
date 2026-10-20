namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchWeaponDispatchTests
{
    [Fact]
    public void IsWeaponSlotValid_AcceptsInRange() =>
        Assert.True(VmDehackedPatchWeaponDispatch.IsWeaponSlotValid(3, 9));

    [Fact]
    public void FrameLabelForPrefix_SwapsSelectDeselect() =>
        Assert.Equal("Select", VmDehackedPatchWeaponDispatch.FrameLabelForPrefix("Deselect frame"));

    [Fact]
    public void NormalizeAmmoTypeIndex_ClampsHigh() =>
        Assert.Equal(5, VmDehackedPatchWeaponDispatch.NormalizeAmmoTypeIndex(99, 4));

    [Fact]
    public void ParseMbf21Line_CombinesNumericAndName() =>
        Assert.Equal(3u, VmDehackedPatchWeaponDispatch.ParseMbf21Line("1|SILENT", ["NOTHRUST", "SILENT"]));

    [Fact]
    public void ResolveReadySound_ChainsawUsesIdle() =>
        Assert.Equal(42, VmDehackedPatchWeaponDispatch.ResolveReadySound(67, 67, 42));

    [Fact]
    public void TryApplyWeaponFrameLine_RecordsBobbingReady() =>
        Assert.True(VmDehackedPatchWeaponDispatch.TryApplyWeaponFrameLine(
            new VmDehackedPatchWeaponDispatch.WeaponFrameLine("Bobbing frame", 12),
            new Dictionary<string, int>(),
            out var ready));
        Assert.Equal(12, ready);
}
