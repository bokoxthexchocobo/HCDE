namespace HCDE.Client.Tests;

public class VmMusicVolumeCommandTests
{
    [Theory]
    [InlineData("-3.5tail", -3.5f)]
    [InlineData("0x1.8p1", 3f)]
    [InlineData("invalid", 0f)]
    public void DispatchParsesRawRelativeValueBeforeCallbackWithoutEnablementGuard(string token, float expected)
    {
        var volume = new VmMusicVolume();
        var calls = new List<string>();
        var console = new VmMusicConsole<object>(new("", null), new([]),
            () => throw new InvalidOperationException(), (_, _) => false, _ => { }, _ => { }, _ => { },
            testMusicVolume: arguments => VmMusicVolumeCommand.Execute(volume, arguments,
                value => { Assert.Equal(expected, value); Assert.Equal(expected, volume.RelativeLevel); calls.Add("relative"); },
                () => calls.Add("callback"), _ => throw new InvalidOperationException()));
        Assert.True(console.TryExecute(["testmusicvol", token, "ignored"]));
        Assert.Equal(new[] { "relative", "callback" }, calls);
    }

    [Theory]
    [InlineData(1.25f, "1.25")]
    [InlineData(float.PositiveInfinity, "Inf")]
    [InlineData(float.NegativeInfinity, "-Inf")]
    public void StatusFormatsTwoDecimalsAndNativeSpecialValuesWithoutCallbacks(float value, string expected)
    {
        var volume = new VmMusicVolume();
        volume.SetRelativeLevel(value);
        var messages = new List<string>();
        VmMusicVolumeCommand.Execute(volume, ["testmusicvol"],
            _ => throw new InvalidOperationException(), () => throw new InvalidOperationException(), messages.Add);
        Assert.Equal(new[] { $"Current relative volume is {expected}\n" }, messages);
    }

    [Fact]
    public void StatusRetainsNegativeZeroAndSignedNan()
    {
        Assert.Equal("-0.00", VmMusicNumberFormat.FixedTwo(BitConverter.Int32BitsToSingle(int.MinValue)));
        Assert.Equal("-NaN", VmMusicNumberFormat.FixedTwo(BitConverter.Int32BitsToSingle(unchecked((int)0xffc00000))));
    }
}
