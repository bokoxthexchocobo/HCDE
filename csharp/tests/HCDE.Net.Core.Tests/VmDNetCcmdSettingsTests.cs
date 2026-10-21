namespace HCDE.Net.Core.Tests;

public class VmDNetCcmdSettingsTests
{
    [Fact]
    public void ClearMuteClientMaskNative_ClearsBit() =>
        Assert.Equal(0ul, VmDNetCcmdSettings.ClearMuteClientMaskNative(1ul << 2, 2));

    [Fact]
    public void ShouldEmitAddControllerNative_WhenEligible() =>
        Assert.True(VmDNetCcmdSettings.ShouldEmitAddControllerNative(settingsController: false, add: true));

    [Fact]
    public void ShouldWriteAddControllerNative_WhenAllowed() =>
        Assert.True(VmDNetCcmdSettings.ShouldWriteAddControllerNative(
            settingsController: false, add: true, inGame: true, authoritySlot: false));
}
