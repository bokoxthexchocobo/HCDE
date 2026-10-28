namespace HCDE.Gamedata;

/// <summary>
/// `ParseMusic`, `ParseCutscene`, and `ParseCluster` head from <c>g_mapinfo.cpp</c> (**833–982**).
/// Chunk **1027**; avoids ParseExit* merge band **754–832** (`VmMapInfoMapOptionsExitEnvironment`) and Hexen remap **1008+** (`VmMapInfoClusterExitTextRemap`).
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

        public void Reset()
        {
            Flags = 0;
            ClusterName = "";
            EnterText = "";
            ExitText = "";
            MessageMusic = "";
            MusicOrder = 0;
            FinaleFlat = "";
            CdTrack = 0;
            CdId = 0;
            Intro.Video = Intro.Function = Intro.SoundName = "";
            Intro.SoundId = Intro.FramesPerSec = 0;
            Outro.Video = Outro.Function = Outro.SoundName = "";
            Outro.SoundId = Outro.FramesPerSec = 0;
            Gameover.Video = Gameover.Function = Gameover.SoundName = "";
            Gameover.SoundId = Gameover.FramesPerSec = 0;
        }
    }

    public static void ParseMusicToken(string token, bool hasTrailingNumber, int trailingNumber, out string name, out int order)
    {
        order = 0;
        var colon = token.IndexOf(':');
        if (colon >= 0)
        {
            order = int.Parse(token.AsSpan(colon + 1), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
            name = token[..colon];
            return;
        }

        name = token;
        if (hasTrailingNumber)
            order = trailingNumber;
    }

    public static bool TryParseCdIdHex(string token, out uint cdId)
    {
        cdId = 0;
        if (string.IsNullOrWhiteSpace(token))
            return false;
        return uint.TryParse(token, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out cdId);
    }

    public static bool TryApplyDefaultClusterStringLabel(
        int clusterNumber,
        string prefix,
        string currentText,
        Func<string, string, bool> matchesDefaultString,
        out string resolvedText,
        out int lookupFlag)
    {
        resolvedText = currentText;
        lookupFlag = 0;
        var testLabel = $"{prefix}{clusterNumber}";
        if (!matchesDefaultString(testLabel, currentText))
            return false;
        resolvedText = testLabel;
        lookupFlag = prefix.StartsWith("CLUSTERENTER", StringComparison.Ordinal)
            ? ClusterLookupEnterText
            : ClusterLookupExitText;
        return true;
    }

    public static bool ApplyClusterProperty(
        ClusterDef cluster,
        string keyword,
        string stringValue,
        bool lookupName,
        int musicOrder,
        bool finaleIsPic,
        CutsceneDef? cutscenePayload,
        Func<string, string, bool> matchesDefaultString)
    {
        if (keyword.Equals("name", StringComparison.OrdinalIgnoreCase))
        {
            cluster.ClusterName = stringValue;
            if (lookupName)
                cluster.Flags |= ClusterLookupClusterName;
            return true;
        }

        if (keyword.Equals("entertext", StringComparison.OrdinalIgnoreCase))
        {
            cluster.EnterText = stringValue;
            if (lookupName)
                cluster.Flags |= ClusterLookupEnterText;
            else if (TryApplyDefaultClusterStringLabel(cluster.Cluster, "CLUSTERENTER", stringValue, matchesDefaultString,
                         out var resolved, out var flag))
            {
                cluster.EnterText = resolved;
                cluster.Flags |= flag;
            }

            return true;
        }

        if (keyword.Equals("exittext", StringComparison.OrdinalIgnoreCase))
        {
            cluster.ExitText = stringValue;
            if (lookupName)
                cluster.Flags |= ClusterLookupExitText;
            else if (TryApplyDefaultClusterStringLabel(cluster.Cluster, "CLUSTEREXIT", stringValue, matchesDefaultString,
                         out var resolved, out var flag))
            {
                cluster.ExitText = resolved;
                cluster.Flags |= flag;
            }

            return true;
        }

        if (keyword.Equals("music", StringComparison.OrdinalIgnoreCase))
        {
            cluster.MessageMusic = stringValue;
            cluster.MusicOrder = musicOrder;
            return true;
        }

        if (keyword.Equals("flat", StringComparison.OrdinalIgnoreCase))
        {
            cluster.FinaleFlat = stringValue;
            return true;
        }

        if (keyword.Equals("pic", StringComparison.OrdinalIgnoreCase))
        {
            cluster.FinaleFlat = stringValue;
            cluster.Flags |= ClusterFinalePic;
            return true;
        }

        if (keyword.Equals("hub", StringComparison.OrdinalIgnoreCase))
        {
            cluster.Flags |= ClusterHub;
            return true;
        }

        if (keyword.Equals("allowintermission", StringComparison.OrdinalIgnoreCase))
        {
            cluster.Flags |= ClusterAllowIntermission;
            return true;
        }

        if (keyword.Equals("cdtrack", StringComparison.OrdinalIgnoreCase))
        {
            cluster.CdTrack = int.Parse(stringValue, System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }

        if (keyword.Equals("cdid", StringComparison.OrdinalIgnoreCase))
        {
            if (TryParseCdIdHex(stringValue, out var id))
                cluster.CdId = id;
            return true;
        }

        if (keyword.Equals("entertextislump", StringComparison.OrdinalIgnoreCase))
        {
            cluster.Flags |= ClusterEnterTextInLump;
            return true;
        }

        if (keyword.Equals("exittextislump", StringComparison.OrdinalIgnoreCase))
        {
            cluster.Flags |= ClusterExitTextInLump;
            return true;
        }

        if (keyword.Equals("intro", StringComparison.OrdinalIgnoreCase) && cutscenePayload is not null)
        {
            CopyCutscene(cluster.Intro, cutscenePayload);
            return true;
        }

        if (keyword.Equals("outro", StringComparison.OrdinalIgnoreCase) && cutscenePayload is not null)
        {
            CopyCutscene(cluster.Outro, cutscenePayload);
            return true;
        }

        if (keyword.Equals("gameover", StringComparison.OrdinalIgnoreCase) && cutscenePayload is not null)
        {
            CopyCutscene(cluster.Gameover, cutscenePayload);
            return true;
        }

        return false;
    }

    public static bool ApplyCutsceneKeyword(CutsceneDef cutscene, string keyword, string value, int numericValue, bool allowFunction)
    {
        if (keyword.Equals("video", StringComparison.OrdinalIgnoreCase))
        {
            cutscene.Video = value;
            cutscene.Function = "";
            return true;
        }

        if (keyword.Equals("function", StringComparison.OrdinalIgnoreCase) && allowFunction)
        {
            cutscene.Function = value;
            cutscene.Video = "";
            return true;
        }

        if (keyword.Equals("sound", StringComparison.OrdinalIgnoreCase))
        {
            cutscene.SoundName = value;
            return true;
        }

        if (keyword.Equals("soundid", StringComparison.OrdinalIgnoreCase))
        {
            cutscene.SoundId = numericValue;
            return true;
        }

        if (keyword.Equals("fps", StringComparison.OrdinalIgnoreCase))
        {
            cutscene.FramesPerSec = numericValue;
            return true;
        }

        if (keyword.Equals("delete", StringComparison.OrdinalIgnoreCase))
        {
            cutscene.Function = "none";
            cutscene.Video = "";
            return true;
        }

        if (keyword.Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            cutscene.Video = cutscene.Function = cutscene.SoundName = "";
            cutscene.SoundId = cutscene.FramesPerSec = 0;
            return true;
        }

        return false;
    }

    public static void BeginClusterParse(int clusterNumber, ClusterDef target)
    {
        target.Reset();
        target.Cluster = clusterNumber;
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
