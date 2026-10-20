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
}
