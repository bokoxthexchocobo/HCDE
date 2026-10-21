namespace HCDE.Net.Core;

/// <summary><c>TryRunTics</c> frame scheduling and tic-gate math (<c>d_net.cpp</c> 7437–7830).</summary>
public static class VmDNetTryRunTicsScheduler
{
    public static int GetClientPredictionEndCapTicNative(int ack, int lead, int ticDup, bool dedicatedClient) =>
        dedicatedClient ? (Math.Max(ack, 0) + lead + 2) * Math.Max(ticDup, 1) : int.MaxValue;

    public static bool ShouldWaitForEnterTicNative(
        bool clCapFps,
        bool pauseExt,
        bool netgame,
        bool noInterpolate,
        bool menuAnimated,
        bool vidDontDowait,
        int vidMaxFps,
        bool vidVsync,
        bool appActive,
        bool lowerInBackground) =>
        (clCapFps || pauseExt || (!netgame && noInterpolate && !menuAnimated))
        && !(vidDontDowait && (vidMaxFps > 0 || vidVsync))
        && !(!netgame && !appActive && lowerInBackground);

    public static int ClampSingleticsTicsNative(int totalTics, bool singletics) =>
        totalTics > 1 && singletics ? 1 : totalTics;

    public static int ComputeLowestSequenceNative(
        IEnumerable<int> ticGateClients,
        Func<int, int> currentSequence,
        bool hasTicGateClient,
        int clientTic,
        int ticDup)
    {
        var lowest = int.MaxValue;
        foreach (var client in ticGateClients)
        {
            var seq = currentSequence(client);
            if (seq < lowest)
                lowest = seq;
        }

        return hasTicGateClient ? lowest : clientTic / Math.Max(ticDup, 1);
    }

    public static int ApplyDedicatedSnapshotFrontierNative(uint frontierGameTic, int ticDup, int currentLowest)
    {
        if (frontierGameTic == 0)
            return currentLowest;
        return (int)(frontierGameTic / (uint)Math.Max(ticDup, 1));
    }

    public static int ApplyDebugPredictionOffsetNative(int lowestSequence, int clDebugPrediction)
    {
        if (lowestSequence > clDebugPrediction)
            return lowestSequence - clDebugPrediction;
        return 0;
    }

    public static int ComputeAvailableTicsAuthorityNative(int totalTics) => totalTics;

    public static int ComputeAvailableTicsClientNative(
        int lowestSequence,
        int gameTic,
        int ticDup,
        bool dedicatedClient,
        int predictionLead,
        int clientTic)
    {
        var dup = Math.Max(ticDup, 1);
        var dedicatedBonus = dedicatedClient ? 1 : 0;
        var available = (lowestSequence - gameTic / dup) + 1 + dedicatedBonus;
        if (dedicatedClient && available <= 0)
        {
            var desiredLead = Math.Clamp(predictionLead, 0, 8);
            var currentLead = clientTic / dup - gameTic / dup;
            if (currentLead > 0 && currentLead <= desiredLead + 1)
                available = 1;
        }

        return available;
    }

    public static bool ShouldReseatLateJoinFrontierNative(
        int availableTics,
        int desiredLead,
        int ticRate,
        ref int stallFrames)
    {
        var trigger = desiredLead + ticRate / 2;
        if (availableTics > trigger)
            ++stallFrames;
        else
            stallFrames = 0;
        return stallFrames >= ticRate / 2;
    }

    public static (int gameTic, int clientTic) ReseatLateJoinClocksNative(
        int lowestSequence,
        int ticDup,
        int desiredLead,
        int gameTic,
        int clientTic)
    {
        var dup = Math.Max(ticDup, 1);
        var newGame = lowestSequence * dup;
        if (newGame <= gameTic)
            return (gameTic, clientTic);
        var newClient = Math.Max(clientTic, newGame + desiredLead * dup);
        return (newGame, newClient);
    }

    public static int RecomputeAvailableAfterReseatNative(int lowestSequence, int gameTic, int ticDup, bool dedicatedClient)
    {
        var dup = Math.Max(ticDup, 1);
        var bonus = dedicatedClient ? 1 : 0;
        return (lowestSequence - gameTic / dup) + 1 + bonus;
    }

    public static int InitialRunTicsNative(int totalTics, int availableTics) => Math.Min(totalTics, availableTics);

    public static bool ShouldZeroRunTicsDuringLevelStartNative(bool netgame, bool demoPlayback, int levelStartStatus, int levelStartDelay) =>
        netgame && !demoPlayback && levelStartStatus != 0 && levelStartDelay <= 0;

    public static int ApplyStabilityCatchUpNative(int runTics, int totalTics, int availableTics, int stabilityBuffer, bool singletics, bool authorityWallClock)
    {
        if (singletics || totalTics <= 0 || authorityWallClock)
            return runTics;
        if (totalTics < availableTics - stabilityBuffer)
            return runTics + 1;
        return runTics;
    }

    public static int ApplyDedicatedCatchUpBurstNative(int runTics, int availableTics, int totalTics, int stabilityBuffer, int ticRate, bool dedicatedClient)
    {
        if (!dedicatedClient)
            return runTics;
        var backlog = availableTics - runTics - stabilityBuffer;
        if (backlog > ticRate / 2)
            return Math.Min(availableTics, Math.Max(runTics, totalTics + 6));
        if (backlog > ticRate / 4)
            return Math.Min(availableTics, Math.Max(runTics, totalTics + 3));
        return runTics;
    }

    public static int ApplyPredictionLeadCapNative(int runTics, int clientTic, int gameTic, int ticDup, int desiredLead)
    {
        if (desiredLead <= 0)
            return runTics;
        var dup = Math.Max(ticDup, 1);
        var currentLead = clientTic / dup - gameTic / dup;
        var cap = Math.Max(0, currentLead - desiredLead) * dup;
        return runTics > cap ? cap : runTics;
    }
}
