namespace HCDE.Playsim;

/// <summary>Input trace formatting from <c>G_BuildTiccmd</c> in <c>g_game.cpp</c> (1118–1127).</summary>
public static class VmGGameInputTrace
{
    public static string FormatInputTrace(
        int gametic,
        int forward,
        int side,
        int up,
        uint buttons) =>
        $"tic={gametic} fwd={forward} side={side} up={up} buttons=0x{buttons:x8}";

    public static bool ShouldEmitTrace(int forward, int side, int up, uint buttons) =>
        forward != 0 || side != 0 || up != 0 || buttons != 0;

    public static int AdvanceTraceTic(int gametic) => gametic;
}
