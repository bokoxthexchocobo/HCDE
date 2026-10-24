namespace HCDE.Playsim;

/// <summary>Native <c>FSwitchDef</c> / <c>anim_switches.cpp</c> switch texture pairs.</summary>
internal sealed class SwitchDefinition
{
    public required string PreTexture { get; init; }
    public SwitchDefinition? Pair { get; set; }
    public string? Sound { get; init; }
    public bool QuestPanel { get; init; }
    public SwitchFrame[] Frames { get; init; } = [new SwitchFrame { Texture = "" }];
}

internal readonly record struct SwitchFrame(string Texture, int TimeMin = 0, int TimeRnd = 0);

internal enum SwitchWallPart { Top = 0, Mid = 1, Bottom = 2 }

internal static class SwitchCatalog
{
    private static readonly Dictionary<string, SwitchDefinition> ByTexture = new(StringComparer.OrdinalIgnoreCase);
    private static bool _initialized;

    internal static void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;
        foreach (var (off, on) in BuiltinPairs)
            RegisterPair(off, on);
    }

    internal static SwitchDefinition? Find(string texture)
    {
        if (string.IsNullOrEmpty(texture) || texture == "-") return null;
        return ByTexture.TryGetValue(texture, out var def) ? def : null;
    }

    internal static void RegisterPair(string offTexture, string onTexture, string? sound = null)
    {
        var off = offTexture.ToUpperInvariant();
        var on = onTexture.ToUpperInvariant();
        if (string.Equals(off, on, StringComparison.Ordinal)) return;
        var defOff = new SwitchDefinition
        {
            PreTexture = off,
            Frames = [new SwitchFrame(on)],
            Sound = sound,
        };
        var defOn = new SwitchDefinition
        {
            PreTexture = on,
            Frames = [new SwitchFrame(off)],
            Sound = sound,
        };
        defOff.Pair = defOn;
        defOn.Pair = defOff;
        ByTexture[off] = defOff;
        ByTexture[on] = defOn;
    }

    private static readonly (string Off, string On)[] BuiltinPairs =
    [
        ("SW1BRCOM", "SW2BRCOM"), ("SW1BRN1", "SW2BRN1"), ("SW1BRN2", "SW2BRN2"), ("SW1BRNGN", "SW2BRNGN"),
        ("SW1BROWN", "SW2BROWN"), ("SW1COMM", "SW2COMM"), ("SW1COMP", "SW2COMP"), ("SW1DIRT", "SW2DIRT"),
        ("SW1EXIT", "SW2EXIT"), ("SW1GRAY", "SW2GRAY"), ("SW1GRAY1", "SW2GRAY1"), ("SW1METAL", "SW2METAL"),
        ("SW1PIPE", "SW2PIPE"), ("SW1SLAD", "SW2SLAD"), ("SW1STARG", "SW2STARG"), ("SW1STON1", "SW2STON1"),
        ("SW1STON2", "SW2STON2"), ("SW1STONE", "SW2STONE"), ("SW1STRTN", "SW2STRTN"), ("SW1BLUE", "SW2BLUE"),
        ("SW1CMT", "SW2CMT"), ("SW1GARG", "SW2GARG"), ("SW1GSTON", "SW2GSTON"), ("SW1HOT", "SW2HOT"),
        ("SW1LION", "SW2LION"), ("SW1SATYR", "SW2SATYR"), ("SW1SKIN", "SW2SKIN"), ("SW1SKULL", "SW2SKULL"),
        ("SW1WOOD", "SW2WOOD"), ("SW1ZIM", "SW2ZIM"),
    ];
}
