namespace HCDE.Client.Tests;

public class VmMusicStartupDataTests
{
    [Fact]
    public void ValidGenMidiStripsHeaderAndPreferredGusAndOpnAreSnapshotted()
    {
        var gen = new byte[8 + 175 * 36];
        "#OPL_II#"u8.CopyTo(gen);
        gen[8] = 42;
        byte[] gus = [17];
        var requests = new List<string>();
        var data = VmMusicStartupData.Load(name =>
        {
            requests.Add(name);
            return name switch { "GENMIDI" => gen, "DMXGUSC" => gus, _ => throw new InvalidOperationException() };
        }, name => { Assert.Equal("xg.wopn", name); return [255]; });
        gen[8] = 0;
        gus[0] = 0;
        var order = new List<string>();
        data.Apply(bytes => { order.Add("gen"); Assert.Equal(6300, bytes.Length); Assert.Equal(42, bytes.Span[0]); },
            bytes => { order.Add("gus"); Assert.Equal(17, bytes.Span[0]); },
            bytes => { order.Add("opn"); Assert.Equal(255, bytes.Span[0]); });
        Assert.Equal(new[] { "gen", "gus", "opn" }, order);
        Assert.DoesNotContain("DMXGUS", requests);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrInvalidGenMidiSkipsSetupAndGusFallsBack(bool invalid)
    {
        var warnings = new List<string>();
        var data = VmMusicStartupData.Load(name => name switch
        {
            "GENMIDI" => invalid ? new byte[6308] : null,
            "DMXGUSC" => null,
            "DMXGUS" => [9],
            _ => null,
        }, _ => null, warnings.Add);
        data.Apply(_ => throw new InvalidOperationException(), bytes => Assert.Equal(9, bytes.Span[0]),
            _ => throw new InvalidOperationException());
        Assert.Equal(invalid ? 0 : 1, warnings.Count);
    }
}
