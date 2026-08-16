using System.Buffers.Binary;
using System.Text;

namespace HCDE.MapLoader.Tests;

public static class TestWadBuilder
{
    public static byte[] BuildMinimalMapWad(string mapName, bool udmfTextMap = false)
    {
        var lumps = new List<(string Name, byte[] Data)>
        {
            (mapName, Array.Empty<byte>()),
        };

        if (udmfTextMap)
        {
            lumps.Add((MapLumpNames.Textmap, Encoding.ASCII.GetBytes("namespace = \"Doom\";\n")));
        }
        else
        {
            lumps.Add((MapLumpNames.Things, new byte[4]));
            lumps.Add((MapLumpNames.Linedefs, new byte[8]));
            lumps.Add((MapLumpNames.Sectors, new byte[12]));
        }

        var headerSize = WadArchiveReader.HeaderSize;
        var directorySize = lumps.Count * WadArchiveReader.LumpEntrySize;
        var dataOffset = headerSize;
        var directoryOffset = dataOffset;
        foreach (var lump in lumps)
            dataOffset += lump.Data.Length;
        directoryOffset = dataOffset;

        var wad = new byte[directoryOffset + directorySize];
        BinaryPrimitives.WriteUInt32LittleEndian(wad.AsSpan(0, 4), (uint)WadType.Pwad);
        BinaryPrimitives.WriteUInt32LittleEndian(wad.AsSpan(4, 4), (uint)lumps.Count);
        BinaryPrimitives.WriteUInt32LittleEndian(wad.AsSpan(8, 4), (uint)directoryOffset);

        var fileCursor = headerSize;
        var directoryCursor = directoryOffset;
        foreach (var lump in lumps)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(wad.AsSpan(directoryCursor, 4), (uint)fileCursor);
            BinaryPrimitives.WriteUInt32LittleEndian(wad.AsSpan(directoryCursor + 4, 4), (uint)lump.Data.Length);
            var nameBytes = Encoding.ASCII.GetBytes(lump.Name);
            nameBytes.AsSpan(0, Math.Min(nameBytes.Length, 8)).CopyTo(wad.AsSpan(directoryCursor + 8, 8));

            if (lump.Data.Length > 0)
                lump.Data.CopyTo(wad, fileCursor);

            fileCursor += lump.Data.Length;
            directoryCursor += WadArchiveReader.LumpEntrySize;
        }

        return wad;
    }
}

public class WadArchiveReaderTests
{
    [Fact]
    public void TryReadDirectory_ParsesMinimalPwad()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(WadArchiveReader.TryReadDirectory(wad, out var entries, out _));
        Assert.Equal(4, entries.Length);
        Assert.Equal("MAP01", entries[0].Name);
        Assert.Equal("THINGS", entries[1].Name);
    }
}

public class MapLumpCatalogReaderTests
{
    [Fact]
    public void TryReadMap_ReturnsBinaryLumps()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.Equal(MapDataFormat.DoomBinary, catalog.Format);
        Assert.True(catalog.TryGetLump(MapLumpKind.Things, out var things));
        Assert.Equal(4u, things.Entry.Size);
        Assert.True(catalog.TryGetLump(MapLumpKind.Sectors, out var sectors));
        Assert.Equal(12u, sectors.Entry.Size);
    }

    [Fact]
    public void TryReadMap_DetectsUdmfTextmap()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01", udmfTextMap: true);
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.Equal(MapDataFormat.UdmfText, catalog.Format);
        Assert.True(catalog.TryGetLump(MapLumpKind.Things, out var textmap));
        Assert.True(WadArchiveReader.TryReadLumpData(wad, textmap.Entry, out var data, out _));
        Assert.True(UdmfMapProbe.LooksLikeUdmf(data));
    }
}
