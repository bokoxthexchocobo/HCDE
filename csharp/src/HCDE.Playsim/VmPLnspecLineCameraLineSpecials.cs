namespace HCDE.Playsim;

/// <summary>Native automap/camera/thing-move/quake/puzzle specials from <c>p_lnspec.cpp</c>.</summary>
public static class VmPLnspecLineCameraLineSpecials
{
    public const int AutomapStyleCount = 0;
    public const int CfRevertPlease = 1;

    public static bool AutomapFlagsRejectZeroId(int lineId) => lineId == 0;

    public static int[] AutomapFlagTable() => [1, 2, 4, 8, 16];

    public static (int set, int clear) DecodeAutomapFlags(int setRaw, int clearRaw)
    {
        var set = 0;
        var clear = 0;
        var setArg = setRaw;
        var clearArg = clearRaw;
        var flags = AutomapFlagTable();
        for (var i = 0; i < flags.Length; i++)
        {
            if ((setArg & 1) != 0)
                set |= flags[i];
            if ((clearArg & 1) != 0)
                clear |= flags[i];
            setArg >>= 1;
            clearArg >>= 1;
        }
        return (set, clear);
    }

    public static int ApplyAutomapFlags(int current, int set, int clear) => (current & ~clear) | set;

    public static bool AutomapStyleInRange(int style, int count) => style >= 0 && style < count;

    public static bool ChangeCameraApplyAllPlayers(int who) => who != 0;

    public static bool ShouldSetRevertCheat(int revertArg) => revertArg != 0;

    public static int ApplyRevertCheat(int cheats, bool set) =>
        set ? cheats | CfRevertPlease : cheats & ~CfRevertPlease;

    public static object? ResolveCameraTid(int tid, object? iteratorNext) =>
        tid != 0 ? iteratorNext : null;

    public static object? FallbackCamera(object? camera, object? activator) => camera ?? activator;

    public static bool ThingMoveFogEnabled(int noFogArg) => noFogArg == 0;

    public static bool PuzzleRequiresActivator(object? activator) => activator == null;

    public static bool PuzzleItemNumberMatches(int itemNumber, int arg0) => itemNumber == arg0;

    public static bool PuzzleInventoryIsKind(object? item, bool isPuzzleKind) => isPuzzleKind;

    public static (int intensity, int duration, int damRad, int tremRad) RadiusQuakeArgs(int i, int d, int dam, int trem) =>
        (i, d, dam * 64, trem * 64);

    public static string QuakeSoundName() => "world/quake";

    public static bool ChangeCameraNeedsPlayer(object? activator, int who, bool activatorHasPlayer) =>
        !activatorHasPlayer && who == 0;

    public static bool ShouldClearViewerOnCameraChange(object? oldCamera, object? newCamera) =>
        !ReferenceEquals(oldCamera, newCamera);

    public static int AutomapStyleClamp(int style, int count) =>
        style < 0 ? -1 : style >= count ? -1 : style;

    public static bool CombineAutomapStyleResult(bool previous, bool next) => previous | next;

    public static bool PuzzleScanContinuesOnMismatch(bool isPuzzle, int number, int wanted) =>
        !isPuzzle || number != wanted;

    public static bool PuzzleUseSucceeded(bool used) => used;

    public static bool QuakeStartsFromTid(int tid) => tid >= 0;

    public readonly record struct ChangeCameraState(
        object? Camera,
        object? Activator,
        int Who,
        int RevertArg,
        bool ActivatorHasPlayer);

    public static ChangeCameraState BeginChangeCamera(int tid, object? iteratorNext, object? activator, int who, int revert, bool activatorHasPlayer) =>
        new(ResolveCameraTid(tid, iteratorNext), activator, who, revert, activatorHasPlayer);

    public static bool ShouldIterateAllPlayers(ChangeCameraState state) =>
        state.Who != 0 || !state.ActivatorHasPlayer;

    public static object? SelectCameraForPlayer(ChangeCameraState state, object? playerMo) =>
        state.Camera ?? playerMo;

    public static int UpdateCheatsForCamera(int cheats, ChangeCameraState state, bool assigningCamera) =>
        state.Camera != null && ShouldSetRevertCheat(state.RevertArg)
            ? ApplyRevertCheat(cheats, assigningCamera)
            : ApplyRevertCheat(cheats, false);

    public readonly record struct PuzzleScanState(bool Found, bool UsedInventory);

    public static PuzzleScanState ScanPuzzleInventory(
        IEnumerable<(bool isPuzzle, int number)> items,
        int wantedNumber)
    {
        foreach (var (isPuzzle, number) in items)
        {
            if (!isPuzzle)
                continue;
            if (!PuzzleItemNumberMatches(number, wantedNumber))
                continue;
            return new(true, true);
        }
        return new(false, false);
    }

    public static bool PuzzleFailsWithoutItem(PuzzleScanState state) => !state.Found;

    public static bool ThingMoveInvoke(int tid, object? activator, int mapspot, int noFog) =>
        tid >= 0 && activator != null && ThingMoveFogEnabled(noFog);

    public static (double intensity, int duration, int damRad, int tremRad, string sound) PackQuake(
        int intensity,
        int duration,
        int damRad,
        int tremRad) =>
        (intensity, duration, damRad * 64, tremRad * 64, QuakeSoundName());

    public static bool AutomapStyleApplySucceeded(int style, int count) =>
        AutomapStyleInRange(style, count);
}
