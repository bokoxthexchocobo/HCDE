using System.Buffers.Binary;
using System.Text;

namespace HCDE.MapLoader.Tests;

public class SoundMapFlagsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BinaryFormatsRetainAmbushAndSoundBlocking(bool hexen)
    {
        var bytes = hexen ? HexenLevelTests.HexenWad() : TestWadBuilder.BuildMinimalMapWad("MAP01", false);
        Assert.True(WadArchiveReader.TryReadDirectory(bytes, out var entries, out _));
        var things = entries.Single(entry => entry.Name == "THINGS");
        var lines = entries.Single(entry => entry.Name == "LINEDEFS");
        var options = bytes.AsSpan((int)things.FilePosition + (hexen ? 12 : 8), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(options, (ushort)(BinaryPrimitives.ReadUInt16LittleEndian(options) | 8));
        var flags = bytes.AsSpan((int)lines.FilePosition + 4, 2);
        BinaryPrimitives.WriteUInt16LittleEndian(flags, (ushort)(BinaryPrimitives.ReadUInt16LittleEndian(flags) | LevelLine.BlockSoundFlag));
        Assert.True(LevelBuilder.TryFromWad(bytes, "MAP01", out var level, out var error), error);
        Assert.True(level.Things[0].Ambush);
        Assert.NotEqual(0, level.Lines[0].Flags & LevelLine.BlockSoundFlag);
    }

    [Fact]
    public void UdmfRetainsAmbushAndSoundBlocking()
    {
        var text = """
            namespace = "ZDoom";
            vertex { x = 0; y = 0; } vertex { x = 0; y = 128; }
            sector { heightceiling = 128; } sidedef { sector = 0; }
            linedef { v1 = 0; v2 = 1; sidefront = 0; blocksound = true; }
            thing { type = 3001; ambush = true; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(Encoding.UTF8.GetBytes(text), out var map, out var error), error);
        var level = LevelBuilder.FromUdmf(map, "MAP01");
        Assert.True(level.Things[0].Ambush);
        Assert.NotEqual(0, level.Lines[0].Flags & LevelLine.BlockSoundFlag);
    }
}
