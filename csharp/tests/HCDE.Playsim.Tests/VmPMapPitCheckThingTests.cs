namespace HCDE.Playsim.Tests;

public class VmPMapPitCheckThingTests
{
    [Fact]
    public void SkipSelf_WhenSame() =>
        Assert.True(VmPMapPitCheckThing.SkipSelf(new AActorRef(1), new AActorRef(1)));

    [Fact]
    public void TouchyDetonate_WhenAligned() =>
        Assert.True(VmPMapPitCheckThing.TouchyDetonate(true, true, 1, true, true, true));

    [Fact]
    public void RipperSkipsCorpse() =>
        Assert.True(VmPMapPitCheckThing.RipperSkipsCorpse(true, true, false));

    [Fact]
    public void ProjectileClipHeight_UsesPassHeight() =>
        Assert.Equal(24, VmPMapPitCheckThing.ProjectileClipHeight(24, false, 40));

    [Fact]
    public void SolidBlock_MirrorBlocksPlayer() =>
        Assert.True(VmPMapPitCheckThing.SolidBlock(false, true, false, true));
}
