namespace HCDE.Client;

public static partial class VmPredefinedNames
{
    private static Dictionary<string, int>? indexByName;

    public static int GetIndex(string name)
    {
        if (string.IsNullOrEmpty(name)) return 0;
        indexByName ??= BuildIndex();
        return indexByName.TryGetValue(name, out var idx) ? idx : -1;
    }

    public static string GetName(int index) =>
        index >= 0 && index < Names.Length ? Names[index] : "";

    private static Dictionary<string, int> BuildIndex()
    {
        var map = new Dictionary<string, int>(Names.Length, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < Names.Length; i++) map[Names[i]] = i;
        return map;
    }
}
