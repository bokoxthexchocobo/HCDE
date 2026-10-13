namespace HCDE.Client.Tests;

public class VmMusicAdlRuntimeBanksTests
{
    private static VmMusicStartupData Resources(bool gen = false)
    {
        var lump = new byte[8 + 175 * 36];
        "#OPL_II#"u8.CopyTo(lump);
        return VmMusicStartupData.Load(name => gen && name == "GENMIDI" ? lump : null, _ => null, _ => { });
    }

    [Fact]
    public void SuccessfulCustomTogglePreservesGainButFilenameChangeRefreshesIt()
    {
        var device = new Device();
        var settings = new VmMusicFmRuntimeSettings(device, 1);
        var banks = new VmMusicAdlRuntimeBanks(new(false, false, "bank", 14), Resources(), device, settings);
        device.Model = 10;
        banks.ChangeInteger("libadl.usecustombank", 1);
        Assert.Equal(2, settings.OutputGain);
        banks.ChangeInteger("libadl.usecustombank", -1);
        Assert.Equal(new[] { "file:bank" }, device.Calls);
        banks.ChangeString("libadl.custombank", "bank");
        Assert.Equal(2.5f, settings.OutputGain);
        banks.ChangeInteger("libadl.usecustombank", 0);
        Assert.Equal(new[] { "file:bank", "file:bank", "builtin:14" }, device.Calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GenMidiToggleReloadsOnlyWhenResourceExists(bool available)
    {
        var device = new Device();
        var settings = new VmMusicFmRuntimeSettings(device, 1);
        var banks = new VmMusicAdlRuntimeBanks(new(true, false, "bank", 14), Resources(available), device, settings);
        banks.ChangeInteger("libadl.usegenmidi", 1);
        Assert.True(banks.Configuration.UseGenMidi);
        Assert.Equal(available ? new[] { "data" } : Array.Empty<string>(), device.Calls);
        banks.ChangeInteger("libadl.usegenmidi", 2);
        Assert.Equal(available ? 1 : 0, device.Calls.Count);
    }

    [Fact]
    public void RejectedGenMidiAndFileFallBackToBuiltinAndRefreshGain()
    {
        var device = new Device { DataResult = 1, FileResult = 1 };
        var settings = new VmMusicFmRuntimeSettings(device, 0.5f);
        var banks = new VmMusicAdlRuntimeBanks(new(false, true, "bad", 14), Resources(true), device, settings);
        device.Model = 2;
        banks.ChangeInteger("libadl.usecustombank", 1);
        Assert.Equal(new[] { "data", "file:bad", "builtin:14" }, device.Calls);
        Assert.Equal(VmMusicAdlBankKind.Builtin, banks.LastLoadedBank);
        Assert.Equal(1.9f, settings.OutputGain);
    }

    [Fact]
    public void BuiltinChangeOverridesEnabledCustomBankAndIgnoresRepeatedValue()
    {
        var device = new Device();
        var settings = new VmMusicFmRuntimeSettings(device, 1);
        var banks = new VmMusicAdlRuntimeBanks(new(true, false, "file", 14), Resources(), device, settings);
        banks.ChangeInteger("libadl.banknum", 8);
        banks.ChangeInteger("libadl.banknum", 8);
        Assert.Equal(new[] { "builtin:8" }, device.Calls);
        Assert.True(banks.Configuration.UseCustomBank);
        Assert.Equal(8, banks.Configuration.BuiltinBank);
    }

    [Fact]
    public void DisabledFilenameAndUnknownKeysDoNotReload()
    {
        var device = new Device();
        var banks = new VmMusicAdlRuntimeBanks(new(false, false, "old", 14), Resources(), device,
            new VmMusicFmRuntimeSettings(device, 1));
        banks.ChangeString("libadl.custombank", "new");
        Assert.Equal("new", banks.Configuration.CustomBank);
        Assert.False(banks.ChangeInteger("libopn.banknum", 8));
        Assert.False(banks.ChangeString("libadl.Custombank", "other"));
        Assert.Empty(device.Calls);
    }

    private sealed class Device : IVmMusicAdlConfigurationDevice
    {
        public List<string> Calls { get; } = [];
        public int Model { get; set; } = 1;
        public int DataResult { get; set; }
        public int FileResult { get; set; }
        public int GetVolumeRangeModel() => Model;
        public int OpenBankData(ReadOnlyMemory<byte> bank) { Calls.Add("data"); return DataResult; }
        public int OpenBankFile(string name) { Calls.Add("file:" + name); return FileResult; }
        public void SetBuiltinBank(int bank) => Calls.Add("builtin:" + bank);
        public void SwitchEmulator(int emulator) { }
        public void SetRunAtPcmRate(bool enabled) { }
        public void SetNumChips(int chips) { }
        public void SetVolumeRangeModel(int model) { }
        public void SetChannelAllocation(int mode) { }
        public void SetSoftPan(bool enabled) { }
        public void SetAutoArpeggio(bool enabled) { }
    }
}
