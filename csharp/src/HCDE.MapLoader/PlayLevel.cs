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
    public int Tag { get; init; }
    public IReadOnlyList<int> AdditionalTags { get; init; } = Array.Empty<int>();
    public bool HasTag(int tag) => tag != 0 && (Tag == tag || AdditionalTags.Contains(tag));
    public bool MatchesTag(int tag) => tag == 0 ? Tag == 0 && AdditionalTags.Count == 0 : HasTag(tag);
    public double Gravity { get; set; } = 1;
    public int HealthFloor { get; set; }
    public int HealthCeiling { get; set; }
    public int Health3D { get; set; }
    public int HealthFloorGroup { get; set; }
    public int HealthCeilingGroup { get; set; }
    public int Health3DGroup { get; set; }
    public string FloorPic { get; set; } = "-";
    /// <summary>Native floor texture scroll offset (visual only).</summary>
    public double FloorTextureOffsetX { get; set; }
    public double FloorTextureOffsetY { get; set; }
    public double FloorTextureScaleX { get; set; } = 1;
    public double FloorTextureScaleY { get; set; } = 1;
    /// <summary>Native floor plane texture rotation in BAM units.</summary>
    public uint FloorTextureAngle { get; set; }
    public uint FloorTextureBaseAngle { get; set; }
    public double FloorTextureBaseOffsetY { get; set; }
    public string CeilingPic { get; init; } = "-";
    public double CeilingTextureOffsetX { get; set; }
    public double CeilingTextureOffsetY { get; set; }
    public double CeilingTextureScaleX { get; set; } = 1;
    public double CeilingTextureScaleY { get; set; } = 1;
    public uint CeilingTextureAngle { get; set; }
    public uint CeilingTextureBaseAngle { get; set; }
    public double CeilingTextureBaseOffsetY { get; set; }
    /// <summary>ACS <c>SetSectorTerrain</c> floor override. Footstep audio is absent.</summary>
    public string FloorTerrain { get; set; } = string.Empty;
    /// <summary>ACS <c>SetSectorTerrain</c> ceiling override.</summary>
    public string CeilingTerrain { get; set; } = string.Empty;
    internal LevelSector Copy() => (LevelSector)MemberwiseClone();
}

public sealed class LevelSide
{
    public double TopTextureScaleX { get; set; } = 1;
    public double TopTextureScaleY { get; set; } = 1;
    public double MidTextureScaleX { get; set; } = 1;
    public double MidTextureScaleY { get; set; } = 1;
    public double BottomTextureScaleX { get; set; } = 1;
    public double BottomTextureScaleY { get; set; } = 1;
    public double MidTextureOffsetY { get; set; }
    public double MidTextureOffsetX { get; set; }
    public double TopTextureOffsetX { get; set; }
    public double TopTextureOffsetY { get; set; }
    public double BottomTextureOffsetX { get; set; }
    public double BottomTextureOffsetY { get; set; }
    public int Index { get; init; }
    public int Sector { get; init; }
    public string TopTexture { get; init; } = "-";
    public string BottomTexture { get; init; } = "-";
    public string MidTexture { get; init; } = "-";
    /// <summary>UDMF map-load wall scroll rates (parts mask matches <c>WallScrollParts</c>).</summary>
    public List<(double Dx, double Dy, int PartsMask)> MapLoadWallScrolls { get; private set; } = [];

    internal LevelSide Copy()
    {
        var copy = (LevelSide)MemberwiseClone();
        copy.MapLoadWallScrolls = new List<(double Dx, double Dy, int PartsMask)>(MapLoadWallScrolls);
        return copy;
    }
}

