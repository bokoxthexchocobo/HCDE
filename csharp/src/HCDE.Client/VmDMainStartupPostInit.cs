namespace HCDE.Client;

/// <summary><c>D_InitGame</c> post-WAD startup from <c>d_main.cpp</c> (3997–4161).</summary>
public static class VmDMainStartupPostInit
{
    public const int GameHexen = 2;
    public const int GameHeretic = 3;
    public const int GameStrife = 4;

    public enum StartupScreenKind
    {
        Default,
        Hexen,
        Heretic,
        Strife,
    }

    public static StartupScreenKind ResolveStartupKind(StartupScreenKind current, int gameType) =>
        current != StartupScreenKind.Default
            ? current
            : gameType switch
            {
                GameHexen => StartupScreenKind.Hexen,
                GameHeretic => StartupScreenKind.Heretic,
                GameStrife => StartupScreenKind.Strife,
                _ => StartupScreenKind.Default,
            };

    public static int TextureProgressHeadroom(int texGuess) =>
        texGuess > 0 ? Math.Max(texGuess * 4, 4096) : 4096;

    public static bool NorunFromArgs(bool norun) => norun;

    public static bool NoStartScreen(
        bool batch,
        bool restart,
        bool join,
        bool dedicatedJoin,
        bool joindedicated,
        bool host,
        bool dedicated,
        bool norun) =>
        batch || restart || join || dedicatedJoin || joindedicated || host || dedicated || norun;

    public static bool DedicatedClearsDelayedCommands(bool dedicated) => dedicated;

    public static string DedicatedSkippedCommandsBanner(int skipped) =>
        $"Dedicated server ignored {skipped} delayed startup command{(skipped == 1 ? "" : "s")}.\n";

    public static bool ShouldPrintStInit(bool batch, bool dedicated) => !batch && !dedicated;

    public static bool RestartUsesZeroProgressWindow(bool restart) => restart;

    public static bool DedicatedUsesStartupScreen(bool dedicated, bool restart) => dedicated && !restart;

    public static bool ClientCreatesStartupScreen(bool dedicated, bool restart) => !dedicated && !restart;

    public static bool ShouldInitVideo(bool noStartScreen, bool dedicated) => !noStartScreen && !dedicated;

    public static bool CompatModeFromArg(string? value) => value != null;

    public static long ParseCompatMode(string value) =>
        long.TryParse(value, out var v) ? v : 0;

    public static bool BatchSkipsSoundBanner(bool batch) => batch;

    public static bool MapInfoAfterSoundData(bool ok) => ok;

    public static bool MusInfoAfterMapInfo(bool ok) => ok;

    public static bool TexManInitAfterCpu(bool ok) => ok;

    public static bool EnableCvarCallbacksAfterTexMan(bool ok) => ok;

    public static bool ProgressAfterPatchTextures(bool ok) => ok;

    public static bool FixWideBarAfterConback(bool ok) => ok;

    public static bool TeamInfoAfterTranslations(bool ok) => ok;

    public static string RenderRefreshBanner(string configName) =>
        $"R_Init: Init {configName} refresh subsystem.";

    public static bool DecalsAfterRenderer(bool ok) => ok;

    public static int ShaderProgressPerCompile(int maxProgress, int shaderCount) =>
        shaderCount > 0 ? maxProgress / 10 / shaderCount : 0;

    public static int StartScreenProgressBudget(int maxProgress, int perShader) =>
        perShader > 0 ? maxProgress * 10 / 9 : maxProgress + 3;
}
