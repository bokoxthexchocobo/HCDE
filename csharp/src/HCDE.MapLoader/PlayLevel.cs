namespace HCDE.MapLoader;

public sealed class LevelVertex
{
    public int Index { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
}

public sealed class LevelSector
{
    public int Index { get; init; }
    public short FloorHeight { get; init; }
    public short CeilingHeight { get; init; }
    public short LightLevel { get; init; }
    public short Special { get; init; }
    public short Tag { get; init; }
    public string FloorPic { get; init; } = "-";
    public string CeilingPic { get; init; } = "-";
}

public sealed class LevelSide
{
    public int Index { get; init; }
    public int Sector { get; init; }
    public string TopTexture { get; init; } = "-";
    public string BottomTexture { get; init; } = "-";
    public string MidTexture { get; init; } = "-";
}

public sealed class LevelLine
{
    public const int BlockingFlag = 1;
    public const int RepeatSpecialFlag = 512;
    public const int NoSide = -1;

    public int Index { get; init; }
    public int V1 { get; init; }
    public int V2 { get; init; }
    public double X1 { get; init; }
    public double Y1 { get; init; }
    public double X2 { get; init; }
    public double Y2 { get; init; }
    public int Flags { get; init; }
    public int SideFront { get; init; }
    public int SideBack { get; init; }
    public int Special { get; set; }
    public int Tag { get; init; }
    public int Arg0 { get; init; }
    public int Arg1 { get; init; }
    public int Arg2 { get; init; }
    public int Arg3 { get; init; }
    public int Arg4 { get; init; }

    public bool OneSided => SideBack < 0;

    public bool BlocksMovement => OneSided || (Flags & BlockingFlag) != 0;

    public bool RepeatsSpecial => Special == 1 || (Flags & RepeatSpecialFlag) != 0;
}

public sealed class LevelThing
{
    public int Index { get; init; }
    public short X { get; init; }
    public short Y { get; init; }
    public short Angle { get; init; }
    public short Type { get; init; }
    public short Options { get; init; }
}

public sealed class PlayLevel
{
    public string MapName { get; init; } = "";
    public IReadOnlyList<LevelVertex> Vertices { get; init; } = Array.Empty<LevelVertex>();
    public IReadOnlyList<LevelSector> Sectors { get; init; } = Array.Empty<LevelSector>();
    public IReadOnlyList<LevelSide> Sides { get; init; } = Array.Empty<LevelSide>();
    public IReadOnlyList<LevelLine> Lines { get; init; } = Array.Empty<LevelLine>();
    public IReadOnlyList<LevelThing> Things { get; init; } = Array.Empty<LevelThing>();

    /// <summary>
    /// Decoded BLOCKMAP. When set, movement tests only the lines listed in the blocks the actor crosses.
    /// </summary>
    public MapBlockmapRecord? Blockmap { get; set; }
}

/// <summary>
/// Builds a playable level from a decoded binary map or a UDMF text map.
/// One-sided lines use side index -1, matching a Doom <c>0xFFFF</c> sidedef.
/// </summary>
public static class LevelBuilder
{
    public static bool TryFromWad(ReadOnlySpan<byte> wad, string mapName, out PlayLevel level, out string? error)
    {
        level = new PlayLevel { MapName = mapName };
        if (!MapLumpCatalogReader.TryReadMap(wad, mapName, out var catalog, out error))
            return false;

        if (catalog.Format == MapDataFormat.UdmfText)
        {
            // The catalog stores TEXTMAP under the Things kind. There is no separate Textmap kind.
            if (!catalog.TryGetLump(MapLumpKind.Things, out var textmap))
            {
                error = "map-missing-textmap";
                return false;
            }

            var bytes = wad.Slice((int)textmap.Entry.FilePosition, (int)textmap.Entry.Size);
            if (!UdmfTextMapParser.TryParse(bytes, out var udmf, out error))
                return false;

            level = FromUdmf(udmf, mapName);
            return true;
        }

        if (!BinaryMapDecoder.TryReadMap(wad, mapName, out var binary, out _, out error))
            return false;

        level = FromBinary(binary, mapName);
        if (catalog.TryGetLump(MapLumpKind.Blockmap, out var blockmapLump)
            && WadArchiveReader.TryReadLumpData(wad, blockmapLump.Entry, out var blockmapData, out _)
            && MapBlockmapCodec.TryRead(blockmapData, out var blockmap, out _))
        {
            level.Blockmap = blockmap;
        }

        return true;
    }

