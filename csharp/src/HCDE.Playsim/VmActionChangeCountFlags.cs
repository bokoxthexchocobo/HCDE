namespace HCDE.Playsim;

/// <summary><c>A_ChangeCountFlags</c> flag toggles from <c>p_actionfunctions.cpp</c> (2805–2821).</summary>
public static class VmActionChangeCountFlags
{
    public const int MfCountkill = 1 << 12;
    public const int MfCountitem = 1 << 15;
    public const int Mf5Countsecret = 1 << 0;

    public static int ApplyTriStateFlag(int flags, int mask, int value) =>
        value == -1 ? flags : value == 0 ? flags & ~mask : flags | mask;

    public static bool CountsAsKill(bool countsAsKill, int health) =>
        countsAsKill && health > 0;
}
