namespace HCDE.Client;

/// <summary><c>RenameSprites</c> filesystem loop tail from <c>d_main.cpp</c> (3058–3083).</summary>
public static class VmDMainRenameSpritesIo
{
    public const uint BlodId = 0x444F4C42; // BLOD
    public const uint BludId = 0x44554C42; // BLUD
    public const int GameDoomChexMask = 1; // GAME_DoomChex bit in native gametype flags

    public static bool ShouldRenameBlodToBlud(int gametypeFlags, uint spriteDword) =>
        (gametypeFlags & GameDoomChexMask) == 0 && spriteDword == BlodId;

    public static uint RenameBlodSprite(uint spriteDword) => BludId;

    public static bool ShouldClearDeletableLump(
        int fileNamespace,
        int globalNs,
        int container,
        int iwadNum,
        int maxIwadNum,
        string lumpName,
        IReadOnlyList<string> deleteLumps) =>
        fileNamespace == globalNs
        && container >= iwadNum
        && container <= maxIwadNum
        && deleteLumps.Contains(lumpName);

    public static string ClearLumpName() => string.Empty;

    public static bool ShouldRenameBigFontLump(string lumpName, string altBigFont) =>
        string.Equals(lumpName, altBigFont, StringComparison.Ordinal);

    public static string BigFontLumpName() => "BIGFONT";
}
