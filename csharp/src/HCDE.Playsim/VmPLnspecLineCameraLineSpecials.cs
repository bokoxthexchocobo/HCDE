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
        foreach (var bit in AutomapFlagTable())
        {
            if ((setArg & 1) != 0)
                set |= bit;
            if ((clearArg & 1) != 0)
                clear |= bit;
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
}
