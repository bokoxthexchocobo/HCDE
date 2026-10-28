namespace HCDE.Playsim;

/// <summary>
/// Turbo scaling, save CVAR clamps, mouse smoothing, and `turnspeeds` CCMD from <c>g_game.cpp</c> (**375–524**).
/// </summary>
public static class VmGGameInput997Slice
{
    public const float TurboMinPercent = 10f;
    public const float TurboMaxPercent = 255f;
    public const double MouseSmoothCurveRange = 64.0;

    public readonly struct MovementScaleResult
    {
        public float ClampedTurboPercent { get; init; }
        public int ForwardMove0 { get; init; }
        public int ForwardMove1 { get; init; }
        public int SideMove0 { get; init; }
        public int SideMove1 { get; init; }
        public bool AppliedNormScale { get; init; }
    }

    public readonly struct TurnSpeedAssignment
    {
        public int WalkFast { get; init; }
        public int SprintFast { get; init; }
        public int WalkSlow { get; init; }
        public int SprintSlow { get; init; }
    }

    public static float ClampTurboPercent(float turboPercent)
    {
        if (turboPercent < TurboMinPercent)
            return TurboMinPercent;
        if (turboPercent > TurboMaxPercent)
            return TurboMaxPercent;
        return turboPercent;
    }

    public static MovementScaleResult ApplyTurboToMovementTables(
        float turboPercent,
        int normForward0,
        int normForward1,
        int normSide0,
        int normSide1)
    {
        var clamped = ClampTurboPercent(turboPercent);
        if (clamped != turboPercent)
        {
            return new MovementScaleResult
            {
                ClampedTurboPercent = clamped,
                ForwardMove0 = normForward0,
                ForwardMove1 = normForward1,
                SideMove0 = normSide0,
                SideMove1 = normSide1,
                AppliedNormScale = false,
            };
        }

        var scale = clamped * 0.01;
        return new MovementScaleResult
        {
            ClampedTurboPercent = clamped,
            ForwardMove0 = (int)(normForward0 * scale),
            ForwardMove1 = (int)(normForward1 * scale),
            SideMove0 = (int)(normSide0 * scale),
            SideMove1 = (int)(normSide1 * scale),
            AppliedNormScale = true,
        };
    }

    public static int NormalizeAutosaveCount(int autosaveCount) =>
        autosaveCount < 0 ? 0 : autosaveCount;

    public static int NormalizeQuicksaveRotationCount(int rotationCount) =>
        rotationCount < 1 ? 1 : rotationCount;

    public static double SmoothMouseDelta(double delta, int smoothCurve)
    {
        if (smoothCurve <= 0 || delta == 0.0)
            return delta;

        var sign = delta < 0.0 ? -1.0 : 1.0;
        var magnitude = Math.Abs(delta);
        var clamped = Math.Min(magnitude / MouseSmoothCurveRange, 1.0);
        var eased = clamped * clamped * (3.0 - 2.0 * clamped);
        var tail = magnitude > MouseSmoothCurveRange ? magnitude - MouseSmoothCurveRange : 0.0;
        return sign * ((eased * MouseSmoothCurveRange) + tail);
    }

    public static TurnSpeedAssignment ApplyTurnspeedsConsoleArgs(
        TurnSpeedAssignment current,
        ReadOnlySpan<int> explicitTurnSpeeds)
    {
        var walkFast = current.WalkFast;
        var sprintFast = current.SprintFast;
        var walkSlow = current.WalkSlow;
        var sprintSlow = current.SprintSlow;

        var i = 1;
        for (; i <= 4 && i - 1 < explicitTurnSpeeds.Length; i++)
        {
            var value = explicitTurnSpeeds[i - 1];
            switch (i)
            {
                case 1:
                    walkFast = value;
                    break;
                case 2:
                    sprintFast = value;
                    break;
                case 3:
                    walkSlow = value;
                    break;
                case 4:
                    sprintSlow = value;
                    break;
            }
        }

        if (i <= 2)
            sprintFast = walkFast * 2;
        if (i <= 3)
            walkSlow = walkFast / 2;
        if (i <= 4)
            sprintSlow = walkSlow;

        return new TurnSpeedAssignment
        {
            WalkFast = walkFast,
            SprintFast = sprintFast,
            WalkSlow = walkSlow,
            SprintSlow = sprintSlow,
        };
    }
}
