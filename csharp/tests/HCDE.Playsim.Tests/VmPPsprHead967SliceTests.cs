namespace HCDE.Playsim.Tests;

public class VmPPsprHead967SliceTests
{
    [Fact]
    public void WrfNoFire_CombinesPrimaryAndSecondary()
    {
        Assert.Equal(
            VmPPsprHead967Slice.WrfNoPrimary | VmPPsprHead967Slice.WrfNoSecondary,
            VmPPsprHead967Slice.WrfNoFire);
    }

    [Fact]
    public void FindButtonRowByReadyFlag_ReturnsReloadRow()
    {
        var row = VmPPsprHead967Slice.FindButtonRowByReadyFlag(VmPPsprHead967Slice.WrfAllowReload);
        Assert.NotNull(row);
        Assert.Equal(VmPPsprHead967Slice.WeaponFlagReloadOk, row.Value.StateFlag);
        Assert.Equal(VmPPsprHead967Slice.ButtonReload, row.Value.ButtonFlag);
        Assert.Equal("Reload", row.Value.StateName);
    }

    [Fact]
    public void ReadyFlagAllowsButton_RequiresStateBit()
    {
        var row = VmPPsprHead967Slice.FindButtonRowByReadyFlag(VmPPsprHead967Slice.WrfAllowZoom)!.Value;
        Assert.True(VmPPsprHead967Slice.ReadyFlagAllowsButton(VmPPsprHead967Slice.WeaponFlagZoomOk, row));
        Assert.False(VmPPsprHead967Slice.ReadyFlagAllowsButton(0, row));
    }

    [Fact]
    public void InputPressedForRow_DetectsButtonBit()
    {
        var row = VmPPsprHead967Slice.FindButtonRowByReadyFlag(VmPPsprHead967Slice.WrfAllowUser3)!.Value;
        Assert.True(VmPPsprHead967Slice.InputPressedForRow(VmPPsprHead967Slice.ButtonUser3, row));
    }

    [Fact]
    public void ButtonChecks_ContainsSixNativeRows()
    {
        Assert.Equal(6, VmPPsprHead967Slice.ButtonChecks.Count);
        Assert.Contains(VmPPsprHead967Slice.ButtonChecks, r => r.StateName == "User4");
    }

    [Fact]
    public void DpspriteFlagBitNames_MatchesNativeRegistrationCount()
    {
        Assert.Equal(9, VmPPsprHead967Slice.DpspriteFlagBitNames.Count);
    }

    [Fact]
    public void Lowerspeed_MatchesNativeMacro()
    {
        Assert.Equal(6.0, VmPPsprHead967Slice.Lowerspeed);
    }
}
