namespace HCDE.Gamedata;

public sealed class VmKeyConfPlayerClassEntry
{
    public string ClassName { get; set; } = "";
    public bool NoMenu { get; set; }
}

/// <summary>KEYCONF <c>clearplayerclasses</c> / <c>addplayerclass</c> from <c>keysections.cpp</c>.</summary>
public static class VmKeyConfPlayerParser
{
    public static readonly string[] IwadPlayerClassNames =
    [
        "DoomPlayer", "HereticPlayer", "StrifePlayer", "FighterPlayer", "ClericPlayer", "MagePlayer", "ChexPlayer"
    ];

    public static void ClearPlayerClasses(List<VmKeyConfPlayerClassEntry> classes, bool setSlotStrict)
    {
        if (setSlotStrict)
        {
            classes.Clear();
            return;
        }

        foreach (var iwad in IwadPlayerClassNames)
            classes.RemoveAll(c => c.ClassName.Equals(iwad, StringComparison.OrdinalIgnoreCase));
    }

    public static bool TryAddPlayerClass(
        List<VmKeyConfPlayerClassEntry> classes,
        string className,
        IReadOnlyList<string> flags,
        List<string> errors,
        Func<string, bool>? validateClass = null)
    {
        if (validateClass is not null && !validateClass(className))
            return false;

        if (classes.Any(c => c.ClassName.Equals(className, StringComparison.OrdinalIgnoreCase)))
            return true;

        var entry = new VmKeyConfPlayerClassEntry { ClassName = className };
        foreach (var flag in flags)
        {
            if (flag.Equals("nomenu", StringComparison.OrdinalIgnoreCase)) entry.NoMenu = true;
            else errors.Add($"Unknown flag '{flag}' for player class '{className}'.");
        }

        classes.Add(entry);
        return errors.Count == 0;
    }
}
