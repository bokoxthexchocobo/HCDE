namespace HCDE.Client.Tests;

public class VmMusicOpnBankSelectionTests
{
    private static VmMusicStartupData Resources(byte[]? bank) => VmMusicStartupData.Load(_ => null,
        name => { Assert.Equal("xg.wopn", name); return bank; }, _ => { });

    [Fact]
    public void SuccessfulCustomFileTakesPrecedenceOverResourceAndEmbeddedBanks()
    {
        var device = new Device();
        Assert.Equal(VmMusicOpnBankKind.CustomFile,
            VmMusicOpnBankSelection.Initialize(new(true, "custom.wopn"), Resources([17]), device));
        Assert.Equal(new[] { "file:custom.wopn" }, device.Calls);
        Assert.Null(device.Bank);
    }

    [Theory]
    [InlineData(false, "custom.wopn", 0)]
    [InlineData(true, "", 0)]
    [InlineData(true, "custom.wopn", 2)]
    public void DefaultResourceLoadsWhenCustomFileIsDisabledEmptyOrRejected(bool enabled, string filename, int fileResult)
    {
        byte[] bank = [17, 42];
        var resources = Resources(bank);
        Array.Clear(bank);
        var device = new Device { FileResult = fileResult };
        Assert.Equal(VmMusicOpnBankKind.ResourceDefault,
            VmMusicOpnBankSelection.Initialize(new(enabled, filename), resources, device));
        Assert.Equal(new byte[] { 17, 42 }, device.Bank);
        Assert.Equal(enabled && filename.Length != 0 ? new[] { "file:custom.wopn", "data" } : new[] { "data" }, device.Calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrEmptyResourceLoadsBundledXgBank(bool empty)
    {
        var device = new Device();
        Assert.Equal(VmMusicOpnBankKind.EmbeddedDefault,
            VmMusicOpnBankSelection.Initialize(new(false, ""), Resources(empty ? [] : null), device));
        Assert.Equal(new[] { "data" }, device.Calls);
        Assert.Equal(186205, device.Bank!.Length);
        Assert.Equal(0, device.Bank[^1]);
        Assert.Equal(VmMusicBankFormat.Wopn, VmMusicBankCatalog.Identify(device.Bank));
        Assert.Equal(Convert.FromHexString("574F504E322D42324E4B000200000A000B09"), device.Bank[..18]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DefaultBankRejectionReportsFailureWithoutUnrelatedFallback(bool embedded)
    {
        var device = new Device { FileResult = 2, DataResult = 7 };
        var error = Assert.Throws<InvalidOperationException>(() => VmMusicOpnBankSelection.Initialize(
            new(true, "custom.wopn"), Resources(embedded ? null : [17]), device));
        Assert.Equal("Loading OPN default bank failed: 7.", error.Message);
        Assert.Equal(new[] { "file:custom.wopn", "data" }, device.Calls);
    }

    [Theory]
    [InlineData("file")]
    [InlineData("data")]
    public void AdapterExceptionsPropagate(string step)
    {
        var device = new Device { FileResult = 2, ThrowOn = step };
        var error = Assert.Throws<InvalidOperationException>(() => VmMusicOpnBankSelection.Initialize(
            new(true, "custom.wopn"), Resources([17]), device));
        Assert.Equal(step, error.Message);
        Assert.Equal(step == "file" ? 1 : 2, device.Calls.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EachDefaultLoadOwnsBytesAndDoesNotChangeResourceOrEmbeddedBank(bool embedded)
    {
        var resources = Resources(embedded ? null : [17, 42]);
        var first = new Device();
        var second = new Device();
        VmMusicOpnBankSelection.Initialize(new(false, ""), resources, first);
        var firstBank = Assert.IsType<byte[]>(first.Bank);
        var expected = firstBank.ToArray();
        Array.Clear(firstBank);
        VmMusicOpnBankSelection.Initialize(new(false, ""), resources, second);
        Assert.Equal(expected, second.Bank);
    }

    private sealed class Device : IVmMusicOpnBankDevice
    {
        public List<string> Calls { get; } = [];
        public byte[]? Bank { get; private set; }
        public int FileResult { get; init; }
        public int DataResult { get; init; }
        public string? ThrowOn { get; init; }
        public int OpenBankFile(string filename)
        {
            Calls.Add("file:" + filename);
            if (ThrowOn == "file") throw new InvalidOperationException("file");
            return FileResult;
        }
        public int OpenBankData(ReadOnlyMemory<byte> bank)
        {
            Calls.Add("data");
            if (ThrowOn == "data") throw new InvalidOperationException("data");
            System.Runtime.InteropServices.MemoryMarshal.TryGetArray(bank, out var segment);
            Bank = segment.Array ?? bank.ToArray();
            return DataResult;
        }
    }
}
