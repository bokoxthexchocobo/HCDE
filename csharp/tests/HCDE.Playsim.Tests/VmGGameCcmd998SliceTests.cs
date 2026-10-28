namespace HCDE.Playsim.Tests;

public class VmGGameCcmd998SliceTests
{
    [Fact]
    public void WeaponSlotParsing()
    {
        Assert.True(VmGGameCcmd998Slice.TryResolveWeaponSlotFromArgument("3", out var slot));
        Assert.Equal(3, slot);
        Assert.Equal(VmGGameCcmd998Slice.WeaponSlotNone,
            VmGGameCcmd998Slice.ResolveExplicitWeaponSlot(99));
        Assert.Equal(VmGGameCcmd998Slice.WeaponSlotNext,
            VmGGameCcmd998Slice.ResolveWeapNextSlot());
    }

    [Fact]
    public void PauseAndCenterviewGates()
    {
        Assert.True(VmGGameCcmd998Slice.ShouldBlockCenterview(VmGGameCcmd998Slice.CheatTotallyFrozen));
        var reject = VmGGameCcmd998Slice.ResolveNetgamePauseRejectReason(
            netgame: true,
            netDisablePause: VmGGameCcmd998Slice.NetDisablePauseFullyDisabled,
            paused: false,
            localCanControlSettings: true);
        Assert.Equal(VmGGameCcmd998Slice.PauseRejectReason.FullyDisabled, reject);
        Assert.False(VmGGameCcmd998Slice.ShouldQueueSendPause(reject));
    }

    [Fact]
    public void InventoryAndUseCommands()
    {
        Assert.True(VmGGameCcmd998Slice.ShouldShowDisplayNameTagHud(displayNameTags: 1));
        Assert.True(VmGGameCcmd998Slice.CanApplySelectedInventoryUse(inventoryTics: 0, hasPlayerMo: true));
        Assert.Equal(0, VmGGameCcmd998Slice.InventoryTicsAfterInvUseCommand());
        Assert.True(VmGGameCcmd998Slice.ResolveInvUseAllCommand().UseAllSentinel);
        Assert.True(VmGGameCcmd998Slice.ParseUseCommandSubclassFlag("true"));
        Assert.True(VmGGameCcmd998Slice.ParseUseCommandSubclassFlag("1"));
        Assert.True(VmGGameCcmd998Slice.ShouldRequireUseCommandItemName(argumentCount: 2));
        Assert.True(VmGGameCcmd998Slice.ResolveLandCommand().RequestLand);
    }
}
