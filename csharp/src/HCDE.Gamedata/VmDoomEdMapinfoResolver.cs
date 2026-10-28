namespace HCDE.Gamedata;

public sealed class VmDoomEdEntry
{
    public string? ClassName { get; set; }
    public int Special { get; set; } = -1;
    public sbyte ArgsDefined { get; set; }
    public int[] Args { get; } = new int[5];
    public bool NoSkillFlags { get; set; }
}

/// <summary><c>InitActorNumsFromMapinfo</c> and <c>P_GetSpawnableType</c> from <c>g_doomedmap.cpp</c>.</summary>
public static class VmDoomEdMapinfoResolver
{
    public static Dictionary<int, VmDoomEdEntry> ResolvePendingEdItems(
        IReadOnlyDictionary<int, VmMapinfoEdMapItem> pending,
        Func<string, bool> validateClass,
        List<string> errors)
    {
        var result = new Dictionary<int, VmDoomEdEntry>();
        foreach (var (key, item) in pending)
        {
            if (item.ClassName is not null && !validateClass(item.ClassName))
            {
                errors.Add($"Unknown actor class {item.ClassName}");
                continue;
            }

            result[key] = new VmDoomEdEntry
            {
                ClassName = item.ClassName,
                Special = item.Special,
                ArgsDefined = item.ArgsDefined,
                NoSkillFlags = item.NoSkillFlags
            };
            Array.Copy(item.Args, result[key].Args, 5);
        }

        return result;
    }

    public static string? TryGetSpawnableClassName(int spawnNum, IReadOnlyDictionary<int, string> spawnables, IReadOnlyDictionary<int, string> namedSpawns)
    {
        if (spawnNum < 0)
        {
            if (namedSpawns.TryGetValue(-spawnNum, out var named)) return named;
            return null;
        }

        return spawnables.TryGetValue(spawnNum, out var cls) ? cls : null;
    }
}
