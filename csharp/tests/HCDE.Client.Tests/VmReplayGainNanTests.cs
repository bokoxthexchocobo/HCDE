namespace HCDE.Client.Tests;

public class VmReplayGainNanTests
{
    [Theory]
    [InlineData("nan", false)]
    [InlineData("-nan", true)]
    [InlineData("+NaN", false)]
    [InlineData("nan(payload)", false)]
    [InlineData("-nan(123)", true)]
    [InlineData("nan(", false)]
    public void GenericNanPreservesSignAndWindowsRuntimeMantissa(string token, bool negative)
    {
        var value = Assert.Single(VmReplayGainIni.Parse("[Gains]\nsong=" + token)).Value;
        var bits = OperatingSystem.IsWindows() ? 0x7fffffff : 0x7fc00000;
        if (negative) bits |= int.MinValue;
        Assert.Equal(bits, BitConverter.SingleToInt32Bits(value));
        float? stored = null;
        VmReplayGainCommand.Execute(true, "song", 1, ["setreplaygain", token],
            (_, gain) => stored = gain, () => { }, _ => { }, _ => throw new InvalidOperationException());
        Assert.Equal(bits, BitConverter.SingleToInt32Bits(Assert.IsType<float>(stored)));
    }

    [Theory]
    [InlineData("nan(ind)", false)]
    [InlineData("-nan(ind)", true)]
    [InlineData("nan(snan)", false)]
    [InlineData("-nan(snan)", true)]
    public void SpecialNanFormsMatchWindowsFloatConversion(string token, bool negative)
    {
        var bits = 0x7fc00000;
        if (negative || OperatingSystem.IsWindows() && token == "nan(ind)") bits |= int.MinValue;
        var value = Assert.Single(VmReplayGainIni.Parse("[Gains]\nsong=" + token)).Value;
        Assert.Equal(bits, BitConverter.SingleToInt32Bits(value));
    }
}
