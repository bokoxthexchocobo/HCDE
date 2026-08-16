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
            lumps.Add((MapLumpNames.Things, BuildThingLump(100, 200, 90, 1, 7)));
            lumps.Add((MapLumpNames.Linedefs, BuildLinedefLump(0, 1, 0, 0, 0, 0, 0xFFFF)));
            lumps.Add((MapLumpNames.Sectors, BuildSectorLump(0, 128, "FLOOR1_1", "CEIL1_1", 160, 0, 1)));
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

    public static byte[] BuildThingLump(short x, short y, short angle, short type, short options)
    {
        var lump = new byte[MapThingRecord.RecordSize];
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(0, 2), x);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(2, 2), y);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(4, 2), angle);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(6, 2), type);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(8, 2), options);
        return lump;
    }

    public static byte[] BuildLinedefLump(ushort v1, ushort v2, ushort flags, ushort special, short tag, ushort sideFront, ushort sideBack)
    {
        var lump = new byte[MapLinedefRecord.RecordSize];
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(0, 2), v1);
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(2, 2), v2);
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(4, 2), flags);
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(6, 2), special);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(8, 2), tag);
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(10, 2), sideFront);
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(12, 2), sideBack);
        return lump;
    }

    public static byte[] BuildSectorLump(
        short floorHeight,
        short ceilingHeight,
        string floorPic,
        string ceilingPic,
        short lightLevel,
        short special,
        short tag)
    {
        var lump = new byte[MapSectorRecord.RecordSize];
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(0, 2), floorHeight);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(2, 2), ceilingHeight);
        WritePicName(lump.AsSpan(4, 8), floorPic);
        WritePicName(lump.AsSpan(12, 8), ceilingPic);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(20, 2), lightLevel);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(22, 2), special);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(24, 2), tag);
        return lump;
    }

    private static void WritePicName(Span<byte> destination, string name)
    {
        var bytes = Encoding.ASCII.GetBytes(name);
        bytes.AsSpan(0, Math.Min(bytes.Length, 8)).CopyTo(destination);
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
        Assert.Equal((uint)MapThingRecord.RecordSize, things.Entry.Size);
        Assert.True(catalog.TryGetLump(MapLumpKind.Sectors, out var sectors));
        Assert.Equal((uint)MapSectorRecord.RecordSize, sectors.Entry.Size);
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
