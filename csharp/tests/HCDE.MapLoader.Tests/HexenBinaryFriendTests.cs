using System.Buffers.Binary;

namespace HCDE.MapLoader.Tests;

public class HexenBinaryFriendTests
{
    [Theory]
    [InlineData("map 1 \"Original\" { }", false)]
    [InlineData("map MAP01 \"Extended\" { }", true)]
    [InlineData("map 2 \"Other\" { }", true)]
    [InlineData("map 1 \"Original\" { } map MAP01 \"Extended\" { }", true)]
    [InlineData("map MAP01 \"Extended\" { } map 1 \"Original\" { }", false)]
    public void MapInfoCompatibilityMasksExtendedThingFlags(string text, bool expected)
    {
        var wad = WithMapInfo(("MAPINFO", text));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        var thing = Assert.Single(level.Things);
        Assert.Equal(expected, thing.Friendly);
        Assert.True(thing.Dormant);
        Assert.True(thing.Ambush);
        Assert.True(thing.Single);
        Assert.Equal(42, thing.Id);
    }

    [Fact]
    public void ZmapInfoOverridesMapInfoCompatibility()
    {
        var wad = WithMapInfo(("MAPINFO", "map 1 \"Original\" { }"),
            ("ZMAPINFO", "map MAP01 \"Extended\" { }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.True(Assert.Single(level.Things).Friendly);
    }

    [Fact]
    public void LaterMapInfoLumpsOverrideEarlierDefinition()
    {
        var wad = WithMapInfo(("MAPINFO", "map MAP01 \"Extended\" { }"),
            ("MAPINFO", "map 1 \"Original\" { }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.False(Assert.Single(level.Things).Friendly);
    }

    [Fact]
    public void InvalidMapInfoReportsFailure()
    {
        Assert.False(LevelBuilder.TryFromWad(WithMapInfo(("MAPINFO", "map")), "MAP01", out _, out var error));
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    private static byte[] WithMapInfo(params (string Name, string Text)[] info)
    {
        var wad = HexenLevelTests.HexenWad();
        Assert.True(WadArchiveReader.TryReadDirectory(wad, out var entries, out _));
        var things = entries.Single(entry => entry.Name == "THINGS");
        BinaryPrimitives.WriteUInt16LittleEndian(wad.AsSpan((int)things.FilePosition + 12), 0x211A);
        var lumps = entries.Select(entry => (entry.Name,
            wad.AsSpan((int)entry.FilePosition, (int)entry.Size).ToArray())).ToList();
        lumps.AddRange(info.Select(item => (item.Name, System.Text.Encoding.UTF8.GetBytes(item.Text))));
        return MapsModsTests.Wad(lumps.ToArray());
    }

    [Fact]
    public void DamageDefinitionsLoadAndLaterLumpsReplaceThem()
    {
        var wad = WithMapInfo(("MAPINFO", "DamageType Acid { NoArmor Factor = 2 }"),
            ("MAPINFO", "DamageType acid { Factor = 0.5 } DamageType Fire { NoArmor }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        var acid = level.DamageTypes.Single(d => d.Name.Equals("acid", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0.5, acid.Factor); Assert.False(acid.NoArmor);
        Assert.Equal(2, level.CopyForSimulation().DamageTypes.Count);
    }

    [Fact]
    public void ZmapInfoDamageDefinitionsReplaceMapInfoSource()
    {
        var wad = WithMapInfo(("MAPINFO", "DamageType Acid { NoArmor }"),
            ("ZMAPINFO", "DamageType Fire { Factor = 0 }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal("Fire", Assert.Single(level.DamageTypes).Name);
    }

    [Theory]
    [InlineData(0x102, false, false, false)]
    [InlineData(0x2102, true, false, false)]
    [InlineData(0x2182, true, false, false)]
    [InlineData(0x2112, true, true, false)]
    [InlineData(0x210A, true, false, true)]
    [InlineData(0x211A, true, true, true)]
    [InlineData(0xA102, true, false, false)]
    [InlineData(0x8102, false, false, false)]
    public void HexenFriendlyBitIsIndependentOfClassDormantAndAmbush(int flags, bool friendly, bool dormant, bool ambush)
    {
        var wad = HexenLevelTests.HexenWad();
        Assert.True(WadArchiveReader.TryReadDirectory(wad, out var entries, out var directoryError), directoryError);
        var things = entries.Single(entry => entry.Name == "THINGS");
        BinaryPrimitives.WriteUInt16LittleEndian(wad.AsSpan((int)things.FilePosition + 12), (ushort)flags);
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(MapDataFormat.HexenBinary, level.Format);
        var thing = Assert.Single(level.Things);
        Assert.Equal(friendly, thing.Friendly);
        Assert.Equal(dormant, thing.Dormant);
        Assert.Equal(ambush, thing.Ambush);
        Assert.True(thing.Single);
        Assert.Equal(4, thing.SkillMask);
        Assert.Equal(42, thing.Id);
        Assert.Equal(80, thing.Special);
    }
}
