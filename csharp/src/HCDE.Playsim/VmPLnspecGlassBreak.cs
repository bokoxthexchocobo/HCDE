namespace HCDE.Playsim;

/// <summary>Native <c>LS_GlassBreak</c> from <c>p_lnspec.cpp</c> (3277–3360).</summary>
public static class VmPLnspecGlassBreak
{
    public const int MlBlocking = 1;
    public const int MlBlockEverything = 2;
    public const int JunkSpawnCount = 7;
    public const double SpawnOffsetUnits = 2.0;
    public const double GlassZOffset = 24.0;

    public static bool ShouldSpawnJunk(int noJunkArg) => noJunkArg == 0;

    public static bool ShouldSpawnTypedJunk(int junkId) => junkId > 0;

    public static bool CombineSwitchResults(bool side0, bool side1) => side0 | side1;

    public static bool MirrorQuestWhenNoBack(bool quest1, bool hasBack, bool quest2) =>
        hasBack ? quest2 : quest1;

    public static int ClearedLineSpecial() => 0;

    public static (double midX, double midY) LineMidpoint(double x1, double y1, double x2, double y2) =>
        ((x1 + x2) / 2, (y1 + y2) / 2);

    public static bool SwitchedOk(bool switched) => switched;

    public static int GlassSpawnIterations() => JunkSpawnCount;

    public static bool ClearForceFieldAnySector(bool found) => found;

    public static int GlassBreakClearFlags(int flags) => flags & ~(MlBlocking | MlBlockEverything);

    public static double GlassYawSpread(int pr) => pr * (360.0 / 256.0);

    public static int GlassVelZ(int pr) => pr & 7;

    public static int GlassVelAngle(int pr) => pr & 3;

    public static bool GlassSpawned(object? glass) => glass != null;

    public static string DefaultGlassJunkClass() => "GlassJunk";

    public static bool TypedJunkNeedsSpawnable(bool hasType) => hasType;

    public static bool QuestComplete(bool q1, bool q2) => q1 || q2;

    public static bool ActivatorMissingUsesFirstPlayer(object? it) => it == null;
}
