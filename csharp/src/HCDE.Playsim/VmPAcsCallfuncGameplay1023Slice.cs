namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::CallFunction</c> ACSF cases from <c>p_acs.cpp</c> (**5965–6114**):
/// named-script dispatch args, TID/math/HUD/CVar/line-attack/play-sound policy.
/// Chunk **1023**; follows chunk **1022** enum head (**4728–4877**).
/// </summary>
public static class VmPAcsCallfuncGameplay1023Slice
{
    public const int FhfNorandompuffz = 1;
    public const int FhfNoimpactdecal = 1 << 1;
    public const int LafNorandompuffz = 1;
    public const int LafNoimpactdecal = 1 << 1;
    public const int ChanBody = 5;
    public const float AttnNorm = 1f;

    public static int NamedExecuteScriptIndex(string? lookupName, Func<string, int> nameToIndex)
    {
        if (string.IsNullOrEmpty(lookupName))
            return 0;
        return -nameToIndex(lookupName);
    }

    public static int NamedExecuteOptionalArg(IReadOnlyList<int> args, int argCount, int index) =>
        argCount > index ? args[index] : 0;

    public static int UniqueTidStartArg(IReadOnlyList<int> args, int argCount) =>
        argCount > 0 ? args[0] : 0;

    public static int UniqueTidMinInUseArg(IReadOnlyList<int> args, int argCount) =>
        argCount > 1 && args[1] >= 0 ? args[1] : 0;

    public static bool IsTidUsedMeetsMinArgs(int argCount) => argCount >= 1;

    public static int IntegerSqrtRoundDown(int value) =>
        (int)Math.Floor(Math.Sqrt(value));

    public static int FixedSqrtFromAcs(int acsFixed) =>
        VmPAcsHeadConstants971Slice.DoubleToAcs(Math.Sqrt(VmPAcsHeadConstants971Slice.AcsToDouble(acsFixed)));

    public static int VectorLengthFixed(int acsX, int acsY)
    {
        var x = VmPAcsHeadConstants971Slice.AcsToDouble(acsX);
        var y = VmPAcsHeadConstants971Slice.AcsToDouble(acsY);
        return VmPAcsHeadConstants971Slice.DoubleToAcs(Math.Sqrt(x * x + y * y));
    }

    public readonly record struct HudClipRectState(
        int Left,
        int Top,
        int Width,
        int Height,
        int WrapWidth,
        bool HandleAspect);

    public static HudClipRectState BuildHudClipRect(IReadOnlyList<int> args, int argCount) =>
        new(
            argCount > 0 ? args[0] : 0,
            argCount > 1 ? args[1] : 0,
            argCount > 2 ? args[2] : 0,
            argCount > 3 ? args[3] : 0,
            argCount > 4 ? args[4] : 0,
            argCount <= 5 || args[5] != 0);

    public static int HudWrapWidth(IReadOnlyList<int> args, int argCount) =>
        argCount > 0 ? args[0] : 0;

    public static bool SetCvarMeetsMinArgs(int argCount) => argCount >= 2;

    public static bool GetUserCvarMeetsMinArgs(int argCount) => argCount >= 2;

    public static bool SetUserCvarMeetsMinArgs(int argCount) => argCount >= 3;

    public static bool LineAttackMeetsMinArgs(int argCount) => argCount >= 4;

    public static int LineAttackFhFlags(int acsFlags)
    {
        var fh = 0;
        if ((acsFlags & FhfNorandompuffz) != 0)
            fh |= LafNorandompuffz;
        if ((acsFlags & FhfNoimpactdecal) != 0)
            fh |= LafNoimpactdecal;
        return fh;
    }

    public static bool LineAttackUsesActivator(int tidArg) => tidArg == 0;

    public readonly record struct PlaySoundArgs(
        int Tid,
        int Channel,
        float Volume,
        bool Looping,
        float Attenuation,
        bool Local);

    public static PlaySoundArgs BuildPlaySoundArgs(IReadOnlyList<int> args, int argCount) =>
        new(
            args[0],
            argCount > 2 ? args[2] : ChanBody,
            argCount > 3 ? VmPAcsHeadConstants971Slice.AcsToFloat(args[3]) : 1f,
            argCount > 4 && args[4] != 0,
            argCount > 5 ? VmPAcsHeadConstants971Slice.AcsToFloat(args[5]) : AttnNorm,
            argCount > 6 && args[6] != 0);

    public static bool PlaySoundUsesActivator(int tid) => tid == 0;

    public static bool PlaySoundShouldResolveLookup(int funcIndex, int acsfPlaySound) =>
        funcIndex == acsfPlaySound;
}
