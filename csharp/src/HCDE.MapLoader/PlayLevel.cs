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
    public double FloorHeight { get; init; }
    public double CeilingHeight { get; init; }
    public short LightLevel { get; init; }
    public short Special { get; set; }
    public int DamageAmount { get; set; }
    public string DamageType { get; set; } = "None";
    public int DamageInterval { get; set; }
    public int Leakiness { get; set; }
    public bool DamageEndsGodMode { get; set; }
    public bool DamageEndsLevel { get; set; }
    public bool HurtMonsters { get; init; }
    public bool HarmInAir { get; init; }
    public short Tag { get; init; }
    public string FloorPic { get; set; } = "-";
    public string CeilingPic { get; init; } = "-";
    internal LevelSector Copy() => (LevelSector)MemberwiseClone();
}

public sealed class LevelSide
{
    public double MidTextureOffsetY { get; init; }
    public int Index { get; init; }
    public int Sector { get; init; }
    public string TopTexture { get; init; } = "-";
    public string BottomTexture { get; init; } = "-";
    public string MidTexture { get; init; } = "-";
}

public sealed class LevelLine
{
    public const int BlockingFlag = 1;
    public const int TwoSidedFlag = 4;
    public const int BlockSoundFlag = 0x40;
    public const int BlockEverythingFlag = 0x00008000;
    public const int BlockHitscanFlag = 0x08000000;
    public const int BlockSightFlag = 0x04000000;
    public const int BlockProjectileFlag = 0x01000000;
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
    public int Arg3 { get; set; }
    public int Arg4 { get; init; }
    public bool PlayerCross { get; init; }
    public bool PlayerUse { get; init; }
    public bool UseThrough { get; init; }
    public bool Repeat { get; init; }

    public bool OneSided => SideBack < 0;

    public bool BlocksMovement => OneSided || (Flags & (BlockingFlag | BlockEverythingFlag)) != 0;

    public bool RepeatsSpecial => Special == 1 || (Flags & RepeatSpecialFlag) != 0;

    internal LevelLine Copy() => (LevelLine)MemberwiseClone();
}

public sealed class LevelThing
{
    public int Index { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public double Angle { get; init; }
    public int Type { get; init; }
    public short Options { get; init; }
    public bool Ambush { get; init; }
    public int Id { get; init; }
    public int Special { get; init; }
    public int[] Args { get; init; } = new int[5];
    public int SkillMask { get; init; } = 31;
    public bool Single { get; init; } = true;
    public bool Coop { get; init; } = true;
    public bool Deathmatch { get; init; } = true;
}

public sealed class PlayLevel
{
    public string MapName { get; init; } = "";
    public MapDataFormat Format { get; init; }
    public string Namespace { get; init; } = "";
    public bool UsesDoomSectorNumbers => Format == MapDataFormat.DoomBinary
        || Format == MapDataFormat.UdmfText && (Namespace.Equals("Doom", StringComparison.OrdinalIgnoreCase)
            || Namespace.Equals("ZDoomTranslated", StringComparison.OrdinalIgnoreCase));
    public ReadOnlyMemory<byte> BehaviorData { get; internal set; }
    public IReadOnlyList<LevelVertex> Vertices { get; init; } = Array.Empty<LevelVertex>();
    public IReadOnlyList<LevelSector> Sectors { get; init; } = Array.Empty<LevelSector>();
    public IReadOnlyList<LevelSide> Sides { get; init; } = Array.Empty<LevelSide>();
    public IReadOnlyList<LevelLine> Lines { get; init; } = Array.Empty<LevelLine>();
    public IReadOnlyList<LevelThing> Things { get; init; } = Array.Empty<LevelThing>();

    /// <summary>
    /// Decoded BLOCKMAP for legacy broad-phase helpers. Authority cylinder physics
    /// scans geometry directly so malformed block lists cannot bypass collision.
    /// </summary>
    public MapBlockmapRecord? Blockmap { get; set; }

    /// <summary>Copies runtime line and sector state while sharing other map data.</summary>
    public PlayLevel CopyForSimulation() => new()
    {
        MapName = MapName, Format = Format, Namespace = Namespace, BehaviorData = BehaviorData,
        Vertices = Vertices, Sectors = Sectors.Select(sector => sector.Copy()).ToArray(), Sides = Sides, Things = Things, Blockmap = Blockmap,
        Lines = Lines.Select(line => line.Copy()).ToList(),
    };
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

            if (!WadArchiveReader.TryReadLumpData(wad, textmap.Entry, out var bytes, out error))
                return false;
            if (!UdmfTextMapParser.TryParse(bytes, out var udmf, out error))
                return false;

