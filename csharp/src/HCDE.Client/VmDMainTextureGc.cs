namespace HCDE.Client;

/// <summary>Texture hacks, GC roots, and HUD scale hooks from <c>d_main.cpp</c> (3646–3818).</summary>
public static class VmDMainTextureGc
{
    public const int GameHexen = 2;
    public const int GameHeretic = 3;
    public const int GameDoom = 1;
    public const int GiMapXx = 1;

    public enum TextureHackKind
    {
        None,
        HexenBlank,
        HereticSky,
        DoomSky1,
        BigDoor7,
    }

    public readonly record struct BuildInfoHackInput(
        int GameType,
        int Flags,
        string Name,
        int Height,
        int PartCount,
        int OriginY);

    public static TextureHackKind ClassifyBuildInfo(BuildInfoHackInput input)
    {
        if (input.PartCount == 0)
            return TextureHackKind.None;
        if (input.GameType == GameHeretic && input.Name.Length == 4 &&
            input.Name[0] == 'S' && input.Name[1] == 'K' && input.Name[2] == 'Y' &&
            input.Name[3] is >= '1' and <= '3' && input.Height == 128 && input.PartCount == 1)
            return TextureHackKind.HereticSky;
        if (input.GameType == GameDoom && (input.Flags & GiMapXx) == 0 && input.Name.Length == 4 &&
            input.Height == 128 && input.PartCount == 1 && input.OriginY == -8 &&
            input.Name[0] == 'S' && input.Name[1] == 'K' && input.Name[2] == 'Y' && input.Name[3] == '1')
            return TextureHackKind.DoomSky1;
        if (input.GameType == GameDoom && (input.Flags & GiMapXx) == 0 &&
            string.Equals(input.Name, "BIGDOOR7", StringComparison.OrdinalIgnoreCase) &&
            input.PartCount == 2 && input.Height == 128 && input.OriginY == -4)
            return TextureHackKind.BigDoor7;
        return TextureHackKind.None;
    }

    public static int ApplyHereticSkyHeight(int partImageHeight) => partImageHeight;

    public static int ApplyDoomSkyOriginY() => 0;

    public static int ApplyBigDoorOriginY() => 0;

    public static bool PatchTexturesApplies(int gametype) => gametype == GameHexen;

    public static bool PatchBlankWhenExists(bool exists) => exists;

    public readonly record struct WideStatusBarPlan(bool Apply, int Offset0, int Offset1);

    public static WideStatusBarPlan PlanWideStatusBar(int width, double scaleX, int left, int top) =>
        width > 320 && scaleX == 1 && left == 0 && top == 0
            ? new(true, (width - 320) / 2, (width - 320) / 2)
            : new(false, 0, 0);

    public static string CastSpriteName(uint id, int spriteCount) =>
        id >= (uint)spriteCount ? "TNT1" : id.ToString();

    public static bool ShouldMarkPlayer(bool inGame) => inGame;

    public static bool ShouldMarkNextThink(object? thinker) => thinker != null;

    public static int GameActionFullConsole() => 1;

    public static bool HudScaleNeedsResize(object? statusBar) => statusBar != null;

    public static bool HudScaleRefreshAutomap(bool hasLevel, bool hasAutomap) => hasLevel && hasAutomap;

    public static int SelectWipeGameState(int type) => type switch
    {
        1 => 1,
        2 => 2,
        _ => 3,
    };

    public static bool FileNameRejectLz4Textures(string baseName, ReadOnlySpan<byte> header) =>
        baseName.StartsWith("textures", StringComparison.OrdinalIgnoreCase) &&
        header.Length >= 3 && header[0] == (byte)'L' && header[1] == (byte)'Z' && header[2] == (byte)'4';

    public static bool FileNameCheckAllow(bool rejectLz4) => !rejectLz4;

    public static bool SystemStartCutsceneBlockUi(bool block) => block;

    public static int WipeTypeToGameState(int wipeType) => SelectWipeGameState(wipeType);

    public static bool CheckForHacksEarlyOut(int partCount) => partCount == 0;

    public static bool HexenBlankNullType(int gametype, bool texExists) =>
        gametype == GameHexen && texExists;

    public static int HereticSkyTargetHeight(int imageHeight) => imageHeight;

    public static void ApplyBuildInfoHack(TextureHackKind kind, ref int height, ref int originY)
    {
        switch (kind)
        {
            case TextureHackKind.HereticSky:
                height = ApplyHereticSkyHeight(height);
                break;
            case TextureHackKind.DoomSky1:
                originY = ApplyDoomSkyOriginY();
                break;
            case TextureHackKind.BigDoor7:
                originY = ApplyBigDoorOriginY();
                break;
        }
    }

    public static bool GcMarkEventHandlers(object? first, object? last) => first != null || last != null;

    public static bool GcMarkAllLevels(bool anyLevels) => anyLevels;

    public static int GcMarkPlayerCount(int count) => count;

    public static bool HudScaleSetsSizeNeeded(bool hasBar) => hasBar;

    public static bool AutomapNewUiScale(bool hasAutomap) => hasAutomap;
}
