namespace HCDE.Client;

/// <summary><c>CUSTOM_CVAR(wipetype)</c> from <c>d_main.cpp</c> (485–488).</summary>
public static class VmDMainWipePolicy
{
    public const int WipeNone = 0;
    public const int DefaultWipeMelt = 1;
    public const int WipeCount = 5;

    public static int Clamp(int value) =>
        VmDMainCvarPolicy.ClampWipeType(value, WipeNone, WipeCount, DefaultWipeMelt);

    public static bool IsValid(int value) => value >= WipeNone && value < WipeCount;
}
