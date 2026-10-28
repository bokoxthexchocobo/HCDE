namespace HCDE.Playsim.Tests;

public class VmPAcsVmOpcode953SliceTests
{
    [Fact]
    public void ClampHudColorIndex_ReturnsUntranslatedWhenOutOfRange()
    {
        Assert.Equal(99, VmPAcsVmOpcode953Slice.ClampHudColorIndex(200, 80, 99));
    }

    [Fact]
    public void FadeOutCompatDuration_AppliesMinFrameWhenBothZero()
    {
        var (hold, fade) = VmPAcsVmOpcode953Slice.FadeOutCompatDuration(0f, 0f, 1f / 35f);
        Assert.True(hold > 0 && fade > 0);
    }

    [Fact]
    public void HudMessageAttachId_OrMasksNonZeroId()
    {
        Assert.Equal(0xff000005u, VmPAcsVmOpcode953Slice.HudMessageAttachId(5));
        Assert.Equal(0u, VmPAcsVmOpcode953Slice.HudMessageAttachId(0));
    }

    [Fact]
    public void PushGameType_SelectsTitleDeathmatchCoop()
    {
        Assert.Equal(VmPAcsVmOpcode953Slice.GameTitleMap,
            VmPAcsVmOpcode953Slice.PushGameType(true, false, false));
        Assert.Equal(VmPAcsVmOpcode953Slice.GameNetDeathmatch,
            VmPAcsVmOpcode953Slice.PushGameType(false, true, true));
    }

    [Fact]
    public void PushActivatorHealth_ZeroWithoutActivator()
    {
        Assert.Equal(0, VmPAcsVmOpcode953Slice.PushActivatorHealth(null, 80));
        Assert.Equal(80, VmPAcsVmOpcode953Slice.PushActivatorHealth(new object(), 80));
    }

    [Fact]
    public void ShouldChangeMusic_RequiresLookup()
    {
        Assert.False(VmPAcsVmOpcode953Slice.ShouldChangeMusic(null));
        Assert.True(VmPAcsVmOpcode953Slice.ShouldChangeMusic("track"));
    }

    [Fact]
    public void HudMessageLayer_ExtractsShiftedBits()
    {
        var type = 0x00003000;
        Assert.Equal(3, VmPAcsVmOpcode953Slice.HudMessageLayer(type));
    }

    [Fact]
    public void EndHudMessageStackPointer_SubtractsSixOperands()
    {
        Assert.Equal(4, VmPAcsVmOpcode953Slice.EndHudMessageStackPointer(10));
    }

    [Fact]
    public void PushSinglePlayerFlag_InvertsMultiplayer()
    {
        Assert.Equal(1, VmPAcsVmOpcode953Slice.PushSinglePlayerFlag(false));
        Assert.Equal(0, VmPAcsVmOpcode953Slice.PushSinglePlayerFlag(true));
    }
}
