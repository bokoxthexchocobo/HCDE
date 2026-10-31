namespace HCDE.MapLoader;

/// <summary>
/// Compatibility option tables, side names, and the MD5 map traits from
/// <c>compatibility.cpp</c> 1-203. <c>SkipZandronumCompatCommand</c> at 204
/// stays on the later chunk. <c>hash_t</c> is a 32-bit unsigned int, so the
/// union's <c>Hash</c> member is the first digest word in little-endian order.
/// </summary>
public static class VmCompatTables1382Slice
{
    public const int SlotCompat = 0;
    public const int SlotCompat2 = 1;
    public const int SlotBcompat = 2;
    public const int SlotCount = 3;

    public readonly record struct Option(string? Name, uint Flags, int Slot);

    public readonly record struct Values(uint Flags1, uint Flags2, uint BugFlags, uint ExtCommandIndex);

    public static readonly Option[] Options =
    [
        new("setslopeoverflow", 1u << 0, SlotBcompat),
        new("resetplayerspeed", 1u << 1, SlotBcompat),
        new("vileghosts", 1u << 25, SlotCompat),
        new("ignoreteleporttags", 1u << 3, SlotBcompat),
        new("rebuildnodes", 1u << 5, SlotBcompat),
        new("linkfrozenprops", 1u << 6, SlotBcompat),
        new("floatbob", 1u << 8, SlotBcompat),
        new("noslopeid", 1u << 9, SlotBcompat),
        new("clipmidtex", 1u << 10, SlotBcompat),
        new("nosectionmerge", 1u << 11, SlotBcompat),
        new("nomirrors", 1u << 12, SlotBcompat),
        new("shorttex", 1u << 0, SlotCompat),
        new("stairs", 1u << 1, SlotCompat),
        new("limitpain", 1u << 2, SlotCompat),
        new("nopassover", 1u << 4, SlotCompat),
        new("notossdrops", 1u << 7, SlotCompat),
        new("useblocking", 1u << 8, SlotCompat),
        new("nodoorlight", 1u << 9, SlotCompat),
        new("ravenscroll", 1u << 10, SlotCompat),
        new("soundtarget", 1u << 11, SlotCompat),
        new("dehhealth", 1u << 12, SlotCompat),
        new("trace", 1u << 13, SlotCompat),
        new("dropoff", 1u << 14, SlotCompat),
        new("boomscroll", 1u << 15, SlotCompat),
        new("invisibility", 1u << 16, SlotCompat),
        new("silentinstantfloors", 1u << 17, SlotCompat),
        new("sectorsounds", 1u << 18, SlotCompat),
        new("missileclip", 1u << 19, SlotCompat),
        new("crossdropoff", 1u << 20, SlotCompat),
        new("wallrun", 1u << 6, SlotCompat),
        new("anybossdeath", 1u << 21, SlotCompat),
        new("mushroom", 1u << 23, SlotCompat),
        new("mbfmonstermove", 1u << 24, SlotCompat),
        new("noblockfriends", 1u << 26, SlotCompat),
        new("spritesort", 1u << 27, SlotCompat),
        new("hitscan", 1u << 28, SlotCompat),
        new("lightlevel", 1u << 29, SlotCompat),
        new("polyobj", 1u << 30, SlotCompat),
        new("maskedmidtex", 1u << 31, SlotCompat),
        new("badangles", 1u << 0, SlotCompat2),
        new("floormove", 1u << 1, SlotCompat2),
        new("soundcutoff", 1u << 2, SlotCompat2),
        new("pointonline", 1u << 3, SlotCompat2),
        new("multiexit", 1u << 4, SlotCompat2),
        new("teleport", 1u << 5, SlotCompat2),
        new("disablepushwindowcheck", 1u << 6, SlotCompat2),
        new("checkswitchrange", 1u << 7, SlotCompat2),
        new("explode1", 1u << 8, SlotCompat2),
        new("explode2", 1u << 9, SlotCompat2),
        new("railing", 1u << 10, SlotCompat2),
        new("scriptwait", 1u << 11, SlotCompat2),
        new("nombf21", 1u << 14, SlotCompat2),
        new("noid24", 1u << 24, SlotCompat2),
        new("voodoozombies", 1u << 15, SlotCompat2),
        new("fdteleport", 1u << 16, SlotCompat2),
        new("noacsargcheck", 1u << 17, SlotCompat2),
        new("novdolllockmsg", 1u << 18, SlotCompat2),
        new("emulatemikoportals", 1u << 19, SlotCompat2),
        new(null, 0u, 0),
    ];

    public static readonly string?[] LineSides = ["Front", "Back", null];
    public static readonly string?[] WallTiers = ["Top", "Mid", "Bot", null];
    public static readonly string?[] SectorPlanes = ["floor", "ceil", null];
    public static readonly string?[] ZandronumCompatCommands =
    [
        "sectorflooroffset",
        "setwallyscale",
        "setlineflags",
        "clearlineflags",
        "setactivation",
        "clearlinespecial",
        "setlinespecial",
        "setthingz",
        "corpsegibs",
        null,
    ];

    public static uint Hash(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 16)
            throw new InvalidOperationException("md5");
        return (uint)bytes[0] | ((uint)bytes[1] << 8) | ((uint)bytes[2] << 16) | ((uint)bytes[3] << 24);
    }

    public static int Compare(uint left0, uint left1, uint left2, uint left3, uint right0, uint right1, uint right2, uint right3)
    {
        if (left0 != right0 || left1 != right1 || left2 != right2 || left3 != right3)
            return 1;
        return 0;
    }
}
