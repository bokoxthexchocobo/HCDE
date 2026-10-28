namespace HCDE.Playsim;

/// <summary>Client prediction gate helpers from <c>p_user.cpp</c> <c>P_PredictClient</c> (2021–2394).</summary>
public static class VmPUserPredictClient
{
    public const int GsLevel = 1;
    public const int PstLive = 0;
    public const int CfPredicting = 1;
    public const int OfJustSpawned = 1;
    public const int RfNointerpolateview = 1;

    public enum PredictClientExit
    {
        Continue,
        SinglePlayer,
        NotInLevel,
        ReservedSlot,
        BadConsolePlayer,
        NoPlayerMo,
        SkipReplay,
        EmptyWindow,
    }

    public sealed class PredictClientContext
    {
        public bool Netgame { get; init; }
        public int Gamestate { get; init; }
        public bool ReservedSlot { get; init; }
        public int ConsolePlayer { get; init; }
        public int MaxPlayers { get; init; } = 8;
        public bool PlayerMoNull { get; init; }
        public bool AlreadyPredicting { get; init; }
        public int ClientTic { get; init; }
        public int LastPredictedTic { get; init; }
        public int PlayerState { get; init; }
        public int ObjectFlags { get; init; }
        public int PredictStart { get; init; }
        public int PredictEnd { get; init; }
        public int PredictMax { get; init; } = 1;
        public int EndCap { get; init; }
        public bool ResetPrediction { get; init; }
        public int LastPosTic { get; init; }
        public float RubberbandScale { get; init; }
        public float RubberbandMinMove { get; init; }
        public float RubberbandThreshold { get; init; }
        public bool Paused { get; init; }
        public bool PredictWeapon { get; init; }
        public int WeaponPspriteTic { get; init; }
        public bool DebugDiag { get; init; }
        public Action? TearDownSinglePlayer { get; init; }
        public Action? ClearPredictingCheat { get; init; }
        public Action? BootstrapPrediction { get; init; }
        public Action? SetPredictingCheat { get; init; }
        public Action? PrepareInterpolationFrame { get; init; }
        public Action<int, int, bool>? OnWindowClamped { get; init; }
        public Action<int>? ReplayTic { get; init; }
        public Func<int, bool>? TryRubberbandAtTic { get; init; }
        public Action<bool>? ApplyRubberband { get; init; }
        public Action? SetPausedNoInterpolate { get; init; }
        public Action<ReplayDiagAccumulators, ReplaySlotMarkers, int, int, bool>? LogReplayDiag { get; init; }
        public Action<int>? UpdateLastPos { get; init; }
    }

    public readonly record struct ReplaySlotMarkers(
        int FirstYaw,
        int LastYaw,
        int FirstForward,
        int LastForward,
        int FirstSide,
        int LastSide,
        int FirstPitch,
        int LastPitch);

    public readonly record struct ReplayYawDiag(
        double PreYaw,
        double PostYaw,
        long YawBamSum,
        int PredictStart,
        int Window,
        bool ResetPrediction,
        ReplaySlotMarkers Slots);

    public readonly record struct ReplayMoveDiag(
        double DeltaLenSq,
        long ForwardSum,
        long SideSum,
        long UpSum,
        bool WindowClamped,
        ReplaySlotMarkers Slots,
        int PredictStart,
        int PredictEnd);

    public static PredictClientExit RunEntryGates(PredictClientContext ctx)
    {
        if (SinglePlayerTearsDownPrediction(ctx.Netgame))
        {
            ctx.TearDownSinglePlayer?.Invoke();
            if (!ConsolePlayerOutOfRange(ctx.ConsolePlayer, ctx.MaxPlayers))
                ctx.ClearPredictingCheat?.Invoke();
            return PredictClientExit.SinglePlayer;
        }

        if (SkipWhenNotInLevel(ctx.Gamestate, GsLevel))
            return PredictClientExit.NotInLevel;

        if (SkipReservedServerSlot(ctx.Netgame, ctx.ReservedSlot))
            return PredictClientExit.ReservedSlot;

        if (ConsolePlayerOutOfRange(ctx.ConsolePlayer, ctx.MaxPlayers))
            return PredictClientExit.BadConsolePlayer;

        if (ctx.PlayerMoNull)
            return PredictClientExit.NoPlayerMo;

        if (ShouldEnablePrediction(ctx.AlreadyPredicting))
            ctx.BootstrapPrediction?.Invoke();

        ctx.SetPredictingCheat?.Invoke();

        if (ShouldSkipPredictionReplay(new PredictionReplayGate(ctx.ClientTic, ctx.LastPredictedTic, ctx.PlayerState, ctx.ObjectFlags)))
            return PredictClientExit.SkipReplay;

        return PredictClientExit.Continue;
    }

