namespace HCDE.Client.Tests;

public class VmMusicAdlBankSelectionTests
{
    private static VmMusicStartupData Resources(byte[]? gen = null) => VmMusicStartupData.Load(
        name => name == "GENMIDI" ? gen : null, _ => null, _ => { });

    private static byte[] Lump()
    {
        var lump = new byte[8 + 175 * 36];
        "#OPL_II#"u8.CopyTo(lump);
        lump[8 + 2] = 128;
        return lump;
    }

    [Fact]
    public void ResourceSnapshotFeedsConvertedGenMidiBeforeCustomFile()
    {
        var lump = Lump();
        var expected = VmMusicGenMidiBank.FromLump(lump);
        var resources = Resources(lump);
        Array.Clear(lump);
        var device = new Device();
        Assert.Equal(VmMusicAdlBankKind.GenMidi,
            VmMusicAdlBankSelection.Initialize(new(true, true, "custom.wopl", 9), resources, device));
        Assert.Equal(new[] { "data" }, device.Calls);
        Assert.Equal(expected, device.Bank);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void CustomFileLoadsWhenGenMidiIsDisabledOrAbsent(bool useGenMidi, bool present)
    {
        var device = new Device();
        Assert.Equal(VmMusicAdlBankKind.CustomFile, VmMusicAdlBankSelection.Initialize(
            new(true, useGenMidi, "custom.wopl", 9), Resources(present ? Lump() : null), device));
        Assert.Equal(new[] { "file:custom.wopl" }, device.Calls);
    }

    [Theory]
    [InlineData(0, VmMusicAdlBankKind.CustomFile)]
    [InlineData(3, VmMusicAdlBankKind.Builtin)]
    public void RejectedGenMidiFallsBackToFileThenBuiltin(int fileResult, VmMusicAdlBankKind expected)
    {
        var device = new Device { DataResult = 2, FileResult = fileResult };
        Assert.Equal(expected, VmMusicAdlBankSelection.Initialize(new(true, true, "custom.wopl", 9), Resources(Lump()), device));
        Assert.Equal(fileResult == 0 ? new[] { "data", "file:custom.wopl" } : new[] { "data", "file:custom.wopl", "builtin:9" }, device.Calls);
    }

    [Theory]
    [InlineData(false, "custom.wopl")]
    [InlineData(true, "")]
    public void DisabledCustomBanksOrEmptyFilenameUseBuiltin(bool enabled, string filename)
    {
        var device = new Device();
        Assert.Equal(VmMusicAdlBankKind.Builtin, VmMusicAdlBankSelection.Initialize(
            new(enabled, false, filename, 9), Resources(Lump()), device));
        Assert.Equal(new[] { "builtin:9" }, device.Calls);
    }

    [Fact]
    public void EnabledGenMidiDoesNotOverrideDisabledCustomBanks()
    {
        var device = new Device();
        Assert.Equal(VmMusicAdlBankKind.Builtin,
            VmMusicAdlBankSelection.Initialize(new(false, true, "custom.wopl", 9), Resources(Lump()), device));
        Assert.Equal(new[] { "builtin:9" }, device.Calls);
    }

    [Fact]
    public void GenMidiCanLoadWithoutCustomFilenameAndEachAttemptOwnsConvertedData()
    {
        var resources = Resources(Lump());
        var device = new Device();
        Assert.Equal(VmMusicAdlBankKind.GenMidi, VmMusicAdlBankSelection.Initialize(new(true, true, "", 9), resources, device));
        var first = device.Bank!;
        Array.Clear(first);
        Assert.Equal(VmMusicAdlBankKind.GenMidi, VmMusicAdlBankSelection.Initialize(new(true, true, "", 9), resources, device));
        Assert.NotEqual(first, device.Bank);
        Assert.Equal(VmMusicGenMidiBank.FromLump(Lump()), device.Bank);
    }

    [Theory]
    [InlineData("data")]
    [InlineData("file")]
    [InlineData("builtin")]
    public void AdapterExceptionsPropagateWithoutFallback(string step)
    {
        var device = new Device { ThrowOn = step, DataResult = 2, FileResult = 2 };
        var error = Assert.Throws<InvalidOperationException>(() => VmMusicAdlBankSelection.Initialize(
            new(true, true, "custom.wopl", 9), Resources(Lump()), device));
        Assert.Equal(step, error.Message);
        Assert.Equal(step == "data" ? 1 : step == "file" ? 2 : 3, device.Calls.Count);
    }

    private sealed class Device : IVmMusicAdlBankDevice
    {
        public List<string> Calls { get; } = [];
        public byte[]? Bank { get; private set; }
        public int DataResult { get; init; }
        public int FileResult { get; init; }
        public string? ThrowOn { get; init; }
        public int OpenBankData(ReadOnlyMemory<byte> bank)
        {
            Calls.Add("data");
            if (ThrowOn == "data") throw new InvalidOperationException("data");
            Bank = bank.ToArray();
            return DataResult;
        }
        public int OpenBankFile(string filename)
        {
            Calls.Add("file:" + filename);
            if (ThrowOn == "file") throw new InvalidOperationException("file");
            return FileResult;
        }
        public void SetBuiltinBank(int bank)
        {
            Calls.Add("builtin:" + bank);
            if (ThrowOn == "builtin") throw new InvalidOperationException("builtin");
        }
    }
}
