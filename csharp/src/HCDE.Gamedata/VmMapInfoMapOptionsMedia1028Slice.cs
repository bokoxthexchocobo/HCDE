namespace HCDE.Gamedata;

/// <summary>
/// MAPINFO media/scalar DEFINE options from <c>g_mapinfo.cpp</c> (**1191–1340**).
/// </summary>
public static class VmMapInfoMapOptionsMedia1028Slice
{
    public const int Level3HideAuthorName = 1 << 0;

    public sealed class LevelScalarsDraft
    {
        public int Flags3 { get; set; }
        public double SkyMistyScale { get; set; }
        public string SkyPic1 { get; set; } = "";
        public double SkySpeed1 { get; set; }
        public int Partime { get; set; }
        public int InvasionWaveLimit { get; set; }
        public int DuelLimit { get; set; }
        public int SuckTime { get; set; }
        public int CdTrack { get; set; }
        public uint CdId { get; set; }
        public int WarpTrans { get; set; }
        public int WallVertLight { get; set; }
        public int WallHorizLight { get; set; }
    }

    public static double ClampSkyMistYScale(double value) => Math.Clamp(value, 0.002, 544.0);

    public static int ClampWaveLimit(int value) => Math.Clamp(value, 0, 3000);

    public static int ClampDuelLimit(int value) => Math.Clamp(value, 0, 255);

    public static sbyte ClampWallShade(int rawHalfSteps) =>
        (sbyte)Math.Clamp(rawHalfSteps / 2, -128, 127);

    public static int ApplyTitlePatchFlags(int flags3, bool newFormat, bool hasTrailingComma, int trailingNumber)
    {
        if (!newFormat || !hasTrailingComma)
            return flags3;
        return trailingNumber != 0 ? flags3 | Level3HideAuthorName : flags3 & ~Level3HideAuthorName;
    }

    public static void StoreMapInterMusic(
        Dictionary<string, (string Music, int Order)> table,
        string mapName,
        string music,
        int order) =>
        table[mapName] = (music, order);

    public static void ApplyEvenLighting(LevelScalarsDraft draft)
    {
        draft.WallVertLight = 0;
        draft.WallHorizLight = 0;
    }

    public static bool TryParseCdIdHex(string token, out uint cdId) =>
        uint.TryParse(token, System.Globalization.NumberStyles.HexNumber, null, out cdId);

    public static bool TryApplyOption(
        string keyword,
        LevelScalarsDraft draft,
        bool newFormat,
        bool hasComma,
        int trailingNumber,
        string stringValue,
        int intValue,
        double floatValue)
    {
        switch (keyword.ToLowerInvariant())
        {
            case "skymistyscale":
                draft.SkyMistyScale = ClampSkyMistYScale(floatValue);
                return true;
            case "skybox":
                draft.SkyPic1 = stringValue;
                draft.SkySpeed1 = 0;
                return true;
            case "partime":
            case "par":
                draft.Partime = intValue;
                return true;
            case "wavelimit":
                draft.InvasionWaveLimit = ClampWaveLimit(intValue);
                return true;
            case "duellimit":
                draft.DuelLimit = ClampDuelLimit(intValue);
                return true;
            case "sucktime":
                draft.SuckTime = intValue;
                return true;
            case "cdtrack":
                draft.CdTrack = intValue;
                return true;
            case "cdid":
                if (TryParseCdIdHex(stringValue, out var id))
                    draft.CdId = id;
                return true;
            case "warptrans":
                draft.WarpTrans = intValue;
                return true;
            case "vertwallshade":
                draft.WallVertLight = ClampWallShade(intValue);
                return true;
            case "horizwallshade":
                draft.WallHorizLight = ClampWallShade(intValue);
                return true;
            case "titlepatch":
                draft.Flags3 = ApplyTitlePatchFlags(draft.Flags3, newFormat, hasComma, trailingNumber);
                return true;
            case "evenlighting":
                ApplyEvenLighting(draft);
                return true;
            default:
                return false;
        }
    }
}