    public static bool RunPredictClient(PredictClientContext ctx, out int predictStart, out int predictEnd, out bool windowClamped)
    {
        predictStart = ctx.PredictStart;
        predictEnd = ctx.PredictEnd;
        windowClamped = false;

        var exit = RunEntryGates(ctx);
        if (exit != PredictClientExit.Continue)
            return false;

        ctx.PrepareInterpolationFrame?.Invoke();

        if (!TryBuildPredictionWindow(
                new PredictionWindow(predictStart, predictEnd, ctx.PredictMax),
                ctx.EndCap,
                out predictStart,
                out predictEnd,
                out windowClamped))
            return false;

        if (windowClamped)
            ctx.OnWindowClamped?.Invoke(predictEnd - predictStart, ctx.PredictMax, true);

        var canRubberband = CanRubberband(
            ctx.ResetPrediction,
            windowClamped,
            ctx.LastPosTic,
            predictStart,
            predictEnd,
            ctx.RubberbandScale);

        var rubberbandTic = SelectRubberbandTic(canRubberband, ctx.LastPosTic);

        var weaponTic = ReseedWeaponPspriteWhenBehind(ctx.WeaponPspriteTic, predictEnd, predictStart);
        var diag = RecordReplayDiag(ctx.Netgame, ctx.DebugDiag);
        var acc = new ReplayDiagAccumulators(0, 0, 0, 0, 0, 0, 0);
        var slots = new ReplaySlotMarkers(0, 0, 0, 0, 0, 0, 0, 0);
        var rubberband = false;
        var rubberbandLimit = false;

        for (var i = predictStart; i < predictEnd; i++)
        {
            if (i == rubberbandTic && ctx.TryRubberbandAtTic?.Invoke(i) == true)
            {
                rubberband = true;
                rubberbandLimit = true;
            }

            if (diag)
                acc = RecordReplaySlot(acc, ref slots, i, predictStart, 0, 0, 0, 0, 0);

            if (!ctx.Paused)
                ctx.ReplayTic?.Invoke(i);
        }

        if (rubberband)
            ctx.ApplyRubberband?.Invoke(rubberbandLimit);
        else if (PausedSetsNoInterpolate(false, ctx.Paused))
            ctx.SetPausedNoInterpolate?.Invoke();

        if (diag)
            ctx.LogReplayDiag?.Invoke(acc, slots, predictStart, predictEnd, windowClamped);

        var lastPredicted = predictEnd;
        var reset = false;
        var weaponWatermark = weaponTic;
        FinalizePredictionTick(ref lastPredicted, ref reset, predictEnd, ref weaponWatermark, ctx.PredictWeapon, ctx.Paused);
        ctx.UpdateLastPos?.Invoke(predictEnd);
        return true;
    }

    public static int ReseedWeaponPspriteWhenBehind(int weaponPspriteTic, int predictEnd, int predictStart) =>
        ReseedWeaponPspriteTic(weaponPspriteTic, predictEnd, predictStart) ? predictStart : weaponPspriteTic;

    public static int SelectRubberbandTic(bool canRubberband, int lastPosTic) =>
        canRubberband ? lastPosTic : int.MinValue;

    public static double ComputeRubberbandThreshold(float minMove, float threshold) =>
        Math.Max(minMove, threshold);

    public static ReplayDiagAccumulators RecordReplaySlot(
        ReplayDiagAccumulators acc,
        ref ReplaySlotMarkers slots,
        int tic,
        int predictStart,
        int yaw,
        int forward,
        int side,
        int pitch,
        int appliedPitch)
    {
        if (tic == predictStart)
            slots = slots with
            {
                FirstYaw = yaw,
                FirstForward = forward,
                FirstSide = side,
                FirstPitch = pitch,
            };
        slots = slots with
        {
            LastYaw = yaw,
            LastForward = forward,
            LastSide = side,
            LastPitch = pitch,
        };
        return AccumulateReplayCmd(acc, yaw, forward, side, 0, pitch, appliedPitch);
    }

