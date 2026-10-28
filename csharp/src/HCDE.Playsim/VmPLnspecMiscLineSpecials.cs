namespace HCDE.Playsim;

/// <summary>Native misc line specials from <c>p_lnspec.cpp</c> (3130–3275).</summary>
public static class VmPLnspecMiscLineSpecials
{
    public const int MlAddTrans = 1;

    public static double TranslucentAlpha(int amount) => amount < 0 ? 0 : amount > 255 ? 1.0 : amount / 255.0;

    public static int TranslucentFlagsForType(int type, int currentFlags) => type switch
    {
        0 => currentFlags & ~MlAddTrans,
        1 => currentFlags | MlAddTrans,
        _ => currentFlags,
    };

    public static bool TranslucentTypeUnknown(int type) => type is not (0 or 1);

    public static bool AutosaveAllowed(int gameAction, int gaSave, int gaQuick) =>
        gameAction != gaSave && gameAction != gaQuick;

    public static int NextSkillFromArg(int arg0, int skillCount) =>
        (uint)arg0 >= (uint)skillCount ? -1 : arg0;

    public static object? ResolveNoiseTarget(int tid, object? activator, object? iteratorNext) =>
        tid == 0 ? activator : iteratorNext;

    public static object? ResolveNoiseEmitter(int emitterTid, int targetTid, object? activator, object? target, object? iteratorNext) =>
        emitterTid == 0 ? activator : emitterTid == targetTid ? target : iteratorNext;

    public static bool CommunicatorRejectBackside(bool frontOnly, bool backSide) => frontOnly && backSide;

    public static bool CommunicatorRequiresPlayer(object? activator, object? player) =>
        activator != null && player != null;

    public static bool CommunicatorHasInventory(bool hasCommunicator) => hasCommunicator;

    public static string CommunicatorSoundName(int voc) => $"svox/voc{voc}";

    public static string CommunicatorSubtitleKey(int identify) => $"TXT_COMM{identify}";

    public static bool CommunicatorShouldSetLog(bool noLog) => !noLog;

    public static bool ForceFieldDamagesWhenActivatorPresent(object? activator) => activator != null;

    public static double ForceFieldThrustSpeed() => 7.8125;

    public static int ForceFieldDamage() => 16;

    public static bool ClearForceFieldReturns(bool anySector) => anySector;

    public readonly record struct TranslucentInvoke(int LineId, double Alpha, int Flags, bool UnknownType);

    public static TranslucentInvoke BuildTranslucent(int lineId, int amount, int type, int flags)
    {
        var alpha = TranslucentAlpha(amount);
        var unknown = TranslucentTypeUnknown(type);
        var newFlags = unknown ? flags : TranslucentFlagsForType(type, flags);
        return new(lineId, alpha, newFlags, unknown);
    }

    public const int MlBlocking = 1;
    public const int MlBlockEverything = 2;

    public static int GlassBreakClearFlags(int flags) => flags & ~(MlBlocking | MlBlockEverything);

    public static bool GlassBreakSwitched(bool side0, bool side1) => side0 | side1;

    public static bool GlassBreakSpawnsJunk(int noJunkArg) => noJunkArg == 0;
}
