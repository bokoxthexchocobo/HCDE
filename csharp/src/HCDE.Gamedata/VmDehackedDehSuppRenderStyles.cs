namespace HCDE.Gamedata;

public readonly record struct VmDehackedRenderStyleName(int Num, string Name);

/// <summary><c>LoadDehSupp</c> <c>RenderStyles</c> from <c>d_dehacked.cpp</c> (3708–3724).</summary>
public static class VmDehackedDehSuppRenderStyles
{
    public const int MaxNameLength = 19;

    public static bool TryParseEntry(int num, string name, out VmDehackedRenderStyleName entry)
    {
        entry = default;
        if (string.IsNullOrWhiteSpace(name))
            return false;
        var trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
            trimmed = trimmed[..MaxNameLength];
        entry = new VmDehackedRenderStyleName(num, trimmed);
        return true;
    }

    public static List<VmDehackedRenderStyleName> ParseRenderStylesSection(
        IReadOnlyList<(int Num, string Name)> rows)
    {
        var list = new List<VmDehackedRenderStyleName>();
        foreach (var (num, name) in rows)
        {
            if (!TryParseEntry(num, name, out var entry))
                break;
            list.Add(entry);
        }

        return list;
    }
}