    public static bool SinglePlayerTearsDownPrediction(bool netgame) => !netgame;

    public static bool SkipWhenNotInLevel(int gamestate, int gsLevel) => gamestate != gsLevel;

    public static bool SkipReservedServerSlot(bool netgame, bool reserved) => netgame && reserved;

    public static bool ConsolePlayerOutOfRange(int consoleplayer, int maxPlayers) =>
        consoleplayer < 0 || consoleplayer >= maxPlayers;

    public static bool ShouldEnablePrediction(bool alreadyPredicting) => !alreadyPredicting;

    public static bool SkipReplayWhenCaughtUp(int clientTic, int lastPredicted) => clientTic <= lastPredicted;

    public static bool SkipReplayWhenNotLive(int playerstate, int pstLive) => playerstate != pstLive;

    public static bool SkipReplayWhenJustSpawned(int objectFlags, int justSpawnedFlag) =>
        (objectFlags & justSpawnedFlag) != 0;

    public static int ClampPredictionEnd(int clientTic, int capTic) => capTic < clientTic ? capTic : clientTic;

    public static bool EmptyPredictionWindow(int predictEnd, int predictStart) => predictEnd <= predictStart;

    public static int ClampPredictionStart(int predictEnd, int predictMax, int predictStart, out bool clamped)
    {
        var window = predictEnd - predictStart;
        var max = Math.Max(1, predictMax);
        if (window > max)
        {
            clamped = true;
            return predictEnd - max;
        }

        clamped = false;
        return predictStart;
    }

    public static bool CanRubberband(
        bool resetPrediction,
        bool windowClamped,
        int lastPosTic,
        int predictStart,
        int predictEnd,
        float rubberbandScale) =>
        resetPrediction
        && !windowClamped
        && lastPosTic >= predictStart
        && lastPosTic < predictEnd
        && rubberbandScale > 0f
        && rubberbandScale < 1f;

    public static bool RubberbandDistanceExceeded(double distSq, double thresholdSq, double epsilonSq) =>
        distSq >= epsilonSq && distSq > thresholdSq;

    public static bool RubberbandHitLimit(double distSq, float limitSq) =>
        limitSq > 0f && distSq > limitSq;

    public static bool ReseedWeaponPspriteTic(int weaponTic, int predictEnd, int predictStart) =>
        weaponTic > predictEnd;

    public static bool AdvanceWeaponPspriteWhenPredicting(bool predictWeapon, bool paused) =>
        predictWeapon && !paused;

    public static int WeaponPspriteTicWatermark(int current, int predictEnd) =>
        Math.Max(current, predictEnd);

    public static bool ShouldClearPredictingCheat(bool netgame) => !netgame;

    public readonly record struct PredictionReplayGate(
        int ClientTic,
        int LastPredictedTic,
        int PlayerState,
        int ObjectFlags);

    public static bool ShouldSkipPredictionReplay(PredictionReplayGate gate) =>
        SkipReplayWhenCaughtUp(gate.ClientTic, gate.LastPredictedTic)
        || SkipReplayWhenNotLive(gate.PlayerState, PstLive)
        || SkipReplayWhenJustSpawned(gate.ObjectFlags, OfJustSpawned);

    public readonly record struct PredictionWindow(
        int PredictStart,
        int PredictEnd,
        int PredictMax);

    public static bool TryBuildPredictionWindow(
        PredictionWindow input,
        int endCap,
        out int start,
        out int end,
        out bool clamped)
    {
        clamped = false;
        end = ClampPredictionEnd(input.PredictEnd, endCap);
        start = input.PredictStart;
        if (EmptyPredictionWindow(end, start))
            return false;
        start = ClampPredictionStart(end, input.PredictMax, start, out clamped);
        return !EmptyPredictionWindow(end, start);
    }

    public readonly record struct RubberbandSample(
        double DistSq,
        double Threshold,
        double Epsilon,
        float Limit);

    public static bool EvaluateRubberband(RubberbandSample sample, out bool hitLimit)
    {
        hitLimit = false;
        var thresholdSq = sample.Threshold * sample.Threshold;
        var epsilonSq = sample.Epsilon * sample.Epsilon;
        if (!RubberbandDistanceExceeded(sample.DistSq, thresholdSq, epsilonSq))
            return false;
        hitLimit = RubberbandHitLimit(sample.DistSq, sample.Limit * sample.Limit);
        return true;
    }

