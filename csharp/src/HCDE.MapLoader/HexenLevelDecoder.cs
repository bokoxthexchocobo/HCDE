using System.Buffers.Binary;

namespace HCDE.MapLoader;

/// <summary>Hexen's 20-byte things and 16-byte lines, translated to the managed level model.</summary>
public static class HexenLevelDecoder
{
    public static bool TryDecode(ReadOnlySpan<byte> wad, MapLumpCatalog catalog, out PlayLevel level, out string? error, bool hexenHack = false)
    {
        level = new PlayLevel(); error = null;
        if (!Read(wad, catalog, MapLumpKind.Things, out var things, out error)
            || !Read(wad, catalog, MapLumpKind.Linedefs, out var lines, out error)
            || !Read(wad, catalog, MapLumpKind.Vertexes, out var vertices, out error)
            || !Read(wad, catalog, MapLumpKind.Sidedefs, out var sides, out error)
            || !Read(wad, catalog, MapLumpKind.Sectors, out var sectors, out error)
            || !Read(wad, catalog, MapLumpKind.Behavior, out var behavior, out error)) return false;
        if (things.Length % 20 != 0 || lines.Length % 16 != 0)
        { error = "hexen-record-size-invalid"; return false; }
        if (!MapVertexCodec.TryReadAll(vertices, out var vertexRecords, out error)
            || !MapSidedefCodec.TryReadAll(sides, out var sideRecords, out error)
            || !MapSectorCodec.TryReadAll(sectors, out var sectorRecords, out error)) return false;
        var thingRecords = new List<UdmfThing>();
        for (var offset = 0; offset < things.Length; offset += 20)
        {
            var data = things.Slice(offset, 20);
            var flags = U16(data, 12);
            if (hexenHack) flags &= 0x7ff;
            thingRecords.Add(new UdmfThing {
                Id = U16(data, 0), X = I16(data, 2), Y = I16(data, 4), Height = I16(data, 6),
                Angle = U16(data, 8), Type = U16(data, 10), Special = data[14],
                Arg0 = data[15], Arg1 = data[16], Arg2 = data[17], Arg3 = data[18], Arg4 = data[19],
                Skill1 = (flags & 1) != 0, Skill2 = (flags & 1) != 0, Skill3 = (flags & 2) != 0,
                Skill4 = (flags & 4) != 0, Skill5 = (flags & 4) != 0,
                Ambush = (flags & 8) != 0,
                Dormant = (flags & 16) != 0,
                Friend = (flags & 0x2000) != 0,
                Single = (flags & 256) != 0, Coop = (flags & 512) != 0, Dm = (flags & 1024) != 0,
            });
        }
        var lineRecords = new List<UdmfLinedef>();
        for (var offset = 0; offset < lines.Length; offset += 16)
        {
            var data = lines.Slice(offset, 16);
            var flags = U16(data, 4); var activation = (flags >> 10) & 7;
            lineRecords.Add(new UdmfLinedef {
                V1 = U16(data, 0), V2 = U16(data, 2), Blocking = (flags & 1) != 0,
                SideFront = Side(U16(data, 12)), SideBack = Side(U16(data, 14)), Special = data[6],
                Arg0 = data[7], Arg1 = data[8], Arg2 = data[9], Arg3 = data[10], Arg4 = data[11],
                // Encoded 7 is projectile touch, not the internal AnyCross value.
                PlayerCross = activation == 0, PlayerUse = activation is 1 or 6, PassUse = activation == 6,
                RepeatSpecial = (flags & 512) != 0,
                BlockSound = (flags & LevelLine.BlockSoundFlag) != 0,
            });
        }
        var map = new UdmfTextMap {
            Namespace = "ZDoom", Things = thingRecords, Linedefs = lineRecords,
            Vertices = vertexRecords.Select(vertex => new UdmfVertex { X = vertex.X, Y = vertex.Y }).ToArray(),
            Sidedefs = sideRecords.Select(side => new UdmfSidedef { Sector = side.Sector,
                TextureTop = side.TopTexture, TextureBottom = side.BottomTexture, TextureMiddle = side.MidTexture }).ToArray(),
            Sectors = sectorRecords.Select(sector => new UdmfSector { HeightFloor = sector.FloorHeight,
                HeightCeiling = sector.CeilingHeight, LightLevel = sector.LightLevel, Special = sector.Special,
                Id = sector.Tag, TextureFloor = sector.FloorPic, TextureCeiling = sector.CeilingPic }).ToArray(),
        };
        level = LevelBuilder.FromUdmf(map, catalog.MapName, MapDataFormat.HexenBinary);
        level.HexenHack = hexenHack;
        level.BehaviorData = behavior.ToArray();
        level.HasBehavior = true;
        return LevelValidation.TryValidate(level, out error);
    }

    private static bool Read(ReadOnlySpan<byte> wad, MapLumpCatalog catalog, MapLumpKind kind, out ReadOnlySpan<byte> data, out string? error)
    {
        data = default; error = "hexen-missing-map-lump";
        return catalog.TryGetLump(kind, out var lump) && WadArchiveReader.TryReadLumpData(wad, lump.Entry, out data, out error);
    }
    private static ushort U16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
    private static short I16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadInt16LittleEndian(data[offset..]);
    private static int Side(ushort value) => value == ushort.MaxValue ? -1 : value;
}
