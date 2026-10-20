namespace HCDE.Client;

/// <summary><c>D_DoomMain_Internal</c> restart-loop class init from <c>d_main.cpp</c> (4658–4664).</summary>
public static class VmDDoomMainLoopIter
{
    public static bool RestartConsoleWhenRestart(bool restart) => restart;

    public static int RestartScreenWidth() => 640;

    public static int RestartScreenHeight() => 480;

    public static bool InitConsoleFullscreen(bool restart) => restart;

    public static bool LoopIterationBegin() => true;

    public static bool AfterPClassStaticInit() => true;

    public static bool AfterPTypeStaticInit() => true;

    public static bool VmDebugFromCvar(bool vmDebug) => vmDebug;

    public static bool DebugPortOverrides(bool hasArg) => hasArg;
}
