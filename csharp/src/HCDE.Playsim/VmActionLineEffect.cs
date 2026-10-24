namespace HCDE.Playsim;

/// <summary><c>A_LineEffect</c> and <c>A_WolfAttack</c> early guards from <c>p_actionfunctions.cpp</c> (3558–3602).</summary>
public static class VmActionLineEffect
{
    public const int Mf6Linedone = 1 << 0;

    public static bool SkipWhenLinedone(int flags6) => (flags6 & Mf6Linedone) != 0;

    public static bool HasLineSpecial(int special) => special != 0;

    public static bool MarkLinedoneWhenNonRepeating(bool executed, bool repeatSpecial) =>
        executed && !repeatSpecial;

    public static bool WolfAttackAbortsWithoutTarget(object? target) => target is null;

    public static bool WolfAttackAbortsWithoutSight(bool sightOk) => !sightOk;

    public static bool LineEffectCanRun(int flags6, int special) =>
        !SkipWhenLinedone(flags6) && HasLineSpecial(special);

    public static int ApplyLinedoneFlag(int flags6, bool executed, bool repeatSpecial) =>
        MarkLinedoneWhenNonRepeating(executed, repeatSpecial) ? flags6 | Mf6Linedone : flags6;
}