public sealed class LevelLine
{
    public IReadOnlyList<int> AdditionalIds { get; init; } = Array.Empty<int>();
    public bool HasId(int id) => id != -1 && (Tag == id || AdditionalIds.Contains(id));
    public const int BlockingFlag = 1;
    public const int BlockMonstersFlag = 2;
    public const int BlockFloatersFlag = 0x00040000;
    public const int TwoSidedFlag = 4;
    public const int BlockSoundFlag = 0x40;
    public const int BlockEverythingFlag = 0x00008000;
    public const int BlockHitscanFlag = 0x08000000;
    public const int BlockSightFlag = 0x04000000;
    public const int BlockProjectileFlag = 0x01000000;
    public const int RepeatSpecialFlag = 512;
    public const int MidTex3DFlag = 0x00200000;
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
    public bool PlayerCross { get; set; }
    public bool PlayerUse { get; set; }
    /// <summary>Native SPAC_UseBack. A use trace may activate this line from its back side.</summary>
    public bool PlayerUseBack { get; set; }
    public bool UseThrough { get; set; }
    public bool Repeat { get; set; }
    public int Health { get; set; }
    public int HealthGroup { get; set; }

    public bool OneSided => SideBack < 0;

    public bool BlocksMovement => OneSided || (Flags & (BlockingFlag | BlockEverythingFlag)) != 0;

    public bool RepeatsSpecial => Special == 1 || (Flags & RepeatSpecialFlag) != 0;

    internal LevelLine Copy() => (LevelLine)MemberwiseClone();
}

public sealed class LevelThing
{
    public double Health { get; init; } = 1;
    public short Pitch { get; init; }
    public short Roll { get; init; }
    public double Gravity { get; init; } = 1;
    public int Index { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public double Angle { get; init; }
    public int Type { get; init; }
    public short Options { get; init; }
    public bool Ambush { get; init; }
    public bool Dormant { get; init; }
    public bool Friendly { get; init; }
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
    public bool HexenHack { get; set; }
    public bool ActivateOwnDeathSpecials { get; set; }
    public IReadOnlyList<HCDE.Gamedata.MapInfoDamageType> DamageTypes { get; set; } = Array.Empty<HCDE.Gamedata.MapInfoDamageType>();
    public string MapName { get; init; } = "";
    public MapDataFormat Format { get; init; }
    public string Namespace { get; init; } = "";
    public bool UsesDoomSectorNumbers => Format == MapDataFormat.DoomBinary
        || Format == MapDataFormat.UdmfText && (Namespace.Equals("Doom", StringComparison.OrdinalIgnoreCase)
            || Namespace.Equals("ZDoomTranslated", StringComparison.OrdinalIgnoreCase));
    public ReadOnlyMemory<byte> BehaviorData { get; internal set; }
    /// <summary>True when the map supplied a BEHAVIOR lump, including an empty or unsupported one.</summary>
    public bool HasBehavior { get; internal set; }
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

    /// <summary>Copies runtime lines, sectors and sides while sharing immutable map data.</summary>
    public PlayLevel CopyForSimulation() => new()
    {
        MapName = MapName, Format = Format, Namespace = Namespace, BehaviorData = BehaviorData, HasBehavior = HasBehavior, HexenHack = HexenHack,
        ActivateOwnDeathSpecials = ActivateOwnDeathSpecials,
        DamageTypes = DamageTypes.ToArray(),
        Vertices = Vertices, Sectors = Sectors.Select(sector => sector.Copy()).ToArray(),
        Sides = Sides.Select(side => side.Copy()).ToArray(), Things = Things, Blockmap = Blockmap,
        Lines = Lines.Select(line => line.Copy()).ToList(),
    };
}

public enum BinaryThingFlagFormat { Doom, Strife }

/// <summary>
/// Builds a playable level from a decoded binary map or a UDMF text map.
/// One-sided lines use side index -1, matching a Doom <c>0xFFFF</c> sidedef.
/// </summary>
public static class LevelBuilder
{
    public static bool TryFromWad(ReadOnlySpan<byte> wad, string mapName, out PlayLevel level, out string? error,
        BinaryThingFlagFormat thingFlagFormat = BinaryThingFlagFormat.Doom)
    {
        level = new PlayLevel { MapName = mapName };
        if (!Enum.IsDefined(thingFlagFormat))
        {
            error = "unsupported-binary-thing-flag-format";
            return false;
        }
        if (!MapLumpCatalogReader.TryReadMap(wad, mapName, out var catalog, out error))
            return false;
        if (!TryReadDeathSpecialPolicy(wad, mapName, out var hexenHack, out var ownDeathSpecials, out var damageTypes, out error)) return false;

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
            level.DamageTypes = damageTypes;
            level.ActivateOwnDeathSpecials = ownDeathSpecials;
            return LevelValidation.TryValidate(level, out error);
        }