    public static PlayLevel FromBinary(BinaryMap map, string mapName)
    {
        var vertices = map.Geometry.Vertices.Select((vertex, index) => new LevelVertex
        {
            Index = index,
            X = vertex.X,
            Y = vertex.Y,
        }).ToArray();
        var sectors = map.Core.Sectors.Select((sector, index) => new LevelSector
        {
            Index = index,
            FloorHeight = sector.FloorHeight,
            CeilingHeight = sector.CeilingHeight,
            LightLevel = sector.LightLevel,
            Special = sector.Special,
            Tag = sector.Tag,
            FloorPic = sector.FloorPic,
            CeilingPic = sector.CeilingPic,
        }).ToArray();
        var sides = map.Surface.Sidedefs.Select((side, index) => new LevelSide
        {
            Index = index,
            Sector = side.Sector,
            TopTexture = side.TopTexture,
            BottomTexture = side.BottomTexture,
            MidTexture = side.MidTexture,
        }).ToArray();
        var lines = new LevelLine[map.Core.Linedefs.Length];
        for (var i = 0; i < lines.Length; i++)
        {
            var source = map.Core.Linedefs[i];
            var v1 = Vertex(vertices, source.V1);
            var v2 = Vertex(vertices, source.V2);
            lines[i] = new LevelLine
            {
                Index = i,
                V1 = source.V1,
                V2 = source.V2,
                X1 = v1.X,
                Y1 = v1.Y,
                X2 = v2.X,
                Y2 = v2.Y,
                Flags = source.Flags,
                SideFront = SideIndex(source.SideFront),
                SideBack = SideIndex(source.SideBack),
                Special = source.Special,
                Tag = source.Tag,
            };
        }

        var things = map.Core.Things.Select((thing, index) => new LevelThing
        {
            Index = index,
            X = thing.X,
            Y = thing.Y,
            Angle = thing.Angle,
            Type = thing.Type,
            Options = thing.Options,
        }).ToArray();

        return new PlayLevel
        {
            MapName = mapName,
            Vertices = vertices,
            Sectors = sectors,
            Sides = sides,
            Lines = lines,
            Things = things,
        };
    }

    public static PlayLevel FromUdmf(UdmfTextMap map, string mapName)
    {
        var vertices = map.Vertices.Select((vertex, index) => new LevelVertex
        {
            Index = index,
            X = vertex.X,
            Y = vertex.Y,
        }).ToArray();
        var sectors = map.Sectors.Select((sector, index) => new LevelSector
        {
            Index = index,
            FloorHeight = (short)sector.HeightFloor,
            CeilingHeight = (short)sector.HeightCeiling,
            LightLevel = (short)sector.LightLevel,
            Special = (short)sector.Special,
            Tag = (short)sector.Id,
            FloorPic = sector.TextureFloor,
            CeilingPic = sector.TextureCeiling,
        }).ToArray();
        var sides = map.Sidedefs.Select((side, index) => new LevelSide
        {
            Index = index,
            Sector = side.Sector,
            TopTexture = side.TextureTop,
            BottomTexture = side.TextureBottom,
            MidTexture = side.TextureMiddle,
        }).ToArray();
        var lines = new LevelLine[map.Linedefs.Count];
        for (var i = 0; i < lines.Length; i++)
        {
            var source = map.Linedefs[i];
            var v1 = Vertex(vertices, source.V1);
            var v2 = Vertex(vertices, source.V2);
            var flags = 0;
            if (source.Blocking)
                flags |= LevelLine.BlockingFlag;
            lines[i] = new LevelLine
            {
                Index = i,
                V1 = source.V1,
                V2 = source.V2,
                X1 = v1.X,
                Y1 = v1.Y,
                X2 = v2.X,
                Y2 = v2.Y,
                Flags = flags,
                SideFront = source.SideFront,
                SideBack = source.SideBack,
                Special = source.Special,
                Tag = source.Id,
                Arg0 = source.Arg0,
                Arg1 = source.Arg1,
                Arg2 = source.Arg2,
                Arg3 = source.Arg3,
                Arg4 = source.Arg4,
            };
        }

        var things = map.Things.Select((thing, index) => new LevelThing
        {
            Index = index,
            X = (short)thing.X,
            Y = (short)thing.Y,
            Angle = (short)thing.Angle,
            Type = (short)thing.Type,
            Options = 0,
        }).ToArray();

        return new PlayLevel
        {
            MapName = mapName,
            Vertices = vertices,
            Sectors = sectors,
            Sides = sides,
            Lines = lines,
            Things = things,
        };
    }

    private static LevelVertex Vertex(IReadOnlyList<LevelVertex> vertices, int index) =>
        index >= 0 && index < vertices.Count ? vertices[index] : new LevelVertex();

    private static int SideIndex(ushort side) => side == ushort.MaxValue ? LevelLine.NoSide : side;
}
