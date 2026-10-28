namespace HCDE.Playsim.Tests;

public class VmPMapFileHead961SliceTests
{
    [Fact]
    public void ShouldPassThroughPlayer_RequiresFlagFriendAndPlayerMo()
    {
        Assert.True(VmPMapFileHead961Slice.ShouldPassThroughPlayer(
            VmPMapFileHead961Slice.Df3NoPlayerClip,
            otherIsControllingPlayer: true,
            selfIsFriendOfOther: true));
        Assert.False(VmPMapFileHead961Slice.ShouldPassThroughPlayer(
            0,
            otherIsControllingPlayer: true,
            selfIsFriendOfOther: true));
    }

    [Fact]
    public void ResolveCanCollideVirtualIndex_CachesAfterFirstResolve()
    {
        var resolved = VmPMapFileHead961Slice.ResolveCanCollideVirtualIndex(
            VmPMapFileHead961Slice.UnresolvedVirtualIndex,
            42);
        Assert.Equal(42u, resolved);
        Assert.Equal(99u, VmPMapFileHead961Slice.ResolveCanCollideVirtualIndex(7, 99));
    }

    [Fact]
    public void ActorVirtualTableLargeEnough_MatchesNativeBoundsCheck()
    {
        Assert.True(VmPMapFileHead961Slice.ActorVirtualTableLargeEnough(10, 5));
        Assert.False(VmPMapFileHead961Slice.ActorVirtualTableLargeEnough(5, 5));
    }

    [Fact]
    public void CollidePairAllowed_BothVirtualCallsMustPass()
    {
        Assert.True(VmPMapFileHead961Slice.CollidePairAllowed(1, 1));
        Assert.False(VmPMapFileHead961Slice.CollidePairAllowed(1, 0));
    }

    [Fact]
    public void ShouldRunSecondCanCollideVirtual_SkipsWhenFirstBlocks()
    {
        Assert.False(VmPMapFileHead961Slice.ShouldRunSecondCanCollideVirtual(firstBlocked: true));
    }

    [Fact]
    public void AppendSpechitWhenNeeded_RecordsPortalAndSpecialSeparately()
    {
        var spechit = 0;
        var portal = 0;
        VmPMapFileHead961Slice.AppendSpechitWhenNeeded(
            special: 12,
            isPortal: true,
            () => spechit++,
            () => portal++);
        Assert.Equal(1, spechit);
        Assert.Equal(1, portal);
    }

    [Fact]
    public void ReverseCollideMateParam_TogglesBetweenActors()
    {
        Assert.True(VmPMapFileHead961Slice.ReverseCollideMateParam(firstPass: false));
    }

    [Fact]
    public void EvaluateFirstCanCollideVirtual_BlocksWhenVmReturnsZero()
    {
        var ran = VmPMapFileHead961Slice.EvaluateFirstCanCollideVirtual(
            virtualsSize: 8,
            virtualIndex: 3,
            hasVirtualFunction: true,
            vmReturnValue: 0,
            out var blocked);
        Assert.True(ran);
        Assert.True(blocked);
    }

    [Fact]
    public void DefaultCvarPolicy_MatchesNativeArchiveDefaults()
    {
        Assert.True(VmPMapFileHead961Slice.DefaultClBloodSplats);
        Assert.Equal(0, VmPMapFileHead961Slice.DefaultSvSmartAim);
        Assert.False(VmPMapFileHead961Slice.DefaultClDoAutoaim);
        Assert.True(VmPMapFileHead961Slice.DefaultSvAutocompat);
    }
}
