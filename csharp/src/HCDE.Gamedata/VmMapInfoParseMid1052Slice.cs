namespace HCDE.Gamedata;

/// <summary>
/// Cluster lookup, snapshot/defered clears, and <c>level_info_t::Reset</c> head from <c>g_mapinfo.cpp</c> (**151–300**).
/// Chunk **1052**; follows **1051** (**1–150**). Uncredited <see cref="VmMapInfoWarpTrans"/> / <see cref="VmMapInfoLevelReset"/> — seam only.
/// </summary>
public static class VmMapInfoParseMid1052Slice
{
    public const uint LevelVisitedFlag = 0x80000000;
    public const uint Level3SecretFlag = 1;
    public const uint Level2LaxMonsterActivation = 0x00000004;
    public const uint OutsideFogDefault = 0xff000000;
    public const int NameNone = 0;

    public static bool CheckWarpTransMapTailReturnsFalse() => false;

    public static bool SecretLevelVisited(IEnumerable<VmMapInfoVisitState> levels)
    {
        foreach (var lev in levels)
        {
            if (lev.IsSecret && lev.Visited)
                return true;
        }

        return false;
    }

    public static int FindWadClusterIndex(IReadOnlyList<VmMapInfoClusterRecord> clusters, int clusterId)
    {
        for (var i = 0; i < clusters.Count; i++)
        {
            if (clusters[i].Cluster == clusterId)
                return i;
        }

        return -1;
    }

    public static VmMapInfoClusterRecord ResolveClusterInfo(
        IReadOnlyList<VmMapInfoClusterRecord> clusters,
        int clusterId,
        VmMapInfoClusterRecord defaultCluster)
    {
        var idx = FindWadClusterIndex(clusters, clusterId);
        return idx >= 0 ? clusters[idx] : defaultCluster;
    }

    public static bool ShouldCleanAttachedLevelSnapshot(bool levelPresent, bool infoPresent) =>
        levelPresent && infoPresent;

    public static int ExtraRuntimeSnapshotClears(bool cleanPrimaryLevel, bool cleanVmLevel) =>
        (cleanPrimaryLevel ? 1 : 0) + (cleanVmLevel ? 1 : 0);

    public static void ClearDeferedForEachLevel(int levelCount, Action<int> clearDefered)
    {
        for (var i = 0; i < levelCount; i++)
            clearDefered(i);
    }

    public static void ApplyLevelInfoResetHead(VmMapInfoLevelInfoResetDraft level, bool hexenGameType)
    {
        level.MapName = string.Empty;
        level.MapBackground = string.Empty;
        level.LevelNum = 0;
        level.PName = string.Empty;
        level.NextMap = string.Empty;
        level.NextSecretMap = string.Empty;
        level.SkyPic1 = level.SkyPic2 = VmMapInfoLevelReset.NoFlatSky;
        level.SkyMistPic = VmMapInfoLevelReset.DefaultSkyMist;
        level.Cluster = 0;
        level.Partime = 0;
        level.Sucktime = 0;
        level.Flags = 0;
        level.Flags2 = hexenGameType ? 0 : Level2LaxMonsterActivation;
        level.Flags3 = 0;
        level.LightningSound = "world/thunder";
        level.Music = string.Empty;
        level.LevelName = string.Empty;
        level.AuthorName = string.Empty;
        level.MapLabel = string.Empty;
        level.FadeTable = "COLORMAP";
        level.CustomColorMap = "COLORMAP";
        level.WallHorizLight = -8;
        level.WallVertLight = 8;
        level.F1Pic = string.Empty;
        level.MusicOrder = 0;
        level.SnapshotCleared = true;
        level.DeferredCleared = true;
        level.SkySpeed1 = 0;
        level.SkySpeed2 = 0;
        level.SkyMistSpeed = 0;
        level.SkyMistyScale = 1;
        level.FadeTo = 0;
        level.OutsideFog = OutsideFogDefault;
        level.CdTrack = 0;
        level.CdId = 0;
        level.Gravity = 0;
        level.AirControl = 0;
        level.WarpTrans = 0;
        level.AirSupply = 20;
        level.CompatFlags = 0;
        level.CompatFlags2 = 0;
        level.CompatMask = 0;
        level.CompatMask2 = 0;
        level.Translator = string.Empty;
        level.RedirectType = NameNone;
        level.RedirectMapName = string.Empty;
        level.RedirectCvar = NameNone;
        level.RedirectCvarMapName = string.Empty;
        level.EnterPic = string.Empty;
        level.ExitPic = string.Empty;
        level.EnterAnim = string.Empty;
        level.ExitAnim = string.Empty;
        level.Intermission = NameNone;
        level.DeathSequence = NameNone;
        level.Slideshow = NameNone;
    }
}

public sealed class VmMapInfoClusterRecord
{
    public int Cluster { get; set; }
}

public sealed class VmMapInfoLevelInfoResetDraft
{
    public string MapName { get; set; } = "";
    public string MapBackground { get; set; } = "";
    public int LevelNum { get; set; }
    public string PName { get; set; } = "";
    public string NextMap { get; set; } = "";
    public string NextSecretMap { get; set; } = "";
    public string SkyPic1 { get; set; } = "";
    public string SkyPic2 { get; set; } = "";
    public string SkyMistPic { get; set; } = "";
    public int Cluster { get; set; }
    public int Partime { get; set; }
    public int Sucktime { get; set; }
    public uint Flags { get; set; }
    public uint Flags2 { get; set; }
    public uint Flags3 { get; set; }
    public string LightningSound { get; set; } = "";
    public string Music { get; set; } = "";
    public string LevelName { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string MapLabel { get; set; } = "";
    public string FadeTable { get; set; } = "";
    public string CustomColorMap { get; set; } = "";
    public int WallHorizLight { get; set; }
    public int WallVertLight { get; set; }
    public string F1Pic { get; set; } = "";
    public int MusicOrder { get; set; }
    public bool SnapshotCleared { get; set; }
    public bool DeferredCleared { get; set; }
    public float SkySpeed1 { get; set; }
    public float SkySpeed2 { get; set; }
    public float SkyMistSpeed { get; set; }
    public float SkyMistyScale { get; set; }
    public int FadeTo { get; set; }
    public uint OutsideFog { get; set; }
    public int CdTrack { get; set; }
    public int CdId { get; set; }
    public float Gravity { get; set; }
    public float AirControl { get; set; }
    public int WarpTrans { get; set; }
    public int AirSupply { get; set; }
    public uint CompatFlags { get; set; }
    public uint CompatFlags2 { get; set; }
    public uint CompatMask { get; set; }
    public uint CompatMask2 { get; set; }
    public string Translator { get; set; } = "";
    public int RedirectType { get; set; }
    public string RedirectMapName { get; set; } = "";
    public int RedirectCvar { get; set; }
    public string RedirectCvarMapName { get; set; } = "";
    public string EnterPic { get; set; } = "";
    public string ExitPic { get; set; } = "";
    public string EnterAnim { get; set; } = "";
    public string ExitAnim { get; set; } = "";
    public int Intermission { get; set; }
    public int DeathSequence { get; set; }
    public int Slideshow { get; set; }
}
