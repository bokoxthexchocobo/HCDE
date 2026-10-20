namespace HCDE.Playsim;

/// <summary>Client prediction gate helpers from <c>p_user.cpp</c> <c>P_PredictClient</c> (2021–2394).</summary>
public static class VmPUserPredictClient
{
    public const int GsLevel = 1;
    public const int PstLive = 0;
    public const int CfPredicting = 1;
    public const int OfJustSpawned = 1;

    public static bool SinglePlayerTearsDownPrediction(bool netgame) => !netgame;

    public static bool SkipWhenNotInLevel(int gamestate, int gsLevel) => gamestate != gsLevel;

    public static bool SkipReservedServerSlot(bool netgame, bool reserved) => netgame && reserved;

    public static bool ConsolePlayerOutOfRange(int consoleplayer, int maxPlayers) =>
        consoleplayer < 0 || consoleplayer >= maxPlayers;

    public static bool SkipWhenNoPawn(bool moNull) => moNull;

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

    public static int ReseedWeaponPspriteValue(int predictStart) => predictStart;

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

    public static bool HcdeSeatUsesAuthoritativeBase(bool baseValid) => baseValid;

    public static int LastPredictedFromBase(int baseSequence) => Math.Max(baseSequence, 0);

    public static bool LogPredictionStart(bool debugEnabled) => debugEnabled;

    public static bool SkipHeavyThinkWhenPaused(bool paused) => paused;

    public static bool RecordReplayDiag(bool netgame, bool debugEnabled) => netgame && debugEnabled;

    public static long AccumulateReplayYaw(long sum, int yaw) => sum + yaw;

    public static bool PortalDisplacementRequiresSector(bool sectorNull) => sectorNull;

    public static bool ApplyPortalOffsetWhenSectorPresent(bool sectorNull) => !sectorNull;

    public static int LocalCmdIndex(int tic, int ringSize) => tic % ringSize;

    public static bool WeaponTickForIndex(int tic, int watermark) => tic >= watermark;

    public static bool RubberbandSnapView(bool hitLimit) => hitLimit;

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
            weaponPspriteTic = Math.Max(weaponPspriteTic, predictEnd);
    }
}