    public static bool TryRubberbandDistance(
        double distSq,
        double threshold,
        double epsilon,
        float limit,
        out bool hitLimit) =>
        EvaluateRubberband(new RubberbandSample(distSq, threshold, epsilon, limit), out hitLimit);

    public static int LastPredictedFromBase(int baseSequence) => Math.Max(baseSequence, 0);

    public static bool RecordReplayDiag(bool netgame, bool debugEnabled) => netgame && debugEnabled;

    public static long AccumulateReplayYaw(long sum, int yaw) => sum + yaw;

    public static bool ApplyPortalOffsetWhenSectorPresent(bool sectorNull) => !sectorNull;

    public static int LocalCmdIndex(int tic, int ringSize) => tic % ringSize;

    public static bool WeaponTickForIndex(int tic, int watermark) => tic >= watermark;

    public static bool RubberbandLerpPath(bool hitLimit) => !hitLimit;

    public static bool PausedSetsNoInterpolate(bool rubberband, bool paused) =>
        !rubberband && paused;

    public static double FoldYawDegrees(double degrees)
    {
        while (degrees > 180.0)
            degrees -= 360.0;
        while (degrees < -180.0)
            degrees += 360.0;
        return degrees;
    }

    public static double ReplayInputDegreesFromBam(long yawBamSum) =>
        yawBamSum * (360.0 / 65536.0);

    public static double PhantomYawDegrees(double yawChange, double replayInputDeg) =>
        FoldYawDegrees(yawChange - replayInputDeg);

    public static bool ShouldLogYawDiag(double yawChange, double phantomDeg) =>
        Math.Abs(yawChange) > 3.0 || Math.Abs(phantomDeg) > 1.0;

    public static bool ShouldLogMoveDiag(
        double deltaLenSq,
        long forwardSum,
        long sideSum,
        long upSum,
        bool windowClamped) =>
        deltaLenSq > 0.01
        || forwardSum != 0
        || sideSum != 0
        || upSum != 0
        || windowClamped;

    public static bool ShouldLogPitchSummary(
        int applied,
        int zeroed,
        int firstPitch,
        int lastPitch) =>
        applied != 0 || zeroed != 0 || firstPitch != 0 || lastPitch != 0;

    public static bool PitchClampHit(bool hitMin, bool hitMax, double deltaDeg) =>
        hitMin || hitMax || Math.Abs(deltaDeg) > 0.05;

    public static bool NonZeroReplayPitch(int pitch) => pitch != 0;

    public static int CountZeroedPitch(int replayPitch, int appliedPitch) =>
        replayPitch != 0 && appliedPitch == 0 ? 1 : 0;

    public readonly record struct ReplayDiagAccumulators(
        long YawBam,
        long Forward,
        long Side,
        long Up,
        long PitchSum,
        int PitchApplied,
        int PitchZeroed);

    public static ReplayDiagAccumulators AccumulateReplayCmd(
        ReplayDiagAccumulators acc,
        int yaw,
        int forward,
        int side,
        int up,
        int pitch,
        int appliedPitch)
    {
        var pitchApplied = acc.PitchApplied;
        var pitchZeroed = acc.PitchZeroed;
        var pitchSum = acc.PitchSum;
        if (pitch != 0)
        {
            pitchApplied++;
            pitchSum += pitch;
        }
        else if (appliedPitch != 0)
            pitchZeroed++;

        return acc with
        {
            YawBam = acc.YawBam + yaw,
            Forward = acc.Forward + forward,
            Side = acc.Side + side,
            Up = acc.Up + up,
            PitchSum = pitchSum,
            PitchApplied = pitchApplied,
            PitchZeroed = pitchZeroed,
        };
    }

    public static double ComputePhantomYawDiag(double preYaw, double postYaw, long yawBamSum)
    {
        var yawChange = FoldYawDegrees(postYaw - preYaw);
        return PhantomYawDegrees(yawChange, ReplayInputDegreesFromBam(yawBamSum));
    }

    public static void FinalizePredictionTick(
        ref int lastPredictedTic,
        ref bool resetPrediction,
        int predictEnd,
        ref int weaponPspriteTic,
        bool predictWeapon,
        bool paused)
    {
        lastPredictedTic = predictEnd;
        resetPrediction = false;
        if (predictWeapon && !paused)
            weaponPspriteTic = WeaponPspriteTicWatermark(weaponPspriteTic, predictEnd);
    }
}
