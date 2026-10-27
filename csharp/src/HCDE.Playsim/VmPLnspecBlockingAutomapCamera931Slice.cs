namespace HCDE.Playsim;

/// <summary>
/// Line blocking, automap, and <c>ChangeCamera</c> head from <c>p_lnspec.cpp</c> (**2719–2868**); chunk **931**.
/// </summary>
public static class VmPLnspecBlockingAutomapCamera931Slice
{
    public const int CfRevertPlease = 1;

    public static bool BlockingRejectZeroId(int lineId) => lineId == 0;

    public readonly record struct BlockingFlagEntry(int FieldIndex, int Bitmask);

    public static BlockingFlagEntry[] BlockingFlagTable() =>
    [
        new(0, 1),
        new(0, 2),
        new(0, 4),
        new(0, 8),
        new(0, 16),
        new(0, 32),
        new(0, 64),
        new(0, 128),
        new(0, 256),
        new(0, 512),
        new(0, 1024),
        new(1, 1),
    ];

    public static (int[] setFlags, int[] clearFlags) DecodeBlockingArgs(int setRaw, int clearRaw)
    {
        var set = new int[2];
        var clear = new int[2];
        var setArg = setRaw;
        var clearArg = clearRaw;
        foreach (var entry in BlockingFlagTable())
        {
            if ((setArg & 1) != 0)
                set[entry.FieldIndex] |= entry.Bitmask;
            if ((clearArg & 1) != 0)
                clear[entry.FieldIndex] |= entry.Bitmask;
            setArg >>= 1;
            clearArg >>= 1;
        }
        return (set, clear);
    }

    public static (int flags, int flags2) ApplyBlocking(int currentFlags, int currentFlags2, int[] set, int[] clear) =>
        ((currentFlags & ~clear[0]) | set[0], (currentFlags2 & ~clear[1]) | set[1]);

    public static bool AutomapFlagsRejectZeroId(int lineId) => lineId == 0;

    public static int[] AutomapFlagTable() => [1, 2, 4, 8, 16];

    public static (int set, int clear) DecodeAutomapFlags(int setRaw, int clearRaw)
    {
        var set = 0;
        var clear = 0;
        var setArg = setRaw;
        var clearArg = clearRaw;
        foreach (var flag in AutomapFlagTable())
        {
            if ((setArg & 1) != 0)
                set |= flag;
            if ((clearArg & 1) != 0)
                clear |= flag;
            setArg >>= 1;
            clearArg >>= 1;
        }
        return (set, clear);
    }

    public static int ApplyAutomapFlags(int current, int set, int clear) => (current & ~clear) | set;

    public static bool AutomapStyleInRange(int style, int styleCount) => style >= 0 && style < styleCount;

    public static bool ShouldSetRevertCheat(int revertArg) => revertArg != 0;

    public static int ApplyRevertCheat(int cheats, bool set) =>
        set ? cheats | CfRevertPlease : cheats & ~CfRevertPlease;

    public readonly record struct ChangeCameraState(
        object? Camera,
        object? Activator,
        int Who,
        int RevertArg,
        bool ActivatorHasPlayer);

    public static object? ResolveCameraFromTid(int tid, object? iteratorNext) =>
        tid != 0 ? iteratorNext : null;

    public static ChangeCameraState BeginChangeCamera(
        int tid,
        object? iteratorNext,
        object? activator,
        int who,
        int revert,
        bool activatorHasPlayer) =>
        new(ResolveCameraFromTid(tid, iteratorNext), activator, who, revert, activatorHasPlayer);

    public static bool ShouldIterateAllPlayers(ChangeCameraState state) =>
        state.Who != 0 || !state.ActivatorHasPlayer;

    public static object? SelectCameraForPlayer(ChangeCameraState state, object? playerMo) =>
        state.Camera ?? playerMo;

    public static int UpdateCheatsForCamera(int cheats, ChangeCameraState state, bool assigningCamera) =>
        state.Camera != null && ShouldSetRevertCheat(state.RevertArg)
            ? ApplyRevertCheat(cheats, assigningCamera)
            : ApplyRevertCheat(cheats, false);

    public static bool ShouldClearViewerOnCameraChange(object? oldCamera, object? newCamera) =>
        !ReferenceEquals(oldCamera, newCamera);
}
