namespace HCDE.Playsim;

/// <summary>
/// Autosave through <c>LS_GlassBreak</c> spawn midpoint from <c>p_lnspec.cpp</c> (**3155–3304**); chunk **932**.
/// </summary>
public static class VmPLnspecAutosaveCommunicator932Slice
{
    public const int DemCheckAutosave = 41;
    public const int Level2NoAutosaveHint = 1;
    public const int MlBlocking = 1;
    public const int MlBlockEverything = 2;
    public const int MlAddTrans = 1;

    public static bool AutosaveShouldQueue(int gameAction, int gaSave, int gaQuick) =>
        gameAction != gaSave && gameAction != gaQuick;

    public static int AutosaveClearHintFlags(int flags2) => flags2 & ~Level2NoAutosaveHint;

    public static int NextSkillFromArg(int arg0, int skillCount) =>
        (uint)arg0 >= (uint)skillCount ? -1 : arg0;

    public static object? NoiseAlertTarget(int targetTid, object? activator, object? iteratorHit) =>
        targetTid == 0 ? activator : iteratorHit;

    public static object? NoiseAlertEmitter(
        int emitterTid,
        int targetTid,
        object? activator,
        object? target,
        object? iteratorHit) =>
        emitterTid == 0 ? activator : emitterTid == targetTid ? target : iteratorHit;

    public static bool NoiseAlertCanRun(object? target) => target != null;

    public static bool CommunicatorRejectBackside(bool frontOnly, bool backSide) => frontOnly && backSide;

    public static bool CommunicatorRequiresInventory(object? activator, object? player, bool hasCommunicator) =>
        activator != null && player != null && hasCommunicator;

    public static string CommunicatorSoundName(int voc) => $"svox/voc{voc}";

    public static string CommunicatorSubtitleKey(int identify) => $"TXT_COMM{identify}";

    public static bool CommunicatorShouldSetLogNumber(bool noLog) => !noLog;

    public static bool CommunicatorPlaysLocally(bool checkLocalView) => checkLocalView;

    public static bool ForceFieldApplies(object? activator) => activator != null;

    public static int ForceFieldDamage() => 16;

    public static double ForceFieldThrustSpeed() => 7.8125;

    public static double ForceFieldThrustYawDegrees(double actorYawDegrees) => actorYawDegrees + 180.0;

    public static bool ClearForceFieldAnySector(bool sectorHit) => sectorHit;

    public static int GlassBreakStripBlocking(int lineFlags) =>
        lineFlags & ~(MlBlocking | MlBlockEverything);

    public static bool GlassBreakAnyTextureChanged(bool side0, bool side1) => side0 || side1;

    public static bool GlassBreakShouldSpawnJunk(int noJunkArg) => noJunkArg == 0;

    public static bool GlassBreakUsesSpawnableType(int junkId) => junkId > 0;

    public static (double x, double y) GlassBreakSpawnPoint(
        double v1x,
        double v1y,
        double v2x,
        double v2y)
    {
        var midX = (v1x + v2x) * 0.5;
        var midY = (v1y + v2y) * 0.5;
        var dx = v2x - v1x;
        var dy = v2y - v1y;
        var len = Math.Sqrt(dx * dx + dy * dy);
        if (len == 0)
            return (midX, midY);
        var nx = dy / len;
        var ny = -dx / len;
        return (midX + nx * 2, midY + ny * 2);
    }

    public readonly record struct AutosaveInvoke(bool QueueDem, int ClearedFlags2);

    public static AutosaveInvoke BuildAutosave(int gameAction, int gaSave, int gaQuick, int flags2)
    {
        if (!AutosaveShouldQueue(gameAction, gaSave, gaQuick))
            return new(false, flags2);
        return new(true, AutosaveClearHintFlags(flags2));
    }

    public readonly record struct CommunicatorInvoke(
        string SoundName,
        string SubtitleKey,
        bool SetLog,
        bool PlaySound);

    public static CommunicatorInvoke? BuildCommunicator(
        bool frontOnly,
        bool backSide,
        object? activator,
        object? player,
        bool hasCommunicator,
        int voc,
        int identify,
        bool noLog,
        bool localView)
    {
        if (CommunicatorRejectBackside(frontOnly, backSide))
            return null;
        if (!CommunicatorRequiresInventory(activator, player, hasCommunicator))
            return null;
        return new(
            CommunicatorSoundName(voc),
            CommunicatorSubtitleKey(identify),
            CommunicatorShouldSetLogNumber(noLog),
            CommunicatorPlaysLocally(localView));
    }

    public readonly record struct GlassBreakInvoke(
        int LineFlags,
        bool Switched,
        bool SpawnJunk,
        bool UseType,
        double SpawnX,
        double SpawnY);

    public static GlassBreakInvoke BuildGlassBreak(
        int lineFlags,
        bool switchedSide0,
        bool switchedSide1,
        int noJunkArg,
        int junkId,
        double v1x,
        double v1y,
        double v2x,
        double v2y)
    {
        var flags = GlassBreakStripBlocking(lineFlags);
        var switched = GlassBreakAnyTextureChanged(switchedSide0, switchedSide1);
        var (sx, sy) = GlassBreakSpawnPoint(v1x, v1y, v2x, v2y);
        return new(
            flags,
            switched,
            switched && GlassBreakShouldSpawnJunk(noJunkArg),
            GlassBreakUsesSpawnableType(junkId),
            sx,
            sy);
    }
}
