using System.Buffers.Binary;
using System.Text;

namespace HCDE.MapLoader.Tests;

public static class TestWadBuilder
{
    public static byte[] BuildMinimalMapWad(string mapName, bool udmfTextMap = false, bool includeBehavior = false)
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
            lumps.Add((MapLumpNames.Sidedefs, BuildSidedefLump(0, 0, "-", "-", "STARTAN2", 0)));
            lumps.Add((MapLumpNames.Vertexes, BuildVertexLump(0, 0, 100, 0)));
            lumps.Add((MapLumpNames.Segs, BuildSegLump(0, 1, 0, 0, 0, 0)));
            lumps.Add((MapLumpNames.Ssectors, BuildSubsectorLump(1, 0)));
            lumps.Add((MapLumpNames.Nodes, BuildNodeLump(50, 0, 100, 0, 0, 0, 128, 128, 0, 0)));
            lumps.Add((MapLumpNames.Sectors, BuildSectorLump(0, 128, "FLOOR1_1", "CEIL1_1", 160, 0, 1)));
            lumps.Add((MapLumpNames.Reject, BuildRejectLump(1)));
            lumps.Add((MapLumpNames.Blockmap, BuildBlockmapLump(0, 0, 1, 1)));
            if (includeBehavior)
                lumps.Add((MapLumpNames.Behavior, BuildBehaviorLump(MapBehaviorFormat.AcsOld)));
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

    public static byte[] BuildSidedefLump(
        short textureOffset,
        short rowOffset,
        string topTexture,
        string bottomTexture,
        string midTexture,
        short sector)
    {
        var lump = new byte[MapSidedefRecord.RecordSize];
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(0, 2), textureOffset);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(2, 2), rowOffset);
        WritePicName(lump.AsSpan(4, 8), topTexture);
        WritePicName(lump.AsSpan(12, 8), bottomTexture);
        WritePicName(lump.AsSpan(20, 8), midTexture);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(28, 2), sector);
        return lump;
    }

    public static byte[] BuildSubsectorLump(ushort numSegs, ushort firstSeg)
    {
        var lump = new byte[MapSubsectorRecord.RecordSize];
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(0, 2), numSegs);
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(2, 2), firstSeg);
        return lump;
    }

    public static byte[] BuildRejectLump(int sectorCount)
    {
        var bytes = (sectorCount * sectorCount + 7) / 8;
        return new byte[bytes];
    }

    public static byte[] BuildBlockmapLump(short originX, short originY, ushort width, ushort height)
    {
        // Minimal 1x1 blockmap: header + one offset + terminator list.
        var lump = new byte[12];
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(0, 2), originX);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(2, 2), originY);
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(4, 2), width);
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(6, 2), height);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(8, 2), 5);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(10, 2), -1);
        return lump;
    }

    public static byte[] BuildBehaviorLump(MapBehaviorFormat format)
    {
        var lump = new byte[MapBehaviorRecord.MinLumpSize];
        lump[0] = (byte)'A';
        lump[1] = (byte)'C';
        lump[2] = (byte)'S';
        lump[3] = format switch
        {
            MapBehaviorFormat.AcsOld => 0,
            MapBehaviorFormat.AcsEnhanced => (byte)'E',
            MapBehaviorFormat.AcsLittleEnhanced => (byte)'e',
            _ => byte.MaxValue,
        };
        BinaryPrimitives.WriteUInt32LittleEndian(lump.AsSpan(4, 4), 24);
        return lump;
    }

    public static byte[] BuildVertexLump(params short[] coordinates)
    {
        if (coordinates.Length % 2 != 0)
            throw new ArgumentException("vertex coordinates must be x/y pairs", nameof(coordinates));

        var lump = new byte[(coordinates.Length / 2) * MapVertexRecord.RecordSize];
        var cursor = 0;
        for (var i = 0; i < coordinates.Length; i += 2)
        {
            BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(cursor, 2), coordinates[i]);
            BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(cursor + 2, 2), coordinates[i + 1]);
            cursor += MapVertexRecord.RecordSize;
        }

        return lump;
    }

    public static byte[] BuildSegLump(ushort v1, ushort v2, short angle, ushort linedef, short side, short offset)
    {
        var lump = new byte[MapSegRecord.RecordSize];
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(0, 2), v1);
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(2, 2), v2);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(4, 2), angle);
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(6, 2), linedef);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(8, 2), side);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(10, 2), offset);
        return lump;
    }

    public static byte[] BuildNodeLump(
        short x,
        short y,
        short deltaX,
        short deltaY,
        short bboxLeft,
        short bboxRight,
        short bboxTop,
        short bboxBottom,
        ushort childA,
        ushort childB)
    {
        var lump = new byte[MapNodeRecord.RecordSize];
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(0, 2), x);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(2, 2), y);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(4, 2), deltaX);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(6, 2), deltaY);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(8, 2), bboxLeft);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(10, 2), bboxRight);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(12, 2), bboxTop);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(14, 2), bboxBottom);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(16, 2), bboxLeft);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(18, 2), bboxRight);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(20, 2), bboxTop);
        BinaryPrimitives.WriteInt16LittleEndian(lump.AsSpan(22, 2), bboxBottom);
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(24, 2), childA);
        BinaryPrimitives.WriteUInt16LittleEndian(lump.AsSpan(26, 2), childB);
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
        Assert.Equal(11, entries.Length);
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
        Assert.True(catalog.TryGetLump(MapLumpKind.Sidedefs, out var sidedefs));
        Assert.Equal((uint)MapSidedefRecord.RecordSize, sidedefs.Entry.Size);
        Assert.True(catalog.TryGetLump(MapLumpKind.Ssectors, out var ssectors));
        Assert.Equal((uint)MapSubsectorRecord.RecordSize, ssectors.Entry.Size);
        Assert.True(catalog.TryGetLump(MapLumpKind.Reject, out var reject));
        Assert.Equal(1u, reject.Entry.Size);
        Assert.True(catalog.TryGetLump(MapLumpKind.Blockmap, out var blockmap));
        Assert.Equal(12u, blockmap.Entry.Size);
        Assert.True(catalog.TryGetLump(MapLumpKind.Sectors, out var sectors));
        Assert.Equal((uint)MapSectorRecord.RecordSize, sectors.Entry.Size);
        Assert.True(catalog.TryGetLump(MapLumpKind.Vertexes, out var vertexes));
        Assert.Equal((uint)(MapVertexRecord.RecordSize * 2), vertexes.Entry.Size);
        Assert.True(catalog.TryGetLump(MapLumpKind.Segs, out var segs));
        Assert.Equal((uint)MapSegRecord.RecordSize, segs.Entry.Size);
        Assert.True(catalog.TryGetLump(MapLumpKind.Nodes, out var nodes));
        Assert.Equal((uint)MapNodeRecord.RecordSize, nodes.Entry.Size);
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