        if (catalog.TryGetLump(MapLumpKind.Behavior, out _))
        {
            if (!HexenLevelDecoder.TryDecode(wad, catalog, out level, out error, hexenHack)) return false;
            level.DamageTypes = damageTypes;
            level.ActivateOwnDeathSpecials = ownDeathSpecials;
            return true;
        }
        if (!BinaryMapDecoder.TryReadMap(wad, mapName, out var binary, out _, out error))
            return false;

        level = FromBinary(binary, mapName, thingFlagFormat);
        level.DamageTypes = damageTypes;
        level.ActivateOwnDeathSpecials = ownDeathSpecials;
        if (catalog.TryGetLump(MapLumpKind.Blockmap, out var blockmapLump)
            && WadArchiveReader.TryReadLumpData(wad, blockmapLump.Entry, out var blockmapData, out _)
            && MapBlockmapCodec.TryRead(blockmapData, out var blockmap, out _))
        {
            level.Blockmap = blockmap;
        }

        return LevelValidation.TryValidate(level, out error);
    }

    private static bool TryReadDeathSpecialPolicy(ReadOnlySpan<byte> wad, string mapName, out bool hexenHack, out bool ownDeathSpecials, out IReadOnlyList<HCDE.Gamedata.MapInfoDamageType> damageTypes, out string? error)
    {
        hexenHack = false;
        ownDeathSpecials = false;
        damageTypes = Array.Empty<HCDE.Gamedata.MapInfoDamageType>();
        var definitions = new Dictionary<string, HCDE.Gamedata.MapInfoDamageType>(StringComparer.OrdinalIgnoreCase);
        if (!WadArchiveReader.TryReadDirectory(wad, out var entries, out error)) return false;
        // ZMAPINFO replaces MAPINFO within this archive; later map definitions win.
        var lumpName = entries.Any(entry => entry.Name.Equals("ZMAPINFO", StringComparison.OrdinalIgnoreCase))
            ? "ZMAPINFO" : "MAPINFO";
        foreach (var entry in entries.Where(entry => entry.Name.Equals(lumpName, StringComparison.OrdinalIgnoreCase)))
        {
            if (!WadArchiveReader.TryReadLumpData(wad, entry, out var data, out error)) return false;
            if (!HCDE.Gamedata.MapInfoParser.TryParse(System.Text.Encoding.UTF8.GetString(data), out var info, out error)) return false;
            foreach (var definition in info.DamageTypes) definitions[definition.Name] = definition;
            if (info.FindMap(mapName) is { } map)
            {
                hexenHack = map.HexenHack;
                ownDeathSpecials = map.ActivateOwnDeathSpecials;
            }
        }
        damageTypes = definitions.Values.ToArray();
        return true;
    }

