namespace HCDE.Client.Tests;

public class VmMusicWildMidiSettingsTests
{
    private sealed class Device : IVmMusicWildMidiSettingsDevice
    {
        public List<(int, int)> Options { get; } = [];
        public List<(int, int, int)> Instruments { get; } = [];
        public void SetOption(int mask, int value) => Options.Add((mask, value));
        public void LoadInstrument(int bank, int drumSet, int instrument) => Instruments.Add((bank, drumSet, instrument));
    }

    [Theory]
    [InlineData(false, false, 0)]
    [InlineData(true, false, 2)]
    [InlineData(false, true, 4)]
    [InlineData(true, true, 6)]
    public void InitializationUpdatesBothFlagsTogether(bool resampling, bool reverb, int expected)
    {
        var device = new Device();
        VmMusicWildMidiSettings.Initialize(device, resampling, reverb);
        Assert.Equal((6, expected), Assert.Single(device.Options));
    }

    [Fact]
    public void PrecachePreservesPackedFieldsOrderDuplicatesAndBothHighBits()
    {
        var device = new Device();
        VmMusicWildMidiSettings.PrecacheInstruments(device, [0, 0x7fff, 0xffff, 0]);
        Assert.Equal(new[] { (0, 0, 0), (127, 1, 127), (127, 3, 127), (0, 0, 0) }, device.Instruments);
    }

    [Theory]
    [InlineData("WILDMIDI.REVERB", -1, 4, -4)]
    [InlineData("wildmidi.resampling", 2, 2, 4)]
    [InlineData("wildmidi.reverb", int.MaxValue, 4, -4)]
    public void RuntimeOptionsUseNativeIntegerMultiplication(string name, int value, int mask, int expected)
    {
        var device = new Device();
        Assert.True(VmMusicWildMidiSettings.ChangeInteger(device, name, value));
        Assert.Equal((mask, expected), Assert.Single(device.Options));
        Assert.False(VmMusicWildMidiSettings.ChangeInteger(device, "unknown", value));
        Assert.Single(device.Options);
    }
}
