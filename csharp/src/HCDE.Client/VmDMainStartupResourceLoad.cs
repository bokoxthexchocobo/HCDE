namespace HCDE.Client;

/// <summary><c>D_InitGame</c> resource and playloop startup from <c>d_main.cpp</c> (4166–4320).</summary>
public static class VmDMainStartupResourceLoad
{
    public static bool ParseTeamInfoAfterTranslations(bool ok) => ok;

    public static bool ActorStaticInitAfterTranslate(bool ok) => ok;

    public static bool FatalWhenNoPlayerClasses(int count) => count == 0;

    public static string NoPlayerClassesMessage() => "No player classes defined";

    public static bool LoadWadSettingsWhenStartScreen(bool hasScreen) => hasScreen;

    public static bool ProgressAfterGlDefs(bool ok) => ok;

    public static string RenderInitBanner(string configName) =>
        $"R_Init: Init {configName} refresh subsystem.";

    public static bool ReadDecalsWhenNotBatch(bool batch) => !batch;

    public static bool LockLocalSndWhenDehAdded(int baseSounds, int afterDeh) => baseSounds < afterDeh;

    public static bool InitMenusWhenNotBatch(bool batch) => !batch;

    public static bool RemoveUnusedSymbolsWhenNotDebugging(bool debug) => !debug;

    public static bool InitActorNumsFromMapinfo(bool ok) => ok;

    public static bool ApplyEdfActorMappings(bool ok) => ok;

    public static int BotArgCount(int count) => count;

    public static bool PushBotSpawnName(bool ok) => ok;

    public static bool PlayloopInit(bool ok) => ok;

    public static bool SbarInfoLoad(bool ok) => ok;

    public static bool PrintStartupStringsWhenNotBatch(bool batch) => !batch;

    public static bool StartupStringNonEmpty(string? str) => !string.IsNullOrEmpty(str);

    public static bool CheckNetGameWhenNotRestart(bool restart) => !restart;

    public static bool EnableNoSetCvarsBeforePlay(bool ok) => ok;

    public static bool LateVideoInitWhenNeeded(bool restart, bool norun, bool dedicated, bool videoInitialized) =>
        !(restart || norun || dedicated) && !videoInitialized;

    public static bool AllocateScreenWhenClient(bool restart, bool norun, bool dedicated) =>
        !(restart || norun || dedicated);

    public static bool ConsiderDehPatches(bool foundDeh, bool foundBex) => foundDeh || foundBex;

    public static bool DefaultDehWhenDoom(int gameType, bool foundDeh, bool foundBex) =>
        gameType == 1 && !foundDeh && !foundBex;

    public static bool ParseOptionsAfterDeh(bool ok) => ok;

    public static bool FinishDehPatch(bool ok) => ok;

    public static int LoadingPhaseActorDefs() => 0;

    public static int LoadingPhaseKeyBindings() => 1;

    public static int LoadingPhaseGlDefs() => 2;

    public static bool StartScreenProgress(bool hasScreen) => hasScreen;

    public static bool StartWindowProgress(bool hasWindow) => hasWindow;

    public static bool LoadingStatusGraphics(bool hasScreen) => hasScreen;

    public static bool SetLoadingPhaseActor(bool hasScreen) => hasScreen;

    public static bool SetLoadingPhaseKeys(bool hasScreen) => hasScreen;

    public static bool SetLoadingPhaseGl(bool hasScreen) => hasScreen;

    public static bool InitZsCallbacks(bool ok) => ok;

    public static bool JobInit(bool ok) => ok;

    public static bool SetupPlayerClasses(bool ok) => ok;

    public static bool ClearBotSpawned(bool ok) => ok;

    public static int WantedBotNum(int spawned) => spawned;

    public static bool AmStaticInit(bool ok) => ok;

    public static bool PInitPlayloop(bool ok) => ok;

    public static bool SetupWeaponsNtoHton(bool ok) => ok;

    public static bool SbarInfoLoadRequired(bool ok) => ok;

    public static int StartupStringIndex(int p) => p;

    public static bool EnableNoSetBeforePlay(bool ok) => ok;

    public static bool GuardClientSubsystemForVideo(bool ok) => ok;

    public static bool FirstShaderCompileNeeded(bool compiled) => !compiled;

    public static bool SetModeNeeded(bool flag) => flag;
}
