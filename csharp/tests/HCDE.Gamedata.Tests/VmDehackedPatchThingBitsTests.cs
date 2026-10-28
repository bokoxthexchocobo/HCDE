namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchThingBitsTests
{
    [Fact]
    public void RemapMbfFlags_SetsGrenadeBounceStyle()
    {
        var words = new VmDehackedThingBitWords { Flags0 = VmDehackedPatchThingBits.MfBounces };
        VmDehackedPatchThingBits.RemapMbfFlags(words, isMissile: false, isSentient: true);
        Assert.Equal("Grenade", words.BounceStyle);
    }

    [Fact]
    public void ResolveTranslucentAlpha_ReturnsHalf()
    {
        Assert.Equal(0.5, VmDehackedPatchThingBits.ResolveTranslucentAlpha(2));
    }
}
