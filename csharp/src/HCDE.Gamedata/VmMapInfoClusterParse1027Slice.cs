namespace HCDE.Gamedata;

/// <summary>
/// `ParseMusic`, `ParseCutscene`, and `ParseCluster` head from <c>g_mapinfo.cpp</c> (**833–982**).
/// </summary>
public static class VmMapInfoClusterParse1027Slice
{
    public const int ClusterLookupClusterName = 1 << 0;
    public const int ClusterLookupEnterText = 1 << 1;
    public const int ClusterLookupExitText = 1 << 2;
    public const int ClusterFinalePic = 1 << 3;
    public const int ClusterHub = 1 << 4;
    public const int ClusterAllowIntermission = 1 << 5;
    public const int ClusterEnterTextInLump = 1 << 6;
    public const int ClusterExitTextInLump = 1 << 7;

    public sealed class CutsceneDef
    {
        public string Video { get; set; } = "";
        public string Function { get; set; } = "";
        public string SoundName { get; set; } = "";
        public int SoundId { get; set; }
        public int FramesPerSec { get; set; }
    }

    public sealed class ClusterDef
    {
        public int Cluster { get; set; }
        public int Flags { get; set; }
        public string ClusterName { get; set; } = "";
        public string EnterText { get; set; } = "";
        public string ExitText { get; set; } = "";
        public string MessageMusic { get; set; } = "";
        public int MusicOrder { get; set; }
        public string FinaleFlat { get; set; } = "";
        public int CdTrack { get; set; }
        public uint CdId { get; set; }
        public CutsceneDef Intro { get; } = new();
        public CutsceneDef Outro { get; } = new();
        public CutsceneDef Gameover { get; } = new();
    }

    public static void ParseMusicToken(string token, bool hasTrailingNumber, int trailingNumber, out string name, out int order)
    {
        order = 0;
        var colon = token.IndexOf(':');
        if (colon >= 0)
        {
            order = int.Parse(token.AsSpan(colon + 1));
            name = token[..colon];
            return;
        }

        name = token;
        if (hasTrailingNumber) order = trailingNumber;
    }

    public static bool TryParseCdIdHex(string token, out uint cdId) =>
        uint.TryParse(token, System.Globalization.NumberStyles.HexNumber, null, out cdId);

    public static bool TryApplyDefaultClusterStringLabel(
        int clusterNumber, string prefix, string currentText, Func<string, string, bool> matchesDefaultString,
        out string resolvedText, out int lookupFlag)
    {
        resolvedText = currentText;
        lookupFlag = 0;
        var testLabel = $"{prefix}{clusterNumber}";
        if (!matchesDefaultString(testLabel, currentText)) return false;
        resolvedText = testLabel;
        lookupFlag = prefix.StartsWith("CLUSTERENTER", StringComparison.Ordinal) ? ClusterLookupEnterText : ClusterLookupExitText;
        return true;
    }

    public static ClusterDef CreateFreshCluster(int clusterNumber) => new() { Cluster = clusterNumber };

    public static bool ApplyClusterTextProperty(
        ClusterDef cluster, bool enterText, string text, bool lookupName, Func<string, string, bool> matchesDefaultString)
    {
        if (enterText) cluster.EnterText = text;
        else cluster.ExitText = text;
        if (lookupName)
        {
            cluster.Flags |= enterText ? ClusterLookupEnterText : ClusterLookupExitText;
            return true;
        }

        var prefix = enterText ? "CLUSTERENTER" : "CLUSTEREXIT";
        if (!TryApplyDefaultClusterStringLabel(cluster.Cluster, prefix, text, matchesDefaultString, out var resolved, out var flag))
            return true;
        if (enterText) cluster.EnterText = resolved;
        else cluster.ExitText = resolved;
        cluster.Flags |= flag;
        return true;
    }

    public static bool ApplyClusterProperty(
        ClusterDef cluster, string keyword, string stringValue, bool lookupName, int musicOrder,
        CutsceneDef? cutscenePayload, Func<string, string, bool> matchesDefaultString)
    {
        switch (keyword.ToLowerInvariant())
        {
            case "name":
                cluster.ClusterName = stringValue;
                if (lookupName) cluster.Flags |= ClusterLookupClusterName;
                return true;
            case "entertext":
                return ApplyClusterTextProperty(cluster, enterText: true, stringValue, lookupName, matchesDefaultString);
            case "exittext":
                return ApplyClusterTextProperty(cluster, enterText: false, stringValue, lookupName, matchesDefaultString);
            case "music":
                cluster.MessageMusic = stringValue;
                cluster.MusicOrder = musicOrder;
                return true;
            case "flat":
                cluster.FinaleFlat = stringValue;
                return true;
            case "pic":
                cluster.FinaleFlat = stringValue;
                cluster.Flags |= ClusterFinalePic;
                return true;
            case "hub":
                cluster.Flags |= ClusterHub;
                return true;
            case "allowintermission":
                cluster.Flags |= ClusterAllowIntermission;
                return true;
            case "cdtrack":
                cluster.CdTrack = int.Parse(stringValue);
                return true;
            case "cdid":
                if (TryParseCdIdHex(stringValue, out var id)) cluster.CdId = id;
                return true;
            case "entertextislump":
                cluster.Flags |= ClusterEnterTextInLump;
                return true;
            case "exittextislump":
                cluster.Flags |= ClusterExitTextInLump;
                return true;
            case "intro" when cutscenePayload is not null:
                CopyCutscene(cluster.Intro, cutscenePayload);
                return true;
            case "outro" when cutscenePayload is not null:
                CopyCutscene(cluster.Outro, cutscenePayload);
                return true;
            case "gameover" when cutscenePayload is not null:
                CopyCutscene(cluster.Gameover, cutscenePayload);
                return true;
            default:
                return false;
        }
    }

    public static bool ApplyCutsceneKeyword(CutsceneDef c, string keyword, string value, int numericValue, bool allowFunction)
    {
        switch (keyword.ToLowerInvariant())
        {
            case "video":
                c.Video = value;
                c.Function = "";
                return true;
            case "function" when allowFunction:
                c.Function = value;
                c.Video = "";
                return true;
            case "sound":
                c.SoundName = value;
                return true;
            case "soundid":
                c.SoundId = numericValue;
                return true;
            case "fps":
                c.FramesPerSec = numericValue;
                return true;
            case "delete":
                c.Function = "none";
                c.Video = "";
                return true;
            case "clear":
                c.Video = c.Function = c.SoundName = "";
                c.SoundId = c.FramesPerSec = 0;
                return true;
            default:
                return false;
        }
    }

    private static void CopyCutscene(CutsceneDef dest, CutsceneDef src)
    {
        dest.Video = src.Video;
        dest.Function = src.Function;
        dest.SoundName = src.SoundName;
        dest.SoundId = src.SoundId;
        dest.FramesPerSec = src.FramesPerSec;
    }
}
