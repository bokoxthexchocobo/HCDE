namespace HCDE.MapLoader;

public enum WadType : uint
{
    Unknown = 0,
    Iwad = 0x44415749, // "IWAD" little-endian
    Pwad = 0x44415750, // "PWAD"
}

public enum MapLumpKind : byte
{
    Label = 0,
    Things = 1,
    Linedefs = 2,
    Sidedefs = 3,
    Vertexes = 4,
    Segs = 5,
    Ssectors = 6,
    Nodes = 7,
    Sectors = 8,
    Reject = 9,
    Blockmap = 10,
    Behavior = 11,
    Conversation = 12,
    Lightmap = 13,
}

public static class MapLumpNames
{
    public const string Things = "THINGS";
    public const string Linedefs = "LINEDEFS";
    public const string Sidedefs = "SIDEDEFS";
    public const string Vertexes = "VERTEXES";
    public const string Segs = "SEGS";
    public const string Ssectors = "SSECTORS";
    public const string Nodes = "NODES";
    public const string Sectors = "SECTORS";
    public const string Reject = "REJECT";
    public const string Blockmap = "BLOCKMAP";
    public const string Behavior = "BEHAVIOR";
    public const string Textmap = "TEXTMAP";

    public static readonly string[] BinaryMapLumpOrder =
    [
        Things,
        Linedefs,
        Sidedefs,
        Vertexes,
        Segs,
        Ssectors,
        Nodes,
        Sectors,
        Reject,
        Blockmap,
    ];
}
