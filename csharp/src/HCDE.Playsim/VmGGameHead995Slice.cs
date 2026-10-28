namespace HCDE.Playsim;

/// <summary>
/// Movement/spy macros and spy-step helpers from <c>g_game.cpp</c> (**1–150**).
/// </summary>
public static class VmGGameHead995Slice
{
    public const int TurboThreshold = 12800;
    public const int SlowTurnTics = 6;
    public const int AnalogLookBase = 1280;

    public const int SpyCancel = 0;
    public const int SpyNext = 1;
    public const int SpyPrev = 2;

    public static int ResolveSpyStep(int spyCommand) =>
        spyCommand switch
        {
            SpyNext => 1,
            SpyPrev => -1,
            _ => 0,
        };

    public static bool ShouldAdvanceSpyCamera(int spyCommand) =>
        spyCommand is SpyNext or SpyPrev;

    public static int AdvanceSpyPlayerIndex(
        int current,
        int spyCommand,
        int maxPlayers,
        Func<int, bool> isEligible)
    {
        if (spyCommand == SpyCancel)
            return current;

        var step = ResolveSpyStep(spyCommand);
        if (step == 0)
            return current;

        var mask = maxPlayers - 1;
        var pnum = current;
        var start = current;
        do
        {
            pnum = (pnum + step) & mask;
            if (isEligible(pnum))
                return pnum;
        } while (pnum != start);

        return current;
    }

    public static int ApplySlowTurnSpeedBoost(int turnHeld, int turnSpeed) =>
        turnHeld < SlowTurnTics ? turnSpeed + 2 : turnSpeed;

    public static int ScaleAnalogLookAxis(float axis, float sensitivity) =>
        TruncateTowardZero(axis * AnalogLookBase * sensitivity);

    public static int ScaleAnalogYawAxis(float axisYaw, float sensitivity) =>
        TruncateTowardZero(axisYaw * -AnalogLookBase * sensitivity);

    public static (int pitch, int yaw) BuildAnalogLookDeltas(
        float axisPitch,
        float axisYaw,
        float pitchSensitivity,
        float yawSensitivity) =>
        (
            ScaleAnalogLookAxis(axisPitch, pitchSensitivity),
            ScaleAnalogYawAxis(axisYaw, yawSensitivity));

    public static bool ForwardMoveExceedsTurboThreshold(int forwardMove) =>
        forwardMove > TurboThreshold;

    public static bool ShouldDetectTurboForwardMove(
        bool multiplayer,
        float turboPercent,
        int forwardMove) =>
        multiplayer && turboPercent > 100f && ForwardMoveExceedsTurboThreshold(forwardMove);

    private static int TruncateTowardZero(float value) =>
        value >= 0f ? (int)value : -(int)(-value);
}
