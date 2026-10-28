namespace HCDE.Playsim;

/// <summary><c>ChangeSpy</c> from <c>g_game.cpp</c> (1208–1281).</summary>
public static class VmGGameChangeSpy
{
    public const int SpyNext = 1;
    public const int SpyPrev = -1;
    public const int SpyCancel = 0;
    public const int MaxPlayersMask = 8 - 1;
    public const int Df2DisallowSpying = 0x00000040;

    public static bool ShouldAbortWhenNotInLevel(int gameState, int gsLevel) => gameState != gsLevel;

    public static bool ShouldRevertCameraWhenNoPlayer(bool demoPlayback) => !demoPlayback;

    public static bool IsSpyingDisallowed(int dmflags2) => (dmflags2 & Df2DisallowSpying) != 0;

    public static int ResolveStartPlayerIndex(int consolePlayer, int? cameraPlayerIndex) =>
        cameraPlayerIndex ?? consolePlayer;

    public static int StepSpyIndex(int index, int step) => (index + step) & MaxPlayersMask;

    public static int ResolveSpyStep(int changespy) =>
        changespy == SpyNext ? 1 : changespy == SpyPrev ? -1 : 0;

    public static bool ShouldContinueSpySearch(int candidate, int consolePlayer) => candidate != consolePlayer;

    public static bool IsEligibleSpyTarget(
        bool inGame,
        bool checkTeam,
        bool isTeammate,
        bool botAllowSpy,
        bool isBot) =>
        inGame && (!checkTeam || isTeammate || (botAllowSpy && isBot));

    public static bool ShouldCheckTeam(bool demoPlayback, bool deathmatch) =>
        !demoPlayback && deathmatch;

    public static bool ShouldShowPlayerName(bool demoPlayback, bool multiplayer) =>
        demoPlayback || multiplayer;

    public static int SelectCameraPlayer(int consolePlayer, int candidate) => candidate;
}
