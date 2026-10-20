namespace HCDE.Client.Tests;

public class VmAutomapDrawThingsTests
{
    [Fact]
    public void ShouldDrawThingOnMap_RespectsCheatAndFlags()
    {
        Assert.True(VmAutomapDrawThings.ShouldDrawThingOnMap(1, VmAutomapDrawThings.Mf6NotOnAutomap, false, 0));
        Assert.False(VmAutomapDrawThings.ShouldDrawThingOnMap(0, VmAutomapDrawThings.Mf6NotOnAutomap, false, 0));
    }

    [Fact]
    public void ClassifyThingTriangleColor_MonsterFriendly()
    {
        var kind = VmAutomapDrawThings.ClassifyThingTriangleColor(
            VmAutomapDrawThings.MfFriendly,
            VmAutomapDrawThings.Mf3IsMonster,
            false, false, false, false, -1);
        Assert.Equal(VmAutomapDrawThings.ThingColorKind.Friend, kind);
    }

    [Fact]
    public void ComputeSpriteRotationIndex_ClampedToZeroSixteen()
    {
        var rot = VmAutomapDrawThings.ComputeSpriteRotationIndex(0, 0, false, false, 0);
        Assert.InRange(rot, 0, 15);
    }
}

public class VmAutomapDrawerComposeTests
{
    [Fact]
    public void ComputeKeyboardPanDelta_AccumulatesDirections()
    {
        var (x, y) = VmAutomapDrawerCompose.ComputeKeyboardPanDelta(true, false, false, true, 10, 1, 35);
        Assert.True(x < 0);
        Assert.True(y < 0);
    }

    [Fact]
    public void ShouldDrawThingsLayer_MatchesCheatGate()
    {
        Assert.True(VmAutomapDrawerCompose.ShouldDrawThingsLayer(2, false));
        Assert.False(VmAutomapDrawerCompose.ShouldDrawThingsLayer(4, false));
    }

    [Fact]
    public void CrosshairPixel_CentersHorizontally()
    {
        var (cx, cy) = VmAutomapDrawerCompose.CrosshairPixel(640, 480);
        Assert.Equal(320, cx);
        Assert.Equal(240, cy);
    }
}
