namespace HCDE.Client.Tests;

public class VmMusicFmRuntimeSettingsTests
{
    [Theory]
    [InlineData("chanalloc", -1, "allocation:-1")]
    [InlineData("emulator", 2, "emulator:2")]
    [InlineData("numchips", 8, "chips:8")]
    [InlineData("fullpan", -1, "pan:True")]
    [InlineData("runatpcmrate", 0, "pcm:False")]
    [InlineData("autoarpeggio", 2, "arpeggio:True")]
    public void CommonIntegerSettingsDispatchForBothSynths(string name, int value, string expected)
    {
        var device = new Device();
        var adl = new VmMusicFmRuntimeSettings((IVmMusicAdlConfigurationDevice)device, 1);
        var opn = new VmMusicFmRuntimeSettings((IVmMusicOpnConfigurationDevice)device, 1);
        device.Calls.Clear();
        Assert.True(adl.ChangeInteger("libadl." + name, value));
        Assert.True(opn.ChangeInteger("libopn." + name, value));
        Assert.Equal(new[] { expected, expected }, device.Calls);
    }

    [Fact]
    public void AdlVolumeChangeUsesEffectiveModelAndCurrentGain()
    {
        var device = new Device();
        var settings = new VmMusicFmRuntimeSettings((IVmMusicAdlConfigurationDevice)device, 0.5f);
        Assert.Equal(1, settings.OutputGain);
        device.EffectiveModel = 10;
        Assert.True(settings.ChangeInteger("libadl.volumemodel", 0));
        Assert.Equal(1.25f, settings.OutputGain);
        Assert.True(settings.ChangeNumber("libadl.gain", 2));
        Assert.Equal(5, settings.OutputGain);
        Assert.Contains("volume:0", device.Calls);
    }

    [Fact]
    public void OpnVolumeChangeDoesNotChangeGainOrQueryModel()
    {
        var device = new Device();
        var settings = new VmMusicFmRuntimeSettings((IVmMusicOpnConfigurationDevice)device, 0.5f);
        settings.ChangeInteger("libopn.volumemodel", 11);
        Assert.Equal(2, settings.OutputGain);
        settings.ChangeNumber("libopn.gain", 0.25);
        Assert.Equal(1, settings.OutputGain);
        Assert.Equal(new[] { "volume:11" }, device.Calls);
    }

    [Fact]
    public void UnknownWrongPrefixAndWrongCaseLeaveSettingsUntouched()
    {
        var device = new Device();
        var settings = new VmMusicFmRuntimeSettings((IVmMusicOpnConfigurationDevice)device, 1);
        Assert.False(settings.ChangeInteger("libadl.numchips", 8));
        Assert.False(settings.ChangeInteger("libopn.Numchips", 8));
        Assert.False(settings.ChangeInteger("libopn.unknown", 8));
        Assert.False(settings.ChangeNumber("libopn.Gain", 2));
        Assert.Empty(device.Calls);
        Assert.Equal(4, settings.OutputGain);
    }

    [Fact]
    public void FailedVolumeChangePropagatesWithoutChangingOutputGain()
    {
        var device = new Device();
        var settings = new VmMusicFmRuntimeSettings((IVmMusicAdlConfigurationDevice)device, 1);
        var failure = new InvalidOperationException("setting rejected");
        device.Failure = failure;
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => settings.ChangeInteger("libadl.volumemodel", 2)));
        Assert.Equal(2, settings.OutputGain);
    }

    private sealed class Device : IVmMusicAdlConfigurationDevice, IVmMusicOpnConfigurationDevice
    {
        public List<string> Calls { get; } = [];
        public int EffectiveModel { get; set; } = 1;
        public Exception? Failure { get; set; }
        public void SwitchEmulator(int value) => Calls.Add($"emulator:{value}");
        public void SetRunAtPcmRate(bool value) => Calls.Add($"pcm:{value}");
        public void SetNumChips(int value) => Calls.Add($"chips:{value}");
        public void SetVolumeRangeModel(int value) { if (Failure is not null) throw Failure; Calls.Add($"volume:{value}"); }
        public void SetChannelAllocation(int value) => Calls.Add($"allocation:{value}");
        public void SetSoftPan(bool value) => Calls.Add($"pan:{value}");
        public void SetAutoArpeggio(bool value) => Calls.Add($"arpeggio:{value}");
        public int GetVolumeRangeModel() { Calls.Add("query"); return EffectiveModel; }
        public int OpenBankData(ReadOnlyMemory<byte> data) => 0;
        public int OpenBankFile(string path) => 0;
        public void SetBuiltinBank(int bank) { }
    }
}
