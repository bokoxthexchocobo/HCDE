namespace HCDE.Client.Tests;

public class VmAutomapRotationAndPlayersTests
{
    [Fact]
    public void Rotate_90Degrees_SwapsAxes()
    {
        var cache = VmAutomapRotationAndPlayers.UpdateRotationCache(null, Math.PI / 2);
        var (x, y) = VmAutomapRotationAndPlayers.Rotate(1, 0, cache);
        Assert.True(Math.Abs(x) < 1e-6);
        Assert.True(Math.Abs(y - 1) < 1e-6);
    }

    [Fact]
    public void TransformLineCharacter_AppliesScaleAndOffset()
    {
        var guy = new[] { new VmAutomapRotationAndPlayers.LineGuyVertex(1, 0, 2, 0) };
        var segs = VmAutomapRotationAndPlayers.TransformLineCharacter(guy, 2, 0, 10, 5, null).ToList();
        Assert.Single(segs);
        Assert.Equal((12, 5, 14, 5), segs[0]);
    }

    [Fact]
    public void ShouldDeferSinglePlayerArrowToThingSprites_WhenCheatAndSprites()
    {
        Assert.True(VmAutomapRotationAndPlayers.ShouldDeferSinglePlayerArrowToThingSprites(2, 1));
        Assert.False(VmAutomapRotationAndPlayers.ShouldDeferSinglePlayerArrowToThingSprites(4, 1));
    }

    [Fact]
    public void ShouldSkipPlayerIcon_RespectsDmflags2Allies()
    {
        Assert.True(VmAutomapRotationAndPlayers.ShouldSkipPlayerIcon(
            true, true, 1, 0, VmAutomapRotationAndPlayers.Df2NoAutomapAllies,
            false, false, true, false));
    }

    [Fact]
    public void ResolvePlayerColorFromHsv_ReturnsRgbBytes()
    {
        var (r, g, b) = VmAutomapRotationAndPlayers.ResolvePlayerColorFromHsv(0, 1, 1);
        Assert.Equal(255, r);
        Assert.Equal(0, g);
        Assert.Equal(0, b);
    }
}
