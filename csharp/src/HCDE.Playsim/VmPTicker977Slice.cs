namespace HCDE.Playsim;

/// <summary>
/// <c>P_ClearLevelInterpolation</c> tail and <c>P_Ticker</c> head from <c>p_tick.cpp</c> (**151–300**).
/// </summary>
public static class VmPTicker977Slice
{
    public const int Mf8RecreateLights = 0x00000100;
    public const int LevelFrozenStateGlobalBit = 2;
    public const int MaptimeGlobalFreezeMask = 3;

    public static bool ActorNeedsRecreateLights(int flags8) =>
        (flags8 & Mf8RecreateLights) != 0;

    public static int ClearRecreateLightsFlag(int flags8) =>
        flags8 & ~Mf8RecreateLights;

    public static int ApplyRecreateLightsClear(int flags8) =>
        ActorNeedsRecreateLights(flags8) ? ClearRecreateLightsFlag(flags8) : flags8;

    public static bool TickerPausedEarlyOut(bool paused, bool checkTickerPaused) =>
        paused || checkTickerPaused;

    public static bool ShouldRunGlobalFreezeTick(int maptime) =>
        (maptime & MaptimeGlobalFreezeMask) == 0;

    public static byte ToggleGlobalFreeze(byte globalFreeze) =>
        (byte)(globalFreeze ^ 1);

    public static int MergeGlobalFreezeIntoLevelState(int frozenState, byte globalFreeze) =>
        (frozenState & ~LevelFrozenStateGlobalBit) | (LevelFrozenStateGlobalBit * globalFreeze);

    public static bool TryApplyGlobalFreezeChange(
        int maptime,
        bool globalChangeFreezePending,
        byte globalFreeze,
        out byte newGlobalFreeze,
        out bool applied)
    {
        newGlobalFreeze = globalFreeze;
        applied = false;
        if (!ShouldRunGlobalFreezeTick(maptime) || !globalChangeFreezePending)
            return false;

        newGlobalFreeze = ToggleGlobalFreeze(globalFreeze);
        applied = true;
        return true;
    }

    public static bool AllPlayersClearOfTimeFreezer(ReadOnlySpan<int> timeFreezers, ReadOnlySpan<bool> inGame)
    {
        var count = Math.Min(timeFreezers.Length, inGame.Length);
        for (var i = 0; i < count; i++)
        {
            if (inGame[i] && timeFreezers[i] != 0)
                return false;
        }

        return true;
    }

    public static bool RendererInterpolationAfterSightReset(bool paused, bool tickerPaused) =>
        !paused && !tickerPaused;

    public static bool ShouldAdvancePspriteInterpolationReset(bool playerInGame, object? pspriteHead) =>
        playerInGame && pspriteHead is not null;

    public static bool ShouldClearStatusBarInterpolation(object? statusBar) => statusBar is not null;

    public static bool SkipHcdeRuntimeJoinerPlayerThink(
        bool mobPresent,
        bool netGame,
        bool clientUsesHcdeService,
        bool isAuthoritySlot) =>
        !mobPresent && netGame && clientUsesHcdeService && !isAuthoritySlot;

    public static bool ShouldInvokePlayerThink(bool playerInLevel, bool skipHcdeJoiner) =>
        playerInLevel && !skipHcdeJoiner;

    public static bool ShouldUpdateSpecials(bool levelFrozen) => !levelFrozen;

    public static (int Time, int Maptime, int Totaltime) IncrementLevelClocks(int time, int maptime, int totaltime) =>
        (time + 1, maptime + 1, totaltime + 1);
}
