namespace HCDE.MapLoader;

/// <summary>
/// <c>FloodZone</c>, <c>FloodZones</c>, <c>LoadExtendedNodes</c>, and <c>P_CheckV4Nodes</c> from <c>maploader.cpp</c>.
/// </summary>
public static class VmMapLoaderExtendedNodes
{
    public const ushort UnassignedZone = 0xffff;

    public static bool ShouldSkipFloodLine(bool hasSecondSide, bool zoneBoundary, bool frontIsSource, bool backIsSource) =>
        !hasSecondSide || zoneBoundary;

    public static int NextZoneIndex(IReadOnlyList<ushort> zoneNumbers)
    {
        var z = 0;
        foreach (var zn in zoneNumbers)
        {
            if (zn == UnassignedZone)
                z++;
        }

        return z;
    }

    public readonly struct ExtendedNodeFormat
    {
        public ExtendedNodeFormat(int glNodeType, bool compressed)
        {
            GlNodeType = glNodeType;
            Compressed = compressed;
        }

        public int GlNodeType { get; }
        public bool Compressed { get; }
    }

    public static bool TryParseExtendedNodeId(uint id, out ExtendedNodeFormat format)
    {
        format = id switch
        {
            0x444f4e5a => new ExtendedNodeFormat(0, true),
            0x4e4c475a => new ExtendedNodeFormat(1, true),
            0x324c475a => new ExtendedNodeFormat(2, true),
            0x334c475a => new ExtendedNodeFormat(3, true),
            0x444f4e58 => new ExtendedNodeFormat(0, false),
            0x4e4c4758 => new ExtendedNodeFormat(1, false),
            0x324c4758 => new ExtendedNodeFormat(2, false),
            0x334c4758 => new ExtendedNodeFormat(3, false),
            _ => default,
        };
        return id is 0x444f4e5a or 0x4e4c475a or 0x324c475a or 0x334c475a
            or 0x444f4e58 or 0x4e4c4758 or 0x324c4758 or 0x334c4758;
    }

    public static bool IsV4NodeHeader(ReadOnlySpan<byte> header8) =>
        header8.Length >= 8
        && header8[0] == (byte)'x'
        && header8[1] == (byte)'N'
        && header8[2] == (byte)'d'
        && header8[3] == (byte)'4';

    public static bool ShouldRebuildAfterNodeFailure => true;

    public static int CountZonesToAssign(IReadOnlyList<ushort> zoneNumbers)
    {
        var max = -1;
        foreach (var zn in zoneNumbers)
        {
            if (zn != UnassignedZone && zn > max)
                max = zn;
        }

        return max + 1;
    }

    public static bool ShouldUseCompressedLoader(bool compressed, bool decompressorOpened) =>
        !compressed || decompressorOpened;
}
