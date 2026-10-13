namespace HCDE.Client.Tests;

public class VmSf2MusicReaderTests
{
    [Fact]
    public void GeneratedConfigurationIsQuotedUtf8AndReturnedIndependently()
    {
        var reader = new VmSf2MusicReader("banks/caf\u00e9.sf2");
        var first = reader.Open(null);
        Assert.Equal("soundfont \"banks/caf\u00e9.sf2\"\n", System.Text.Encoding.UTF8.GetString(first.Data!));
        first.Data![0] = 0;
        Assert.Equal((byte)'s', reader.Open(null).Data![0]);
        Assert.Equal("timidity.cfg", first.Name);
        Assert.Null(reader.Open("unrelated.sf2").Data);
    }

    [Fact]
    public void NamedBankReadsBinaryPayloadAndMissingBankErrorsPropagate()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".sf2");
        var reader = new VmSf2MusicReader(path);
        Assert.Throws<FileNotFoundException>(() => reader.Open(path));
        try
        {
            File.WriteAllBytes(path, [0, 255, 17]);
            Assert.Equal(new byte[] { 0, 255, 17 }, reader.Open(path).Data);
            Assert.Equal(path, reader.Open(path).Name);
        }
        finally { File.Delete(path); }
    }
}
