namespace HCDE.Client.Tests;

public class VmMusicOpnRuntimeBanksTests
{
    private static VmMusicStartupData Resources() => VmMusicStartupData.Load(_ => null, _ => [17, 42], _ => { });

    [Fact]
    public void ToggleReloadsOnlyWhenBooleanValueChanges()
    {
        var device = new Device();
        var banks = new VmMusicOpnRuntimeBanks(new(false, "custom.wopn"), Resources(), device);
        Assert.True(banks.ChangeInteger("libopn.usecustombank", 0));
        Assert.Empty(device.Calls);
        Assert.True(banks.ChangeInteger("libopn.usecustombank", -1));
        Assert.Equal(VmMusicOpnBankKind.CustomFile, banks.LastLoadedBank);
        banks.ChangeInteger("libopn.usecustombank", 2);
        Assert.Equal(new[] { "file:custom.wopn" }, device.Calls);
        banks.ChangeInteger("libopn.usecustombank", 0);
        Assert.Equal(new[] { "file:custom.wopn", "default" }, device.Calls);
        Assert.Equal(VmMusicOpnBankKind.ResourceDefault, banks.LastLoadedBank);
    }

    [Fact]
    public void DisabledFilenameIsRememberedAndEnabledFilenameAlwaysReloads()
    {
        var device = new Device();
        var banks = new VmMusicOpnRuntimeBanks(new(false, "old"), Resources(), device);
        banks.ChangeString("libopn.custombank", "new");
        Assert.Empty(device.Calls);
        banks.ChangeInteger("libopn.usecustombank", 1);
        banks.ChangeString("libopn.custombank", "new");
        banks.ChangeString("libopn.custombank", "");
        Assert.Equal(new[] { "file:new", "file:new", "default" }, device.Calls);
        Assert.Equal("", banks.Configuration.CustomBank);
    }

    [Fact]
    public void RejectedCustomFileFallsBackToDefault()
    {
        var device = new Device { FileResult = 1 };
        var banks = new VmMusicOpnRuntimeBanks(new(true, "old"), Resources(), device);
        banks.ChangeString("libopn.custombank", "bad");
        Assert.Equal(new[] { "file:bad", "default" }, device.Calls);
        Assert.Equal(VmMusicOpnBankKind.ResourceDefault, banks.LastLoadedBank);
        Assert.Equal(new byte[] { 17, 42 }, device.Data);
    }

    [Fact]
    public void DefaultLoadFailureSurfacesAndRetainsRequestedConfiguration()
    {
        var device = new Device { FileResult = 1, DefaultResult = 9 };
        var banks = new VmMusicOpnRuntimeBanks(new(false, "bad"), Resources(), device);
        Assert.Contains("9", Assert.Throws<InvalidOperationException>(() => banks.ChangeInteger("libopn.usecustombank", 1)).Message);
        Assert.True(banks.Configuration.UseCustomBank);
        Assert.Null(banks.LastLoadedBank);
        device.DefaultResult = 0;
        banks.ChangeString("libopn.custombank", "bad");
        Assert.Equal(VmMusicOpnBankKind.ResourceDefault, banks.LastLoadedBank);
    }

    [Fact]
    public void UnknownSettingsDoNotMutateConfigurationOrLoadBanks()
    {
        var device = new Device();
        var original = new VmMusicOpnBankConfiguration(false, "old");
        var banks = new VmMusicOpnRuntimeBanks(original, Resources(), device);
        Assert.False(banks.ChangeInteger("libadl.usecustombank", 1));
        Assert.False(banks.ChangeString("libopn.Custombank", "new"));
        Assert.Equal(original, banks.Configuration);
        Assert.Empty(device.Calls);
    }

    private sealed class Device : IVmMusicOpnBankDevice
    {
        public List<string> Calls { get; } = [];
        public int FileResult { get; set; }
        public int DefaultResult { get; set; }
        public byte[]? Data { get; private set; }
        public int OpenBankFile(string filename) { Calls.Add("file:" + filename); return FileResult; }
        public int OpenBankData(ReadOnlyMemory<byte> bank) { Calls.Add("default"); Data = bank.ToArray(); return DefaultResult; }
    }
}
