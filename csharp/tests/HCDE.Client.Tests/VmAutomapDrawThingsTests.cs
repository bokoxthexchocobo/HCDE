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

