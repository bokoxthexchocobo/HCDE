namespace HCDE.Playsim.Tests;

public class VmPAcsCallfuncEnum1022SliceTests
{
    [Fact]
    public void PrimaryEnum_AnchorsMatchNative()
    {
        Assert.Equal(60, VmPAcsCallfuncEnum1022Slice.AcsfLineAttack);
        Assert.Equal(92, VmPAcsCallfuncEnum1022Slice.AcsfWarp);
        Assert.Equal(99, VmPAcsCallfuncEnum1022Slice.AcsfCheckActorState);
    }

    [Fact]
    public void ExtendedEnum_AnchorsMatchNative()
    {
        Assert.Equal(200, VmPAcsCallfuncEnum1022Slice.AcsfCheckClass);
        Assert.Equal(207, VmPAcsCallfuncEnum1022Slice.AcsfFloor);
    }

    [Fact]
    public void IsReservedZandronumGap_Blocks100Through199()
    {
        Assert.True(VmPAcsCallfuncEnum1022Slice.IsReservedZandronumGap(100));
        Assert.False(VmPAcsCallfuncEnum1022Slice.IsReservedZandronumGap(99));
        Assert.False(VmPAcsCallfuncEnum1022Slice.IsReservedZandronumGap(200));
    }

    [Fact]
    public void UsesHudOrCvarStack_FlagsCvarCluster()
    {
        Assert.True(VmPAcsCallfuncEnum1022Slice.UsesHudOrCvarStack(
            VmPAcsCallfuncEnum1022Slice.AcsfGetCvarString));
        Assert.False(VmPAcsCallfuncEnum1022Slice.UsesHudOrCvarStack(
            VmPAcsCallfuncEnum1022Slice.AcsfWarp));
    }

    [Fact]
    public void ResolvePlayActorSoundSlot_DelegatesToExistingSoundMapper()
    {
        var id = VmPAcsCallfuncEnum1022Slice.ResolvePlayActorSoundSlot(
            VmAcsActorPropertySound.SoundPain, 1, 2, 7, 3, 4, 5, 6, 8, 9, 10, 11);
        Assert.Equal(7, id);
    }
}
