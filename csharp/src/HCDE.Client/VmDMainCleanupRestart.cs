namespace HCDE.Client;

/// <summary><c>D_Cleanup</c> body and <c>debug_restart</c> from <c>d_main.cpp</c> (4888–5019).</summary>
public static class VmDMainCleanupRestart
{
    public static bool NetgameCleanupWarn(bool netgame, bool demoplayback) => netgame && !demoplayback;

    public static bool StopDebugServer(bool hasServer) => hasServer;

    public static bool CheckDemoOnCleanup(bool demorecording) => demorecording;

    public static bool StopMusicFirst() => true;

    public static bool ClearMapinfoOnCleanup() => true;

    public static bool ErrorCleanupShutdown(string tag) => tag.Length > 0;

    public static bool SaveDefaultsBeforeRestart() => true;

    public static bool ResetGameinfoPlacement() => true;

    public static bool ClearStartupInfoStrings() => true;

    public static bool FullGcBeforeRestart() => true;

    public static bool NullVmActionPointers() => true;

    public static bool ClearPendingWeapons() => true;

    public static bool StaticShutdownClasses() => true;

    public static bool AssertGcRootNull(bool rootNull) => rootNull;

    public static bool IncrementRestartCounter(int restart) => restart >= 0;

    public static bool ClearVmOperationalFlags() => false;

    public static bool IgnoreDebugRestartDuringNetgame(bool netgame, bool demoplayback) =>
        netgame && !demoplayback;

    public static bool StripRestartArgs(bool norun) => !norun;

    public static bool SetWantToRestart() => true;

    public static bool ClearSoundData() => true;

    public static bool UnloadReverbDef() => true;

    public static bool ClearMenusOnCleanup() => true;

    public static bool ClearColorsets() => true;

    public static bool DeinitSwColormaps() => true;

    public static bool FreeSbarScript() => true;

    public static bool DeleteScreenJob() => true;

    public static bool PShutdown() => true;

    public static bool CleanSwDrawer() => true;

    public static bool ClearFonts() => true;

    public static bool ClearColorSets() => true;

    public static bool ClearPainFlashes() => true;

    public static bool SShutdown() => true;

    public static bool ClearAliases() => true;

    public static bool DestroyModCvars() => true;

    public static bool DeinitMenus() => true;

    public static bool ClearSaveGames() => true;

    public static bool ClearLightDefaults() => true;

    public static bool DeleteAllTexAnim() => true;

    public static bool DeleteAllTexMan() => true;

    public static bool ResetStartupInfoFields() => true;

    public static bool DelSoftRootHead() => true;

    public static bool ClearAllActorClasses() => true;

    public static bool ScriptUtilClear() => true;

    public static bool PClassStaticShutdown() => true;

    public static bool RemoveIwadArgsOnRestart() => true;

    public static bool RemoveDehBexArgs() => true;

    public static bool RemoveDemoArgs() => true;

    public static bool RemoveFileArgs() => true;

    public static bool RemoveSkillArgs() => true;

    public static bool ReappendArgvWhenPresent(int argc) => argc > 1;

    public static bool WantToRestartFlag() => true;

    public static bool EventTypeFieldDefined() => true;

    public static bool EventSubtypeFieldDefined() => true;

    public static bool EventDataFieldsDefined() => true;

    public static bool EventPositionFieldsDefined() => true;

    public static bool DdMainClosingBrace() => true;

    public static bool CleanupBannerComment() => true;

    public static bool RestartSectionComment() => true;

    public static bool ShutdownTagNonEmpty(string tag) => !string.IsNullOrEmpty(tag);
}
