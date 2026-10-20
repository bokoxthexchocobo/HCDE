namespace HCDE.Client;

/// <summary><c>CUSTOM_CVAR(fraglimit)</c> from <c>d_main.cpp</c> (466–482).</summary>
public static class VmDMainFragLimitPolicy
{
    public static bool ShouldCheckFragLimit(bool deathmatch, int fragLimit) =>
        deathmatch && fragLimit > 0;

    public static bool ShouldExitLevel(int fragLimit, int playerFragCount) =>
        fragLimit > 0 && playerFragCount >= fragLimit;
}