            if (!new[] { "Doom", "ZDoom", "ZDoomTranslated" }.Contains(udmf.Namespace, StringComparer.OrdinalIgnoreCase))
            {
                error = "unsupported-udmf-namespace";
                return false;
            }
            if (udmf.Sectors.Any(sector => !FitsShort(sector.LightLevel) || !FitsShort(sector.Special) || !FitsShort(sector.Id)))
            {
                error = "unsupported-udmf-sector-range-or-fraction";
                return false;
            }

            level = FromUdmf(udmf, mapName);
            return LevelValidation.TryValidate(level, out error);
        }

        if (catalog.TryGetLump(MapLumpKind.Behavior, out _))
        {
            return HexenLevelDecoder.TryDecode(wad, catalog, out level, out error);
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

        return LevelValidation.TryValidate(level, out error);
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
            MidTextureOffsetY = side.RowOffset,
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
            Ambush = (thing.Options & 8) != 0,
            SkillMask = ((thing.Options & 1) != 0 ? 3 : 0) | ((thing.Options & 2) != 0 ? 4 : 0) | ((thing.Options & 4) != 0 ? 24 : 0),
            Single = (thing.Options & 16) == 0,
            Coop = (thing.Options & 64) == 0,
            Deathmatch = (thing.Options & 32) == 0,
        }).ToArray();

        return new PlayLevel
        {
            MapName = mapName,
            Format = MapDataFormat.DoomBinary,
            Vertices = vertices,
            Sectors = sectors,
            Sides = sides,
            Lines = lines,
            Things = things,
        };
    }

    public static PlayLevel FromUdmf(UdmfTextMap map, string mapName, MapDataFormat format = MapDataFormat.UdmfText)
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
            FloorHeight = sector.HeightFloor,
            CeilingHeight = sector.HeightCeiling,
            LightLevel = (short)sector.LightLevel,
            Special = (short)sector.Special,
            DamageAmount = sector.DamageAmount,
            DamageType = sector.DamageAmount == 0 ? "None" : sector.DamageType,
            // Native sector fields narrow to signed 16 bits before interval normalization.
            DamageInterval = sector.DamageAmount == 0 ? 0 : Math.Max(1, (int)unchecked((short)sector.DamageInterval)),
            Leakiness = sector.DamageAmount == 0 ? 0 : unchecked((short)sector.Leakiness),
            HurtMonsters = sector.HurtMonsters,
            HarmInAir = sector.HarmInAir,
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
            MidTextureOffsetY = side.OffsetY + (
                string.Equals(map.Namespace, "ZDoom", StringComparison.OrdinalIgnoreCase)
                || string.Equals(map.Namespace, "ZDoomTranslated", StringComparison.OrdinalIgnoreCase)
                || string.Equals(map.Namespace, "Vavoom", StringComparison.OrdinalIgnoreCase)
                    ? side.OffsetYMid : 0),
        }).ToArray();
        var lines = new LevelLine[map.Linedefs.Count];
        for (var i = 0; i < lines.Length; i++)
        {
            var source = map.Linedefs[i];
            var v1 = Vertex(vertices, source.V1);
            var v2 = Vertex(vertices, source.V2);
            var flags = 0;
            if (source.TwoSided) flags |= LevelLine.TwoSidedFlag;
            if (source.Blocking)
                flags |= LevelLine.BlockingFlag;
            if (source.BlockEverything) flags |= LevelLine.BlockEverythingFlag;
            if (source.BlockSight) flags |= LevelLine.BlockSightFlag;
            if (source.BlockHitscan) flags |= LevelLine.BlockHitscanFlag;
            if (source.BlockProjectiles) flags |= LevelLine.BlockProjectileFlag;
            if (source.BlockSound) flags |= LevelLine.BlockSoundFlag;
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
                PlayerCross = source.PlayerCross,
                PlayerUse = source.PlayerUse,
                UseThrough = source.PassUse,
                Repeat = source.RepeatSpecial,
            };
        }

        var things = map.Things.Select((thing, index) => new LevelThing
        {
            Index = index,
            X = thing.X,
            Y = thing.Y,
            Z = thing.Height,
            Angle = thing.Angle,
            Type = thing.Type,
            Id = thing.Id,
            Ambush = thing.Ambush,
            Special = thing.Special,
            Args = new[] { thing.Arg0, thing.Arg1, thing.Arg2, thing.Arg3, thing.Arg4 },
            SkillMask = (thing.Skill1 ? 1 : 0) | (thing.Skill2 ? 2 : 0) | (thing.Skill3 ? 4 : 0) | (thing.Skill4 ? 8 : 0) | (thing.Skill5 ? 16 : 0),
            Single = thing.Single, Coop = thing.Coop, Deathmatch = thing.Dm,
            Options = 0,
        }).ToArray();

        return new PlayLevel
        {
            MapName = mapName,
            Format = format,
            Namespace = map.Namespace,
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

    private static bool FitsShort(double value) => double.IsFinite(value) && value >= short.MinValue
        && value <= short.MaxValue && value == Math.Truncate(value);
}
