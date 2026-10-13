namespace HCDE.Client.Tests;

public class VmReplayGainTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(20, 10)]
    [InlineData(-20, .1f)]
    public void DecibelConversionsMatchNativeAmplitudeFormula(float decibels, float amplitude)
    {
        Assert.Equal(amplitude, VmReplayGain.DecibelsToAmplitude(decibels), 6);
        Assert.Equal(decibels, VmReplayGain.AmplitudeToDecibels(amplitude), 5);
    }

    [Theory]
    [InlineData(20, 10)]
    [InlineData(-40, -40)]
    public void ManualGainCapsOnlyUpperLimitAndStoresThenSavesThenApplies(float input, float stored)
    {
        var calls = new List<string>();
        var result = VmReplayGain.SetManualGain("hash", input,
            (hash, gain) => { Assert.Equal("hash", hash); Assert.Equal(stored, gain); calls.Add("store"); },
            () => calls.Add("save"), amplitude =>
            { Assert.Equal(VmReplayGain.DecibelsToAmplitude(stored), amplitude); calls.Add("apply"); });
        Assert.Equal(VmReplayGain.DecibelsToAmplitude(stored), result);
        Assert.Equal(new[] { "store", "save", "apply" }, calls);
    }

    [Fact]
    public void NativeNonfiniteArithmeticIsRetained()
    {
        Assert.Equal(float.NegativeInfinity, VmReplayGain.AmplitudeToDecibels(0));
        Assert.True(float.IsNaN(VmReplayGain.AmplitudeToDecibels(-1)));
        Assert.Equal(0, VmReplayGain.DecibelsToAmplitude(float.NegativeInfinity));
        Assert.True(float.IsNaN(VmReplayGain.DecibelsToAmplitude(float.NaN)));
    }

    [Fact]
    public void SaveFailurePropagatesBeforeVolumeApplication()
    {
        var stored = false;
        Assert.Throws<InvalidOperationException>(() => VmReplayGain.SetManualGain("hash", 2,
            (_, _) => stored = true, () => throw new InvalidOperationException("save failed"),
            _ => throw new Exception("unexpected apply")));
        Assert.True(stored);
    }
}
