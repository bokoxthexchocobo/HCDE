namespace HCDE.Client.Tests;

public class VmReplayGainNumberFormatTests
{
    [Theory]
    [InlineData(float.PositiveInfinity, "Inf")]
    [InlineData(float.NegativeInfinity, "-Inf")]
    [InlineData(1.25f, "1.250000")]
    [InlineData(1.2345678f, "1.234568")]
    public void StatusFormatterUsesNativeFloatPromotionAndSpecialValues(float value, string expected)
    {
        Assert.Equal(expected, VmMusicNumberFormat.FixedSix(value));
    }

    [Theory]
    [InlineData(0, "0.000000")]
    [InlineData(int.MinValue, "-0.000000")]
    [InlineData(0x7fc00000, "NaN")]
    [InlineData(unchecked((int)0xffc00000), "-NaN")]
    public void StatusFormatterPreservesZeroAndNanSigns(int bits, string expected)
    {
        Assert.Equal(expected, VmMusicNumberFormat.FixedSix(BitConverter.Int32BitsToSingle(bits)));
    }

    [Theory]
    [InlineData(0f, "-Inf")]
    [InlineData(float.PositiveInfinity, "Inf")]
    [InlineData(1f, "0.000000")]
    public void StatusUsesNativeSpecialValueNames(float volume, string expected)
    {
        var messages = new List<string>();
        VmReplayGainCommand.Execute(true, "song", volume, ["setreplaygain"],
            (_, _) => throw new InvalidOperationException(), () => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException(), messages.Add);
        Assert.Equal($"Current replay gain is {expected} dB\n", messages[1]);
    }
}
