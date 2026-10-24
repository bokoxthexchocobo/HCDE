namespace HCDE.Client;

/// <summary>Startup status helpers from <c>d_main.cpp</c> (2802–2842).</summary>
public static class VmDMainStartupProgress
{
    public static bool ShouldSkipStartupUi(bool batchRun) => batchRun;

    public static bool ShouldUpdateStartScreen(bool startScreenPresent) => startScreenPresent;

    public static bool ShouldShowPreVideoStatus(bool batchRun, bool dedicatedServer) =>
        !batchRun && !dedicatedServer;

    public static string NormalizeStartupTraceText(string? text) => text ?? string.Empty;

    public static bool ShouldAdvanceProgress(int advance) => advance > 0;

    public static bool ShouldTickStartWindow(bool startWindowPresent) => startWindowPresent;
}
