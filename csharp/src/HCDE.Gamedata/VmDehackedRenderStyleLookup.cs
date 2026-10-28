namespace HCDE.Gamedata;

/// <summary><c>FindStyle</c> and DEHSUPP <c>RenderStyles</c> rows from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedRenderStyleLookup
{
    private static readonly Dictionary<string, int> Styles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Normal"] = 0,
        ["Fuzzy"] = 1,
        ["Translucent"] = 2,
    };

    public static void RegisterStyle(int num, string name) => Styles[name] = num;

    public static int FindStyle(string name) =>
        Styles.TryGetValue(name, out var num) ? num : -1;
}
