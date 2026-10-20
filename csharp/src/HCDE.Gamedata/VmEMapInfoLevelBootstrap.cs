namespace HCDE.Gamedata;

public sealed class VmEMapInfoWadLevel
{
    public string MapName { get; set; } = "";
    public int LevelNum { get; set; }
    public int Id24LevelNum { get; set; }
}

/// <summary><c>SetLevelNum</c> / <c>GetOrCreateLevel</c> from <c>hcde_emapinfo.cpp</c> (302–349).</summary>
public static class VmEMapInfoLevelBootstrap
{
    public static void SetLevelNum(IList<VmEMapInfoWadLevel> levels, VmEMapInfoWadLevel target, int levelNum)
    {
        foreach (var level in levels)
        {
            if (!ReferenceEquals(level, target) && level.LevelNum == levelNum)
                level.LevelNum = 0;
        }

        target.LevelNum = levelNum;
        target.Id24LevelNum = 0;
    }

    public static VmEMapInfoWadLevel GetOrCreateLevel(
        IList<VmEMapInfoWadLevel> levels,
        string rawMapName,
        VmEMapInfoWadLevel template,
        Func<IReadOnlyList<VmEMapInfoWadLevel>, string, VmEMapInfoWadLevel?> findExisting)
    {
        var upper = rawMapName.ToUpperInvariant();
        var existing = findExisting((IReadOnlyList<VmEMapInfoWadLevel>)levels, upper);
        if (existing != null)
            return existing;

        var created = new VmEMapInfoWadLevel
        {
            MapName = upper,
            LevelNum = VmEMapInfoParsePrimitives.GetDefaultLevelNum(upper, out var id24),
            Id24LevelNum = id24
        };

        levels.Add(created);
        if (created.LevelNum != 0)
            SetLevelNum(levels, created, created.LevelNum);

        return created;
    }
}
