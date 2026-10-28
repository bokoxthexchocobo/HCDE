namespace HCDE.Gamedata;

/// <summary>Field layout mirrored from <c>UMapEntry</c> in <c>umapinfo.cpp</c> (data-only).</summary>
public static class VmUMapInfoSchema
{
    public static IReadOnlyList<string> LumpNameFields =>
    [
        "levelpic", "nextmap", "nextsecret", "music", "skytexture", "endpic",
        "exitpic", "enterpic", "exitanim", "enteranim", "interbackdrop", "intermusic"
    ];

    public static int DefaultId24LevelNum => 0;

    public static VmUMapPlayerActionOption DefaultPlayerAction => VmUMapPlayerActionOption.Unset;
}
