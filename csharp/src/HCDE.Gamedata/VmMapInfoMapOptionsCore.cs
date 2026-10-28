namespace HCDE.Gamedata;

public sealed class VmMapInfoLevelOptionsDraft
{
    public int LevelNum { get; set; }
    public string NextMap { get; set; } = "";
    public string NextSecretMap { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string MapLabel { get; set; } = "";
    public string LightningSound { get; set; } = "";
    public int Cluster { get; set; }
    public bool ClusterIsHub { get; set; }
    public string SkyPic1 { get; set; } = "";
    public string SkyPic2 { get; set; } = "";
    public string SkyMistPic { get; set; } = "";
    public double SkySpeed1 { get; set; }
    public double SkySpeed2 { get; set; }
}

/// <summary>MAPINFO map option handlers from <c>g_mapinfo.cpp</c> (1083–1178).</summary>
public static class VmMapInfoMapOptionsCore
{
    public static void ApplyLevelNum(VmMapInfoLevelOptionsDraft draft, int levelNum) =>
        draft.LevelNum = levelNum;

    public static void ApplyNextMap(VmMapInfoLevelOptionsDraft draft, string next) =>
        draft.NextMap = next.Trim();

    public static void ApplySecretNext(VmMapInfoLevelOptionsDraft draft, string secret) =>
        draft.NextSecretMap = secret.Trim();

    public static void ApplyAuthor(VmMapInfoLevelOptionsDraft draft, string author) =>
        draft.AuthorName = author;

    public static void ApplyLabel(VmMapInfoLevelOptionsDraft draft, string label) =>
        draft.MapLabel = label;

    public static void ApplyLightningSound(VmMapInfoLevelOptionsDraft draft, string sound) =>
        draft.LightningSound = sound;

    public static void ApplyCluster(VmMapInfoLevelOptionsDraft draft, int cluster, bool hexenHack)
    {
        draft.Cluster = cluster;
        draft.ClusterIsHub = hexenHack;
    }

    public static void ApplySky1(VmMapInfoLevelOptionsDraft draft, string sky, double speed, bool hexenHack)
    {
        draft.SkyPic1 = sky;
        draft.SkySpeed1 = hexenHack ? speed / 256 : speed;
    }

    public static void ApplySky2(VmMapInfoLevelOptionsDraft draft, string sky, double speed, bool hexenHack)
    {
        draft.SkyPic2 = sky;
        draft.SkySpeed2 = hexenHack ? speed / 256 : speed;
    }
}
