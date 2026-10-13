namespace HCDE.Client.Tests;

public class VmMusicFmInitializationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InitializationGainIsAppliedToGeneratedStereoAudio(bool opn)
    {
        var device = new Device { EffectiveModel = 10 };
        var settings = new VmMusicFmSettings { Gain = 0.25f };
        var gain = opn
            ? VmMusicFmInitialization.InitializeOpn(new() { Settings = settings }, Resources(), device).OutputGain
            : VmMusicFmInitialization.InitializeAdl(new() { Settings = settings }, Resources(), device).OutputGain;
        float[] samples = new float[4];
        Assert.Equal(4, VmMusicFmOutput.ComputeOutput(device, samples, 2, gain));
        var expected = opn ? 1f : 0.625f;
        Assert.Equal(new float[] { expected, -expected, expected * 0.5f, -expected * 0.5f }, samples);
    }

    private static VmMusicStartupData Resources() => VmMusicStartupData.Load(_ => null, _ => [17], _ => { });

    [Fact]
    public void AdlDefaultsFollowNativeOrderAndUseEffectiveVolumeModel()
    {
        var device = new Device { EffectiveModel = 10 };
        var result = VmMusicFmInitialization.InitializeAdl(new(), Resources(), device);
        Assert.Equal(VmMusicAdlBankKind.Builtin, result.Bank);
        Assert.Equal(2.5f, result.OutputGain);
        Assert.Equal(new[] { "emulator:0", "pcm:False", "builtin:14", "chips:6", "volume:0",
            "allocation:-1", "pan:True", "arpeggio:False", "get-volume" }, device.Calls);
    }

    [Fact]
    public void OpnDefaultsLoadBankBeforeEmulatorAndUseEightChips()
    {
        var device = new Device();
        var result = VmMusicFmInitialization.InitializeOpn(new(), Resources(), device);
        Assert.Equal(VmMusicOpnBankKind.ResourceDefault, result.Bank);
        Assert.Equal(4f, result.OutputGain);
        Assert.Equal(new[] { "data", "emulator:0", "pcm:False", "chips:8", "volume:0",
            "allocation:-1", "pan:True", "arpeggio:False" }, device.Calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CustomSettingsAndBankAreForwardedInNativeOrder(bool opn)
    {
        var settings = new VmMusicFmSettings { Chips = 3, Emulator = 2, RunAtPcmRate = true,
            VolumeModel = 2, ChannelAllocation = 1, FullPan = false, AutoArpeggio = true, Gain = 0.5f };
        var device = new Device { EffectiveModel = 2 };
        if (opn)
        {
            var result = VmMusicFmInitialization.InitializeOpn(new() { Bank = new(true, "custom"), Settings = settings }, Resources(), device);
            Assert.Equal(VmMusicOpnBankKind.CustomFile, result.Bank);
            Assert.Equal(2f, result.OutputGain);
            Assert.Equal(new[] { "file:custom", "emulator:2", "pcm:True", "chips:3", "volume:2",
                "allocation:1", "pan:False", "arpeggio:True" }, device.Calls);
        }
        else
        {
            var result = VmMusicFmInitialization.InitializeAdl(new() { Bank = new(true, false, "custom", 14), Settings = settings }, Resources(), device);
            Assert.Equal(VmMusicAdlBankKind.CustomFile, result.Bank);
            Assert.Equal(1.9f, result.OutputGain);
            Assert.Equal(new[] { "emulator:2", "pcm:True", "file:custom", "chips:3", "volume:2",
                "allocation:1", "pan:False", "arpeggio:True", "get-volume" }, device.Calls);
        }
    }

    [Theory]
    [InlineData(0, 3.5f)]
    [InlineData(1, 2f)]
    [InlineData(2, 3.8f)]
    [InlineData(3, 3.5f)]
    [InlineData(4, 3.5f)]
    [InlineData(5, 2f)]
    [InlineData(6, 3.5f)]
    [InlineData(7, 3.5f)]
    [InlineData(8, 3.5f)]
    [InlineData(9, 2f)]
    [InlineData(10, 2.5f)]
    [InlineData(11, 2.5f)]
    [InlineData(99, 3.5f)]
    public void AdlGainMatchesEveryNativeVolumeModel(int model, float multiplier)
    {
        Assert.Equal(multiplier * 0.25f, VmMusicFmInitialization.AdlOutputGain(model, 0.25f));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AdapterSettingExceptionStopsInitialization(bool opn)
    {
        var device = new Device { ThrowOn = "chips:3" };
        var settings = new VmMusicFmSettings { Chips = 3 };
        var error = Assert.Throws<InvalidOperationException>(() =>
        {
            if (opn) VmMusicFmInitialization.InitializeOpn(new() { Settings = settings }, Resources(), device);
            else VmMusicFmInitialization.InitializeAdl(new() { Settings = settings }, Resources(), device);
        });
        Assert.Equal("chips:3", error.Message);
        Assert.Equal("chips:3", device.Calls[^1]);
        Assert.DoesNotContain("get-volume", device.Calls);
    }

    private sealed class Device : IVmMusicAdlConfigurationDevice, IVmMusicOpnConfigurationDevice, IVmMusicFmAudioDevice
    {
        public List<string> Calls { get; } = [];
        public int EffectiveModel { get; init; }
        public string? ThrowOn { get; init; }
        private void Call(string value)
        {
            Calls.Add(value);
            if (ThrowOn == value) throw new InvalidOperationException(value);
        }
        public int OpenBankData(ReadOnlyMemory<byte> bank) { Call("data"); return 0; }
        public int OpenBankFile(string filename) { Call("file:" + filename); return 0; }
        public void SetBuiltinBank(int bank) => Call("builtin:" + bank);
        public void SwitchEmulator(int emulator) => Call("emulator:" + emulator);
        public void SetRunAtPcmRate(bool enabled) => Call("pcm:" + enabled);
        public void SetNumChips(int chips) => Call("chips:" + chips);
        public void SetVolumeRangeModel(int model) => Call("volume:" + model);
        public void SetChannelAllocation(int mode) => Call("allocation:" + mode);
        public void SetSoftPan(bool enabled) => Call("pan:" + enabled);
        public void SetAutoArpeggio(bool enabled) => Call("arpeggio:" + enabled);
        public int GetVolumeRangeModel() { Call("get-volume"); return EffectiveModel; }
        public int Generate(Span<float> stereoSamples)
        {
            new float[] { 1, -1, 0.5f, -0.5f }.CopyTo(stereoSamples);
            return 4;
        }
    }
}