    public static PlayLevel FromBinary(BinaryMap map, string mapName,
        BinaryThingFlagFormat thingFlagFormat = BinaryThingFlagFormat.Doom)
    {
        if (!Enum.IsDefined(thingFlagFormat)) throw new ArgumentOutOfRangeException(nameof(thingFlagFormat));
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
            MidTextureOffsetX = side.TextureOffset,
            TopTextureOffsetX = side.TextureOffset,
            TopTextureOffsetY = side.RowOffset,
            BottomTextureOffsetX = side.TextureOffset,
            BottomTextureOffsetY = side.RowOffset,
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
            Ambush = (thing.Options & (thingFlagFormat == BinaryThingFlagFormat.Strife ? 0x20 : 8)) != 0,
            // Native Doom loading discards extension flags from bad-editor records.
            Friendly = thingFlagFormat == BinaryThingFlagFormat.Strife ? (thing.Options & 0x40) != 0
                : (thing.Options & 0x180) == 0x80,
            SkillMask = ((thing.Options & 1) != 0 ? 3 : 0) | ((thing.Options & 2) != 0 ? 4 : 0) | ((thing.Options & 4) != 0 ? 24 : 0),
            // Native Doom loading leaves MTF_SINGLE set even with BTF_NOTSINGLE.
            Single = true,
            Coop = thingFlagFormat == BinaryThingFlagFormat.Strife || (thing.Options & 0x140) != 0x40,
            Deathmatch = thingFlagFormat == BinaryThingFlagFormat.Strife || (thing.Options & 0x120) != 0x20,
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

    private static double WallScale(double scale) => scale == 0 ? 1 : scale;

    public static uint TextureAngleFromDegrees(double degrees)
    {
        var normalized = degrees % 360.0;
        if (normalized < 0)
            normalized += 360.0;
        return (uint)(normalized * (4294967296.0 / 360.0));
    }

    public static PlayLevel FromUdmf(UdmfTextMap map, string mapName, MapDataFormat format = MapDataFormat.UdmfText)
    {
        var planeTransforms = map.Namespace.Equals("ZDoom", StringComparison.OrdinalIgnoreCase)
            || map.Namespace.Equals("ZDoomTranslated", StringComparison.OrdinalIgnoreCase)
            || map.Namespace.Equals("Vavoom", StringComparison.OrdinalIgnoreCase);
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
            Gravity = planeTransforms ? sector.Gravity : 1,
            Tag = sector.Id,
            AdditionalTags = planeTransforms ? MoreIds(sector.MoreIds, sector.Id, sector: true) : Array.Empty<int>(),
            HealthFloor = sector.HealthFloor,
            HealthCeiling = sector.HealthCeiling,
            Health3D = sector.Health3D,
            HealthFloorGroup = sector.HealthFloorGroup,
            HealthCeilingGroup = sector.HealthCeilingGroup,
            Health3DGroup = sector.Health3DGroup,
            FloorPic = sector.TextureFloor,
            CeilingPic = sector.TextureCeiling,
            FloorTextureAngle = planeTransforms ? TextureAngleFromDegrees(sector.RotationFloor) : 0,
            CeilingTextureAngle = planeTransforms ? TextureAngleFromDegrees(sector.RotationCeiling) : 0,
            FloorTextureOffsetX = planeTransforms ? sector.XPanningFloor : 0,
            FloorTextureOffsetY = planeTransforms ? sector.YPanningFloor : 0,
            CeilingTextureOffsetX = planeTransforms ? sector.XPanningCeiling : 0,
            CeilingTextureOffsetY = planeTransforms ? sector.YPanningCeiling : 0,
            FloorTextureScaleX = planeTransforms ? sector.XScaleFloor : 1,
            FloorTextureScaleY = planeTransforms ? sector.YScaleFloor : 1,
            CeilingTextureScaleX = planeTransforms ? sector.XScaleCeiling : 1,
            CeilingTextureScaleY = planeTransforms ? sector.YScaleCeiling : 1,
        }).ToArray();
        var sides = map.Sidedefs.Select((side, index) =>
        {
            var built = new LevelSide
            {
                Index = index,
                Sector = side.Sector,
                TopTexture = side.TextureTop,
                BottomTexture = side.TextureBottom,
                MidTexture = side.TextureMiddle,
                TopTextureScaleX = planeTransforms ? WallScale(side.ScaleXTop) : 1,
                TopTextureScaleY = planeTransforms ? WallScale(side.ScaleYTop) : 1,
                MidTextureScaleX = planeTransforms ? WallScale(side.ScaleXMid) : 1,
                MidTextureScaleY = planeTransforms ? WallScale(side.ScaleYMid) : 1,
                BottomTextureScaleX = planeTransforms ? WallScale(side.ScaleXBottom) : 1,
                BottomTextureScaleY = planeTransforms ? WallScale(side.ScaleYBottom) : 1,
                TopTextureOffsetX = (planeTransforms ? side.OffsetXTop : 0) + side.OffsetX,
                TopTextureOffsetY = (planeTransforms ? side.OffsetYTop : 0) + side.OffsetY,
                MidTextureOffsetX = (planeTransforms ? side.OffsetXMid : 0) + side.OffsetX,
                MidTextureOffsetY = side.OffsetY + (
                    string.Equals(map.Namespace, "ZDoom", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(map.Namespace, "ZDoomTranslated", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(map.Namespace, "Vavoom", StringComparison.OrdinalIgnoreCase)
                        ? side.OffsetYMid : 0),
                BottomTextureOffsetX = (planeTransforms ? side.OffsetXBottom : 0) + side.OffsetX,
                BottomTextureOffsetY = (planeTransforms ? side.OffsetYBottom : 0) + side.OffsetY,
            };
            foreach (var scroll in BuildUdmfWallScrolls(side))
                built.MapLoadWallScrolls.Add(scroll);
            return built;
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
            if (source.BlockMonsters) flags |= LevelLine.BlockMonstersFlag;
            if (source.BlockFloaters) flags |= LevelLine.BlockFloatersFlag;
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
                AdditionalIds = planeTransforms ? MoreIds(source.MoreIds, source.Id, sector: false) : Array.Empty<int>(),
                Arg0 = source.Arg0,
                Arg1 = source.Arg1,
                Arg2 = source.Arg2,
                Arg3 = source.Arg3,
                Arg4 = source.Arg4,
                PlayerCross = source.PlayerCross,
                PlayerUse = source.PlayerUse,
                PlayerUseBack = source.PlayerUseBack,
                UseThrough = source.PassUse,
                Repeat = source.RepeatSpecial,
                Health = source.Health,
                HealthGroup = source.HealthGroup,
            };
        }

        var things = map.Things.Select((thing, index) => new LevelThing
        {
            Pitch = unchecked((short)thing.Pitch),
            Roll = unchecked((short)thing.Roll),
            Gravity = map.Namespace.Equals("ZDoom", StringComparison.OrdinalIgnoreCase)
                || map.Namespace.Equals("ZDoomTranslated", StringComparison.OrdinalIgnoreCase) ? thing.Gravity : 1,
            Index = index,
            X = thing.X,
            Y = thing.Y,
            Z = thing.Height,
            Health = planeTransforms ? thing.Health : 1,
            Angle = thing.Angle,
            Type = thing.Type,
            Id = thing.Id,
            Ambush = thing.Ambush,
            Dormant = thing.Dormant && (planeTransforms || map.Namespace.Equals("Hexen", StringComparison.OrdinalIgnoreCase)),
            Friendly = planeTransforms ? (thing.StrifeAllySpecifiedLast ? thing.StrifeAlly : thing.Friend)
                : map.Namespace.Equals("Doom", StringComparison.OrdinalIgnoreCase) ? thing.Friend
                : map.Namespace.Equals("Strife", StringComparison.OrdinalIgnoreCase) && thing.StrifeAlly,
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

    private static IReadOnlyList<int> MoreIds(string text, int primary, bool sector)
        => UdmfAdditionalIds.Parse(text, primary, sector);

    private static IEnumerable<(double Dx, double Dy, int PartsMask)> BuildUdmfWallScrolls(UdmfSidedef side)
    {
        const int allParts = 1 | 2 | 4;
        var scrolls = new (double X, double Y)[]
        {
            (side.XScroll, side.YScroll),
            (side.XScrollTop, side.YScrollTop),
            (side.XScrollMid, side.YScrollMid),
            (side.XScrollBottom, side.YScrollBottom),
        };
        var mask = allParts;
        for (var i = 1; i < 4; i++)
        {
            var (x, y) = scrolls[i];
            if (x == 0 && y == 0)
                continue;
            mask &= ~(1 << (i - 1));
            yield return (x, y, 1 << (i - 1));
        }

        var (gx, gy) = scrolls[0];
        if ((gx != 0 || gy != 0) && mask != 0)
            yield return (gx, gy, mask);
    }
}
