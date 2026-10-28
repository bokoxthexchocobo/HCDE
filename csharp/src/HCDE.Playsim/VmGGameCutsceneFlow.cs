namespace HCDE.Playsim;

/// <summary>Cutscene tick/advance helpers from <c>g_game.cpp</c> (1414–1473).</summary>
public static class VmGGameCutsceneFlow
{
    public const int StUnskippable = 2;

    public static bool ShouldFinishSinglePlayerScreenJob(bool netgame, bool demoplayback, bool screenJobFinished) =>
        screenJobFinished && !demoplayback && !netgame;

    public static bool ShouldAuthorityEndScreenJob(bool isLocalAuthority, int skipType) =>
        isLocalAuthority && skipType == StUnskippable;

    public static bool ShouldAdvanceCutsceneWhenReady(bool netgame, bool demoplayback, bool ready) =>
        netgame && !demoplayback && ready;

    public static bool IsCutsceneGameState(int gameState, int gsCutscene, int gsIntro) =>
        gameState == gsCutscene || gameState == gsIntro;

    public static bool ShouldTickStalledRecovery(bool netgame, bool demoplayback) =>
        netgame && !demoplayback;

    public static bool ShouldDoWorldDoneWhenQueued(int gameAction, int gaWorldDone) =>
        gameAction == gaWorldDone;
}
