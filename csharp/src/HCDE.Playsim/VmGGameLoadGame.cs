namespace HCDE.Playsim;

/// <summary><c>G_LoadGame</c> and save WAD checks from <c>g_game.cpp</c> (2515–2570).</summary>
public static class VmGGameLoadGame
{
    public const int GaLoadGame = 1;
    public const int GaLoadGameHideCon = 2;

    public static bool ShouldSetSaveName(string? name) => name != null;

    public static int ResolveLoadGameAction(bool hideConsole) =>
        hideConsole ? GaLoadGameHideCon : GaLoadGame;

    public static bool CheckSingleWadLoaded(int checkResult) => checkResult >= 0;

    public static bool ShouldPrintRequiresHeader(bool printRequires) => !printRequires;

    public static bool ShouldValidateSecondWad(string? gameWad, string? mapWad) =>
        gameWad != null && mapWad != null
        && !string.Equals(gameWad, mapWad, StringComparison.OrdinalIgnoreCase);

    public static bool SaveGameWadsComplete(bool printRequires) => !printRequires;
}
