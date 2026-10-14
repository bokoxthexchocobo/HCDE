using HCDE.Playsim;

namespace HCDE.Client.Tests;

public class SoftwarePlayerBlendTests
{
    [Fact]
    public void NativeAccumulatorPreservesEarlierColorWeight()
    {
        var frame = new SoftwareFrame(1, 1); var blend = new PlayerBlendAccumulator();
        blend.Add(255, 0, 0, 0.5f); blend.Add(0, 0, 255, 0.5f); blend.Apply(frame);
        Assert.Equal(new byte[] { 127, 0, 63 }, frame.Pixels);
    }

    [Fact]
    public void ThreeColorsAccumulateBeforePixelRounding()
    {
        var frame = new SoftwareFrame(1, 1); var blend = new PlayerBlendAccumulator();
        blend.Add(255, 0, 0, 0.25f); blend.Add(0, 255, 0, 0.5f); blend.Add(0, 0, 255, 0.5f);
        blend.Apply(frame); Assert.Equal(new byte[] { 63, 95, 47 }, frame.Pixels);
    }

    [Fact]
    public void OpaqueFirstBlendAndZeroAlphaFollowNativeRules()
    {
        var frame = new SoftwareFrame(1, 1); frame.Clear(40, 30, 20); var blend = new PlayerBlendAccumulator();
        blend.Add(0, 255, 0, 0); blend.Add(255, 0, 0, 1); blend.Add(0, 0, 255, 0.5f);
        blend.Apply(frame); Assert.Equal(new byte[] { 255, 0, 0 }, frame.Pixels);
    }

    [Fact]
    public void InvalidSettingsRejectBeforeAnyPixelChanges()
    {
        var frame = new SoftwareFrame(1, 1); frame.Clear(40, 30, 20); var player = new PlayerPawn();
        player.GivePowerStrength();
        Assert.Throws<ArgumentOutOfRangeException>(() => SoftwarePlayerBlend.Apply(frame, player, bloodFadeScalar: double.NaN));
        Assert.Equal(new byte[] { 40, 30, 20 }, frame.Pixels);
    }
}
