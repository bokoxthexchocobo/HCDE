using System.Buffers.Binary;
using System.Text;

namespace HCDE.MapLoader.Tests;

public class MapsModsTests
{
    public static byte[] Wad(params (string Name, byte[] Data)[] lumps)
    {
        var directory = 12 + lumps.Sum(lump => lump.Data.Length);
        var bytes = new byte[directory + 16 * lumps.Length];
        "PWAD"u8.CopyTo(bytes);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), lumps.Length);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8), directory);
        var cursor = 12;
        foreach (var lump in lumps)
        {
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(directory), cursor);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(directory + 4), lump.Data.Length);
            Encoding.ASCII.GetBytes(lump.Name).CopyTo(bytes, directory + 8);
            lump.Data.CopyTo(bytes, cursor);
            cursor += lump.Data.Length;
            directory += 16;
        }
        return bytes;
    }

    [Theory]
    [InlineData(268435456u, 12u)]
    [InlineData(1u, 4294967290u)]
    public void OverflowingDirectoryIsRejected(uint count, uint offset)
    {
        var bytes = Wad();
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), count);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(8), offset);
        Assert.False(WadArchiveReader.TryReadDirectory(bytes, out _, out _));
    }

    [Fact]
    public void OverflowingLumpAndTextmapAreRejectedWithoutThrowing()
    {
        var bytes = Wad(("MAP01", []), ("TEXTMAP", []));
        Assert.False(WadArchiveReader.TryReadLumpData(bytes, new WadLumpEntry(uint.MaxValue, 8, "BAD"), out _, out _));
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(28), uint.MaxValue);
        Assert.False(LevelBuilder.TryFromWad(bytes, "MAP01", out _, out _));
    }

    [Fact]
    public void LaterMapOverridesEntireEarlierGroupAndPreservesOtherResources()
    {
        var first = TestWadBuilder.BuildMinimalMapWad("MAP01");
        var second = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(WadArchiveReader.TryReadDirectory(second, out var entries, out _));
        var things = entries.Single(entry => entry.Name == "THINGS");
        BinaryPrimitives.WriteInt16LittleEndian(second.AsSpan((int)things.FilePosition), 321);
        Assert.True(WadLoadOrder.TryMerge([first, Wad(("CUSTOM", [1, 2, 3])), second], out var merged, out var error), error);
        Assert.True(LevelBuilder.TryFromWad(merged, "map01", out var level, out error), error);
        Assert.Equal(321, Assert.Single(level.Things).X);
        Assert.True(WadArchiveReader.TryReadDirectory(merged, out entries, out _));
        Assert.Contains(entries, entry => entry.Name == "CUSTOM");
        Assert.True(WadLoadOrder.TryMerge([first, Wad(("MAP01", []))], out merged, out error), error);
        Assert.False(LevelBuilder.TryFromWad(merged, "MAP01", out _, out _));
    }

    [Fact]
    public void InvalidGeometryAndHexenBinaryFailBeforePlayback()
    {
        var bytes = TestWadBuilder.BuildMinimalMapWad("MAP01");
        WadArchiveReader.TryReadDirectory(bytes, out var entries, out _);
        var line = entries.Single(entry => entry.Name == "LINEDEFS");
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan((int)line.FilePosition), 1000);
        Assert.False(LevelBuilder.TryFromWad(bytes, "MAP01", out _, out var error));
        Assert.Equal("map-line-vertex-invalid", error);
        Assert.False(LevelBuilder.TryFromWad(TestWadBuilder.BuildMinimalMapWad("MAP01", includeBehavior: true), "MAP01", out _, out error));
        Assert.Equal("hexen-record-size-invalid", error);
    }

    [Theory]
    [InlineData("sector { heightfloor = 32768; }")]
    [InlineData("sector { heightfloor = 0.5; }")]
    [InlineData("thing { type = 999999999999; }")]
    [InlineData("linedef { v1 = 0.5; }")]
    [InlineData("vertex { x = 999999999; }")]
    public void UnsupportedNumericValuesAreRejected(string body)
    {
        var bytes = Wad(("MAP01", []), ("TEXTMAP", Encoding.UTF8.GetBytes("namespace = \"ZDoom\"; " + body)));
        Assert.False(LevelBuilder.TryFromWad(bytes, "MAP01", out _, out _));
    }

    [Fact]
    public void UdmfRetainsFractionalPositionsIdsAndSpawnAndActivationFlags()
    {
        Assert.True(UdmfTextMapParser.TryParse("""
            namespace = "ZDoom";
            thing { x=1.25; y=2.5; height=3.75; angle=270.5; type=3004; id=42; skill3=true; coop=true; }
            linedef { playeruse=true; playeruseback=true; playercross=false; repeatspecial=true; arg0=9; }
            """, out var map, out var error), error);
        var level = LevelBuilder.FromUdmf(map, "MAP01");
        var thing = Assert.Single(level.Things);
        Assert.Equal((1.25, 2.5, 3.75, 270.5, 42, 4), (thing.X, thing.Y, thing.Z, thing.Angle, thing.Id, thing.SkillMask));
        Assert.True(thing.Coop);
        Assert.False(thing.Single);
        Assert.True(level.Lines[0].PlayerUse);
        Assert.True(level.Lines[0].PlayerUseBack);
        Assert.True(level.Lines[0].Repeat);
        Assert.False(level.Lines[0].PlayerCross);
    }
}
