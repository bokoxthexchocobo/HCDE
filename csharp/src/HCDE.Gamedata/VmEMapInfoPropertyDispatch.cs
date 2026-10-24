namespace HCDE.Gamedata;

public sealed class VmEMapInfoLevelDraft
{
    public string MapName { get; set; } = "";
    public string LevelName { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string NextMap { get; set; } = "";
    public string NextSecretMap { get; set; } = "";
    public string EdName { get; set; } = "";
    public string AcsName { get; set; } = "";
    public int Flags { get; set; }
    public double AirControl { get; set; }
    public double Gravity { get; set; } = double.MaxValue;
    public string CustomColorMap { get; set; } = "";
    public int Partime { get; set; }
    public string Music { get; set; } = "";
    public string InterMusic { get; set; } = "";
    public string SkyPic1 { get; set; } = "";
    public string SkyPic2 { get; set; } = "";
    public float SkySpeed1 { get; set; }
    public float SkySpeed2 { get; set; }
}

/// <summary><c>ResolveSiblingLump</c> tail and <c>ApplyEMapInfoProperty</c> from <c>hcde_emapinfo.cpp</c> (539–663).</summary>
public static class VmEMapInfoPropertyDispatch
{
    public static bool ShouldTryShortLumpName(string name) =>
        name.Length <= 8 && !VmEMapInfoLineParser.HasPathOrExtension(name);

    public static bool TryApplyLevelName(string key, string value, VmEMapInfoLevelDraft draft)
    {
        if (key is not ("levelname" or "inter-levelname")) return false;
        draft.LevelName = value;
        return true;
    }

    public static bool TryApplyGravity(
        VmEMapInfoStats stats,
        string mapName,
        string key,
        string value,
        int line,
        VmEMapInfoLevelDraft draft)
    {
        if (key != "gravity") return false;
        if (!VmEMapInfoLineParser.TryParseInt(value, out var raw))
        {
            VmEMapInfoApplyHelpers.RecordInvalid(stats, mapName, key, line);
            return true;
        }

        draft.Gravity = raw == 0 ? double.MaxValue : raw / 65536.0;
        return true;
    }

    public static bool TryApplyAirControl(
        VmEMapInfoStats stats,
        string mapName,
        string key,
        string value,
        int line,
        VmEMapInfoLevelDraft draft)
    {
        if (key != "aircontrol") return false;
        if (!VmEMapInfoLineParser.TryParseInt(value, out var raw))
        {
            VmEMapInfoApplyHelpers.RecordInvalid(stats, mapName, key, line);
            return true;
        }

        draft.AirControl = raw == 0 ? double.Epsilon : raw / 65536.0;
        return true;
    }

    public static bool TryApplyMusic(VmEMapInfoLevelDraft draft, string key, string value, VmEMapInfoGameType game)
    {
        if (key == "music")
        {
            draft.Music = VmEMapInfoMusicNormalize.NormalizeMusicName(value, game);
            return true;
        }

        if (key == "intermusic")
        {
            draft.InterMusic = VmEMapInfoMusicNormalize.NormalizeMusicName(value, game);
            return true;
        }

        return false;
    }

    public static bool TryApplySkyName(VmEMapInfoLevelDraft draft, string key, string value)
    {
        if (key == "skyname") { draft.SkyPic1 = value.ToUpperInvariant(); return true; }
        if (key == "sky2name") { draft.SkyPic2 = value.ToUpperInvariant(); return true; }
        return false;
    }

    public static bool TryApplySkyDelta(
        VmEMapInfoStats stats,
        string mapName,
        string key,
        string value,
        int line,
        VmEMapInfoLevelDraft draft)
    {
        if (key is not ("skydelta" or "sky2delta")) return false;
        if (!double.TryParse(value, out var delta))
        {
            VmEMapInfoApplyHelpers.RecordInvalid(stats, mapName, key, line);
            return true;
        }

        var speed = VmEMapInfoPropertyTail.SkySpeedFromDelta(delta);
        if (key == "skydelta") draft.SkySpeed1 = speed;
        else draft.SkySpeed2 = speed;
        return true;
    }

    public static bool TryApplyBoolMapFlag(
        VmEMapInfoStats stats,
        string mapName,
        string key,
        string value,
        int line,
        ref int flags,
        int flagBit)
    {
        if (key is not ("doublesky" or "lightning" or "noautosequences" or "killstats"))
            return false;

        var enabled = false;
        if (!VmEMapInfoApplyHelpers.ApplyBoolProperty(stats, mapName, key, value, line, ref enabled))
            return true;
        VmEMapInfoParsePrimitives.SetFlag(ref flags, flagBit, enabled);
        return true;
    }

    public static bool TryApplyDisableJump(
        VmEMapInfoStats stats,
        string mapName,
        string key,
        string value,
        int line,
        ref int flags)
    {
        if (key != "disable-jump") return false;
        var enabled = false;
        if (!VmEMapInfoApplyHelpers.ApplyBoolProperty(stats, mapName, key, value, line, ref enabled))
            return true;
        VmEMapInfoParsePrimitives.SetFlag(ref flags, 0x80, enabled);
        return true;
    }
}
