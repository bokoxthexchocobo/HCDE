namespace HCDE.Client;

/// <summary>Texture hacks, GC roots, and HUD scale hooks from <c>d_main.cpp</c> (3646–3796).</summary>
public static class VmDMainTextureGc
{
    public const int GameHexen = 2;
    public const int GameHeretic = 3;
    public const int GameDoom = 1;
    public const int GiMapXx = 1;
    public const int MaxPlayers = 64;

    public static bool PatchTexturesApplies(int gametype) => gametype == GameHexen;

    public static bool PatchBlankWhenExists(bool exists) => exists;

    public static bool CheckForHacksRejectEmptyParts(int partCount) => partCount == 0;

    public static bool HereticSkyHack(string name, int height, int partCount) =>
        name.Length == 4 && name[0] == 'S' && name[1] == 'K' && name[2] == 'Y' &&
        name[3] is >= '1' and <= '3' && height == 128 && partCount == 1;

    public static bool DoomSkyHack(string name, int height, int originY, int flags, int gametype) =>
        gametype == GameDoom && (flags & GiMapXx) == 0 && name.Length == 4 &&
        height == 128 && originY == -8 && name.StartsWith("SKY1", StringComparison.Ordinal);

    public static bool BigDoorHack(string name, int height, int originY, int partCount, int gametype, int flags) =>
        gametype == GameDoom && (flags & GiMapXx) == 0 &&
        string.Equals(name, "BIGDOOR7", StringComparison.OrdinalIgnoreCase) &&
        partCount == 2 && height == 128 && originY == -4;

    public static bool FixWideStatusBar(int width, double scaleX, int leftOffset, int topOffset) =>
        width > 320 && scaleX == 1 && leftOffset == 0 && topOffset == 0;

    public static int WideBarOffset(int width) => (width - 320) / 2;

    public static string SpriteNameFromId(uint id, int spriteCount, string fallback) =>
        id >= (uint)spriteCount ? fallback : id.ToString();

    public static bool GcMarkPlayerInGame(bool inGame) => inGame;

    public static bool GcMarkNextThink(object? thinker) => thinker != null;

    public static int GameActionFullConsole() => 1;

    public static bool HudScaleNeedsResize(object? statusBar) => statusBar != null;

    public static bool HudScaleRefreshAutomap(bool hasLevel, bool hasAutomap) => hasLevel && hasAutomap;

    public static int WipeBurnGameState() => 1;

    public static int WipeFadeGameState() => 2;

    public static int WipeMeltGameState() => 3;

    public static int SelectWipeState(int type) =>
        type switch
        {
            1 => WipeBurnGameState(),
            2 => WipeFadeGameState(),
            _ => WipeMeltGameState(),
        };

    public static bool FileNameCheckRejectTextures(string baseName) =>
        baseName.StartsWith("textures", StringComparison.OrdinalIgnoreCase);

    public static bool FileNameCheckLz4Header(ReadOnlySpan<byte> header) =>
        header.Length >= 3 && header[0] == (byte)'L' && header[1] == (byte)'Z' && header[2] == (byte)'4';

    public static bool FileNameCheckAllow(bool reject) => !reject;

    public static bool SystemStartCutscene(bool blockUi) => blockUi;

    public static int GameActionIntermission() => 2;

    public static bool CheckSkipGameOption(string name) => !string.IsNullOrEmpty(name);

    public static int CastSpriteFallbackId() => 0;
}
