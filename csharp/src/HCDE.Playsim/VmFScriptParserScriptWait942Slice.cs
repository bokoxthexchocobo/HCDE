namespace HCDE.Playsim;

/// <summary>
/// Script wait/start/running helpers from <c>fragglescript/t_func.cpp</c> (**3845–3994**); chunk **942**.
/// </summary>
public static class VmFScriptParserScriptWait942Slice
{
    public const int MaxScripts = VmFScriptScript.MaxScripts;
    public const int TicRate = GameTicClock.TicRate;

    public enum FraggleWaitKind
    {
        Delay = VmFScriptScript.WtDelay,
        TagWait = VmFScriptScript.WtTagwait,
        ScriptWait = VmFScriptScript.WtScriptwait,
        ScriptWaitPre = VmFScriptScript.WtScriptwaitpre,
    }

    public static bool UnaryWaitRequiresExactlyOneArg(int argc) => argc == 1;

    public static bool ScriptRunningRequiresAtLeastOneArg(int argc) => argc >= 1;

    public static int WaitDelayTicksFromCentiseconds(int centiseconds) =>
        centiseconds * TicRate / 100;

    public readonly record struct WaitScriptSuspend(int WaitType, int WaitData, bool ThrowsTerminator);

    public static WaitScriptSuspend? BuildDelayWait(int argc, int centiseconds)
    {
        if (!UnaryWaitRequiresExactlyOneArg(argc))
            return null;
        return new((int)FraggleWaitKind.Delay, WaitDelayTicksFromCentiseconds(centiseconds), true);
    }

    public static WaitScriptSuspend? BuildTagWait(int argc, int sectorTag)
    {
        if (!UnaryWaitRequiresExactlyOneArg(argc))
            return null;
        return new((int)FraggleWaitKind.TagWait, sectorTag, true);
    }

    public static WaitScriptSuspend? BuildScriptWait(int argc, int scriptNumber)
    {
        if (!UnaryWaitRequiresExactlyOneArg(argc))
            return null;
        return new((int)FraggleWaitKind.ScriptWait, scriptNumber, true);
    }

    public static WaitScriptSuspend? BuildScriptWaitPre(int argc, int scriptNumber)
    {
        if (!UnaryWaitRequiresExactlyOneArg(argc))
            return null;
        return new((int)FraggleWaitKind.ScriptWaitPre, scriptNumber, true);
    }

    public static bool StartScriptRequiresExactlyOneArg(int argc) => argc == 1;

    public static bool StartScriptNumberInRange(int scriptNumber) =>
        scriptNumber >= 0 && scriptNumber < MaxScripts;

    public static bool StartScriptHasThinker(object? thinker) => thinker != null;

    public static bool StartScriptChildDefined(object? childScript) => childScript is not null;

    public readonly record struct StartScriptInvoke(object Trigger, object ChildScript);

    public static StartScriptInvoke? BuildStartScript(
        int argc,
        int scriptNumber,
        object? thinker,
        object? childScript,
        object? trigger)
    {
        if (!StartScriptRequiresExactlyOneArg(argc)
            || !StartScriptNumberInRange(scriptNumber)
            || !StartScriptHasThinker(thinker)
            || !StartScriptChildDefined(childScript)
            || trigger is null)
            return null;
        return new(trigger, childScript!);
    }

    public static int ScriptRunningReturn(bool scriptFound) => scriptFound ? 1 : 0;

    public static bool ScriptRunningEntryMatches(int runningScriptNum, int requestedScriptNum) =>
        runningScriptNum == requestedScriptNum;

    public static bool ScriptRunningScanShouldContinue(object? nextEntry) => nextEntry is not null;
}
