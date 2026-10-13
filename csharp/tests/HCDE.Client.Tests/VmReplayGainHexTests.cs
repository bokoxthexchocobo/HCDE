namespace HCDE.Client.Tests;

public class VmReplayGainHexTests
{
    [Theory]
    [InlineData("0x1.8p+1tail", 3f)]
    [InlineData("-0X.8P2", -2f)]
    [InlineData("+0xap-1", 5f)]
    [InlineData("0x10", 16f)]
    [InlineData("0x1p+", 1f)]
    [InlineData("0xg", 0f)]
    [InlineData("0x1p-999999999999999999999", 0f)]
    [InlineData("0x1p999999999999999999999", float.PositiveInfinity)]
    [InlineData("0x0p999999999999999999999", 0f)]
    [InlineData("0xffffffffffffffffffffffffffffffffp-124", 16f)]
    public void ParsesHexadecimalPrefixThroughSharedGainParser(string token, float expected)
    {
        var entry = Assert.Single(VmReplayGainIni.Parse("[Gains]\nsong=" + token));
        Assert.Equal(expected, entry.Value);
        float? stored = null;
        VmReplayGainCommand.Execute(true, "song", 1, ["setreplaygain", token],
            (_, gain) => stored = gain, () => { }, _ => { }, _ => throw new InvalidOperationException());
        Assert.Equal(expected > 10 ? 10 : expected, stored);
    }

    [Fact]
    public void HexadecimalNegativeZeroKeepsSign()
    {
        var gain = Assert.Single(VmReplayGainIni.Parse("[Gains]\nsong=-0x0p0")).Value;
        Assert.Equal(int.MinValue, BitConverter.SingleToInt32Bits(gain));
    }
}
