namespace HCDE.Playsim;

/// <summary>
/// `getRealLightmode`, cutscene creation, and deferred new-game setup from <c>g_level.cpp</c> (**151–300**).
/// </summary>
public static class VmGLevelLightAndDefer1011Slice
{
    public const int LightModeNotSet = -1;
    public const int LightModeDoom = 2;
    public const int LightModeZdoomSoftware = 8;
    public const int LightModeDoomSoftware = 16;
    public const int GlMapLightModeInactive = -1;
    public const int GlLightModeSoftwareAccurate = 1;
    public const int GlLightModeVanillaAccurate = 2;
    public const int GaNewGame2 = 3;
    public const int GaIntermission = 18;
    public const int FinishNoHub = 2;

    public enum CutsceneContentKind
    {
        None,
        Function,
        Video,
    }

    public static int ResolveRealLightMode(
        int levelInfoLightMode,
        int glMapLightMode,
        int glLightMode,
        bool for3d)
    {
        if (levelInfoLightMode != LightModeNotSet)
            return levelInfoLightMode;
        if (glMapLightMode != GlMapLightModeInactive)
            return glMapLightMode;
        if (!for3d)
            return LightModeDoom;
        if (glLightMode == GlLightModeSoftwareAccurate)
            return LightModeZdoomSoftware;
        if (glLightMode == GlLightModeVanillaAccurate)
            return LightModeDoomSoftware;
        return LightModeDoom;
    }

    public static CutsceneContentKind ClassifyCutsceneDefinition(string? function, string? video)
    {
        if (!string.IsNullOrEmpty(function)
            && string.Equals(function, "none", StringComparison.OrdinalIgnoreCase))
            return CutsceneContentKind.None;
        if (!string.IsNullOrEmpty(function))
            return CutsceneContentKind.Function;
        if (!string.IsNullOrEmpty(video))
            return CutsceneContentKind.Video;
        return CutsceneContentKind.None;
    }

    public static bool ValidateCutsceneFunctionArity(int argumentCount, int runnerOnlyArity, int runnerAndMapArity) =>
        argumentCount == runnerOnlyArity || argumentCount == runnerAndMapArity;

    public static bool ShouldAssignStartupPlayerClass(bool hasPlayerClass, string? startupClass) =>
        hasPlayerClass && !string.IsNullOrEmpty(startupClass);

    public static bool ShouldRunEpisodeIntro(bool introDefined, bool cutsceneCreated) =>
        introDefined && cutsceneCreated;

    public static bool ShouldAbortEpisodeIntroOnCutsceneFailure(bool introDefined, bool cutsceneCreated) =>
        introDefined && !cutsceneCreated;

    public static bool ShouldClearCutsceneCompletionAfterInvalidScreenJob(
        bool screenJobValid,
        bool hadCompletionCallback) =>
        !screenJobValid && hadCompletionCallback;

    public static int ResolveFinishStateForNewGameStartup() => FinishNoHub;

    public static int ResolveDeferredNewGameTraceAction() => GaNewGame2;
}
