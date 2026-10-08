using System.Buffers.Binary;

namespace HCDE.MapLoader.Tests;

public class SoundWadResourceTests
{
    [Fact]
    public void SoundCurveUsesLastCaseInsensitiveLumpAndOwnsBytes()
    {
        var wad = MapsModsTests.Wad(("SNDCURVE", [1]), ("sndcurve", [127, 64]));
        Assert.True(SoundResourceResolver.TryReadSoundCurve(wad, out var curve, out var error), error);
        Assert.Equal(new byte[] { 127, 64 }, curve);
        Array.Clear(wad); Assert.Equal(new byte[] { 127, 64 }, curve);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AbsentOrEmptySoundCurveAllowsDoomFallback(bool include)
    {
        var wad = include ? MapsModsTests.Wad(("SNDCURVE", [])) : MapsModsTests.Wad(("OTHER", [1]));
        Assert.True(SoundResourceResolver.TryReadSoundCurve(wad, out var curve, out var error), error);
        Assert.Empty(curve);
    }

    [Fact]
    public void InvalidSoundCurveLumpFails()
    {
        var wad = MapsModsTests.Wad(("SNDCURVE", [1]));
        var directory = BinaryPrimitives.ReadInt32LittleEndian(wad.AsSpan(8));
        BinaryPrimitives.WriteUInt32LittleEndian(wad.AsSpan(directory), uint.MaxValue);
        Assert.False(SoundResourceResolver.TryReadSoundCurve(wad, out var curve, out var error));
        Assert.Empty(curve); Assert.NotNull(error);
    }

    [Fact]
    public void ResourceLookupIncludesIndexZeroAndLaterOverrides()
    {
        var level = Level();
        Assert.True(SoundResourceResolver.TryReadWadResource(MapsModsTests.Wad(("DSPISTOL", [1])), level, "alias", out var bytes, out var found, out var error), error);
        Assert.True(found); Assert.Equal(new byte[] { 1 }, bytes.ToArray());
        var wad = MapsModsTests.Wad(("DSPISTOL", [1]), ("DSPISTOL", [2, 3]));
        Assert.True(SoundResourceResolver.TryReadWadResource(wad, level, "alias", out bytes, out found, out error), error);
        Assert.True(found); Assert.Equal(new byte[] { 2, 3 }, bytes.ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingAndEmptyLumpsRemainDistinct(bool include)
    {
        var wad = include ? MapsModsTests.Wad(("DSPISTOL", [])) : MapsModsTests.Wad(("OTHER", [1]));
        Assert.True(SoundResourceResolver.TryReadWadResource(wad, Level(), "alias", out var bytes, out var found, out var error), error);
        Assert.Equal(include, found); Assert.True(bytes.IsEmpty);
    }

    [Fact]
    public void InvalidSelectedLumpFailsExplicitly()
    {
        var wad = MapsModsTests.Wad(("DSPISTOL", [1]));
        var directory = BinaryPrimitives.ReadInt32LittleEndian(wad.AsSpan(8));
        BinaryPrimitives.WriteUInt32LittleEndian(wad.AsSpan(directory), uint.MaxValue);
        Assert.False(SoundResourceResolver.TryReadWadResource(wad, Level(), "alias", out _, out var found, out var error));
        Assert.False(found); Assert.NotNull(error);
    }

    [Fact]
    public void RandomSelectionReadsChosenResource()
    {
        var level = Level();
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["group"] = new[] { "missing", "alias" } };
        Assert.True(SoundResourceResolver.TryReadWadResource(MapsModsTests.Wad(("DSPISTOL", [7])), level, "group", out var bytes, out var found, out var error, () => 1), error);
        Assert.True(found); Assert.Equal(new byte[] { 7 }, bytes.ToArray());
    }

    [Fact]
    public void FullResourcePathsFailRatherThanTruncating()
    {
        var level = new PlayLevel { SoundDefinitions = new Dictionary<string, string> { ["sound"] = "sounds/test.wav" } };
        Assert.False(SoundResourceResolver.TryReadWadResource([], level, "sound", out _, out _, out var error));
        Assert.StartsWith("sound-resource-path-unsupported", error);
    }

    private static PlayLevel Level() => new()
    {
        SoundAliases = new Dictionary<string, string> { ["alias"] = "sound" },
        SoundDefinitions = new Dictionary<string, string> { ["sound"] = "dspistol" },
    };
}
