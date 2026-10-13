namespace HCDE.Client.Tests;

public class VmReplayGainPersistenceFormatTests
{
    [Theory]
    [InlineData(0, "0.000000")]
    [InlineData(int.MinValue, "-0.000000")]
    [InlineData(0x7f800000, "inf")]
    [InlineData(unchecked((int)0xff800000), "-inf")]
    [InlineData(0x7fc00000, "nan")]
    [InlineData(unchecked((int)0xffc00000), "-nan(ind)")]
    [InlineData(unchecked((int)0xffc00001), "-nan")]
    public void MatchesWindowsRuntimeSpecialValuesAndRoundTrips(int bits, string expected)
    {
        var value = BitConverter.Int32BitsToSingle(bits);
        Assert.Equal(expected, VmMusicNumberFormat.Persistence(value, windows: true));
        var parsed = Assert.Single(VmReplayGainIni.Parse("[Gains]\nsong=" + expected)).Value;
        if (float.IsNaN(value)) Assert.True(float.IsNaN(parsed));
        else Assert.Equal(value, parsed);
    }

    [Fact]
    public void NonWindowsNanUsesPortableSpelling()
    {
        Assert.Equal("-nan", VmMusicNumberFormat.Persistence(BitConverter.Int32BitsToSingle(unchecked((int)0xffc00000)), false));
    }

    [Fact]
    public void CacheUsesPersistenceRatherThanStatusFormatter()
    {
        var cache = new VmReplayGainCache();
        cache.Store("song", float.PositiveInfinity);
        cache.Save(entries => Assert.Equal("inf", Assert.Single(entries).Value));
        cache.Store("song", 1.2345678f);
        cache.Save(entries => Assert.Equal("1.234568", Assert.Single(entries).Value));
    }
}
