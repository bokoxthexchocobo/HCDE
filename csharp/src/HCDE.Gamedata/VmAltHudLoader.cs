namespace HCDE.Gamedata;

public enum VmGameTypeHud
{
    Doom,
    Heretic,
    Hexen,
    Strife
}

/// <summary><c>LoadAltHudStuff</c> defaults from <c>info.cpp</c>.</summary>
public static class VmAltHudLoader
{
    public static (string HealthPic, int BerserkPic) GetDefaultHudPics(VmGameTypeHud game)
    {
        return game switch
        {
            VmGameTypeHud.Heretic or VmGameTypeHud.Hexen => ("ARTIPTN2", -1),
            VmGameTypeHud.Strife => ("I_MDKT", -1),
            _ => ("MEDIA0", 0)
        };
    }

    public static bool TryApplyAltHudToken(string key, string value, VmAltHudDocument doc)
    {
        if (key.Equals("Health", StringComparison.OrdinalIgnoreCase))
        {
            doc.HealthPic = value;
            return true;
        }

        if (key.Equals("Berserk", StringComparison.OrdinalIgnoreCase))
        {
            doc.BerserkPic = value;
            return true;
        }

        doc.ItemIcons[key] = value;
        return true;
    }
}

public sealed class VmAltHudDocument
{
    public string HealthPic { get; set; } = "";
    public string BerserkPic { get; set; } = "";
    public Dictionary<string, string> ItemIcons { get; } = new(StringComparer.OrdinalIgnoreCase);
}
