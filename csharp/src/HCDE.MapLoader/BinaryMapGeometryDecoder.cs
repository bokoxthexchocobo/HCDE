namespace HCDE.MapLoader;

public readonly struct BinaryMapGeometry
{
    public BinaryMapGeometry(MapVertexRecord[] vertices, MapSegRecord[] segs, MapNodeRecord[] nodes)
    {
        Vertices = vertices;
        Segs = segs;
        Nodes = nodes;
    }

    public MapVertexRecord[] Vertices { get; }
    public MapSegRecord[] Segs { get; }
    public MapNodeRecord[] Nodes { get; }
}

public static class BinaryMapGeometryDecoder
{
    public static bool TryDecode(
        ReadOnlySpan<byte> wad,
        MapLumpCatalog catalog,
        out BinaryMapGeometry geometry,
        out string? rejectReason)
    {
        geometry = default;
        rejectReason = null;
        if (catalog.Format != MapDataFormat.DoomBinary)
        {
            rejectReason = "map-not-binary";
            return false;
        }

        if (!catalog.TryGetLump(MapLumpKind.Vertexes, out var vertexLump)
            || !catalog.TryGetLump(MapLumpKind.Segs, out var segLump)
            || !catalog.TryGetLump(MapLumpKind.Nodes, out var nodeLump))
        {
            rejectReason = "map-missing-geometry-lumps";
            return false;
        }

        if (!WadArchiveReader.TryReadLumpData(wad, vertexLump.Entry, out var vertexData, out rejectReason)
            || !MapVertexCodec.TryReadAll(vertexData, out var vertices, out rejectReason))
        {
            return false;
        }

        if (!WadArchiveReader.TryReadLumpData(wad, segLump.Entry, out var segData, out rejectReason)
            || !MapSegCodec.TryReadAll(segData, out var segs, out rejectReason))
        {
            return false;
        }

        if (!WadArchiveReader.TryReadLumpData(wad, nodeLump.Entry, out var nodeData, out rejectReason)
            || !MapNodeCodec.TryReadAll(nodeData, out var nodes, out rejectReason))
        {
            return false;
        }

        geometry = new BinaryMapGeometry(vertices, segs, nodes);
        return true;
    }
}
