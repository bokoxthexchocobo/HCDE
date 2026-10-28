using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>FLevelLocals::ChangeLevel</c> / MAPINFO next-map resolution for headless servers.</summary>
public static class HeadlessMapExit
{
    public static bool TryResolveNextMap(
        MapInfoSet? metadata,
        string currentMap,
        bool secretExit,
        out string? nextMap,
        out bool endSequence)
    {
        nextMap = null;
        endSequence = false;
        if (metadata is null)
        {
            nextMap = currentMap;
            return true;
        }

        var info = metadata.FindMap(currentMap);
        if (info is null)
        {
            nextMap = currentMap;
            return true;
        }

        var candidate = secretExit && info.SecretNextMap.Length > 0 ? info.SecretNextMap : info.NextMap;
        if (candidate.Length == 0)
        {
            endSequence = true;
            return true;
        }

        if (candidate.StartsWith("#", StringComparison.Ordinal))
        {
            endSequence = true;
            return true;
        }

        nextMap = candidate;
        return true;
    }

    public static bool ShouldDoIntermission(
        MapInfoSet? metadata,
        string currentMap,
        string nextMap,
        bool deathmatch,
        bool alwaysTally = false,
        bool levelNoIntermission = false)
    {
        if (alwaysTally || deathmatch)
            return true;
        if (levelNoIntermission)
            return false;
        if (metadata is null)
            return true;

        var current = metadata.FindMap(currentMap);
        var next = metadata.FindMap(nextMap);
        if (current is null || next is null)
            return true;

        if (current.Cluster == next.Cluster)
        {
            var cluster = metadata.FindCluster(current.Cluster);
            if (cluster is { Hub: true, AllowIntermission: false })
                return false;
        }

        return true;
    }

    public static bool TryLoadPlayLevel(
        ReadOnlySpan<byte> wad,
        string mapName,
        MapInfoSet? metadata,
        BinaryThingFlagFormat thingFlags,
        out PlayLevel? level,
        out string? error)
    {
        if (!LevelBuilder.TryFromWad(wad, mapName, out level, out error, thingFlags))
            return false;
        return true;
    }
}
