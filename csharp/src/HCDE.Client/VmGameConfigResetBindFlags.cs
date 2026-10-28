namespace HCDE.Client;

/// <summary><c>ResetBinds</c> / <c>bResetBindFlags</c> from <c>gameconfigfile.cpp</c>.</summary>
public static class VmGameConfigResetBindFlags
{
    public const int V226GamePad = 1 << 0;
    public const int V230Gamma = 1 << 1;

    public static void QueueFromLastRunVersion(double lastRunVersion, ref int flags)
    {
        if (lastRunVersion < 226 || lastRunVersion == 228) flags |= V226GamePad;
        if (lastRunVersion < 230) flags |= V230Gamma;
    }
}
