namespace HCDE.Client;

/// <summary><c>dmflags3</c>, <c>compatflags</c>, and <c>compatflags2</c> callbacks from <c>d_main.cpp</c> (803–839).</summary>
public static class VmDMainCompatCallbacks
{
    public const int Df3NoPlayerClip = 1;
    public const int Df3CoopShareKeys = 2;

    public static bool ShouldApplyCompatibilityOnCompatFlagsChange(int newValue, int oldValue) =>
        newValue != oldValue;

    public static bool ShouldApplyCompatibility2OnChange(int newValue, int oldValue) =>
        newValue != oldValue;

    public static bool ShouldSetCompatLineOnSide(bool onCompatFlags2Change) => onCompatFlags2Change;

    public static bool IsDmflags3FlagSet(int dmflags3, int flag) => (dmflags3 & flag) != 0;
}
