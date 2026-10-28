namespace HCDE.Playsim.Tests;

public class VmPSetupPrecache979SliceTests
{
    [Fact]
    public void HitlistMerge_SkipsWhenBitSet()
    {
        var hitlist = new byte[] { VmPSetupPrecache979Slice.HitWall, 0 };
        Assert.True(VmPSetupPrecache979Slice.ShouldSkipAddToList(hitlist[0], VmPSetupPrecache979Slice.HitWall));
        Assert.True(VmPSetupPrecache979Slice.TryAddToHitlist(
            hitlist,
            textureIndex: 1,
            VmPSetupPrecache979Slice.HitFlat,
            out var merged));
        Assert.Equal(VmPSetupPrecache979Slice.HitFlat, merged);
    }

    [Fact]
    public void AnimationExpansion_RespectsDiscreteFlag()
    {
        Assert.True(VmPSetupPrecache979Slice.ShouldExpandAnimationFrames(
            texIndex: 12,
            baseIndex: 10,
            numFrames: 5,
            discrete: false));
        Assert.False(VmPSetupPrecache979Slice.ShouldExpandAnimationFrames(
            12,
            10,
            5,
            discrete: true));
    }

    [Fact]
    public void SwitchAndDoorPredicates()
    {
        Assert.True(VmPSetupPrecache979Slice.ShouldUseBoomAnimatedSwitchFallback(1, 1));
        Assert.False(VmPSetupPrecache979Slice.ShouldUseBoomAnimatedSwitchFallback(2, 1));
        Assert.True(VmPSetupPrecache979Slice.ShouldMarkSwitchFrames(switchFound: true, numFrames: 2));
        Assert.True(VmPSetupPrecache979Slice.ShouldMarkAnimatedDoorFrames(true, numTextureFrames: 3));
        Assert.True(VmPSetupPrecache979Slice.PrecacheLevelHeaderInBand(150));
    }

    [Fact]
    public void MarkHitlistRange_MarksUnsetEntries()
    {
        var hitlist = new byte[4];
        var marked = VmPSetupPrecache979Slice.MarkHitlistRange(
            hitlist,
            startIndex: 1,
            frameCount: 2,
            VmPSetupPrecache979Slice.HitSky);
        Assert.Equal(2, marked);
        Assert.Equal(VmPSetupPrecache979Slice.HitSky, hitlist[1]);
    }

    [Fact]
    public void ApplyAddToListPolicy_ReturnsOneWhenMerged()
    {
        var hitlist = new byte[1];
        Assert.Equal(1, VmPSetupPrecache979Slice.ApplyAddToListPolicy(
            hitlist,
            textureIndex: 0,
            VmPSetupPrecache979Slice.HitWall));
        Assert.Equal(0, VmPSetupPrecache979Slice.ApplyAddToListPolicy(
            hitlist,
            0,
            VmPSetupPrecache979Slice.HitWall));
    }
}
