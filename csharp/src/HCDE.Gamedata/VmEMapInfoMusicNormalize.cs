namespace HCDE.Gamedata;

public enum VmEMapInfoGameType
{
    Other,
    Doom,
    Chex,
    Heretic
}

/// <summary><c>NormalizeMusicName</c> from <c>hcde_emapinfo.cpp</c> (351–375).</summary>
public static class VmEMapInfoMusicNormalize
{
    public static string NormalizeMusicName(string value, VmEMapInfoGameType gameType)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        var lower = value.ToLowerInvariant();
        if (value[0] == '$' || value.Contains('/') || value.Contains('\\') || value.Contains('.') ||
            lower.StartsWith("d_") || lower.StartsWith("mus_"))
            return value;

        var upper = value.ToUpperInvariant();
        if (gameType is VmEMapInfoGameType.Doom or VmEMapInfoGameType.Chex)
            return $"D_{upper}";
        if (gameType is VmEMapInfoGameType.Heretic)
            return $"MUS_{upper}";
        return value;
    }
}
