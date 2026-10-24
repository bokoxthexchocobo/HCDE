namespace HCDE.Net.Core;

/// <summary>
/// Cutscene start, ready vote, countdown, and advance from <c>d_net.cpp</c>
/// (<c>Net_StartCutscene</c> through <c>Net_AdvanceCutscene</c>).
/// </summary>
public sealed class CutsceneAdvanceSession
{
    public const int TicRate = LiveConstants.TicRate;

    private readonly CutsceneReadyCoordinator _readyCoordinator = new();

    public ulong ReadyMask => _readyCoordinator.ReadyMask;
    public int CountdownTics { get; private set; }
    public bool RunnerActive { get; set; }
    public CutsceneScreenSkipType ScreenSkipType { get; set; } = CutsceneScreenSkipType.Vote;

    public void StartCutscene(CutsceneStartOptions options)
    {
        _readyCoordinator.ClearReady();
        CountdownTics = options.ResolveCountdownTics(TicRate);
        options.OnInvasionCountdownStarted?.Invoke(CountdownTics);
    }

    public void PlayerReadiedUp(int player, int gameTic, IReadOnlyList<CutsceneParticipant> participants, bool netGame, bool demoPlayback)
    {
        if (!netGame || demoPlayback)
            return;

        var humans = CountHumanParticipants(participants);
        _readyCoordinator.ToggleReady(player, gameTic, humans, true);
    }

    public bool IsVoteSatisfied(
        CutsceneReadyType readyType,
        float readyPercent,
        IReadOnlyList<CutsceneParticipant> participants,
        int authoritySlot,
        Func<int, bool> isPlayerReady)
    {
        if (readyType == CutsceneReadyType.Anyone)
        {
            foreach (var p in participants)
            {
                if (p.IsParticipant && IsReadyBitSet(p.Slot))
                    return true;
            }

            return false;
        }

        if (readyType == CutsceneReadyType.HostOnly)
        {
            var host = ResolveReadyHost(participants, authoritySlot);
            return host >= 0 && IsReadyBitSet(host);
        }

        ulong mask = 0;
        ulong readyMask = 0;
        var totalReady = 0;
        var totalClients = 0;
        var humanReady = 0;
        var humanClients = 0;
        foreach (var p in participants)
        {
            if (!p.IsParticipant)
                continue;

            mask |= 1ul << p.Slot;
            totalClients++;
            var ready = isPlayerReady(p.Slot);
            if (ready)
            {
                readyMask |= 1ul << p.Slot;
                totalReady++;
            }

            if (!p.IsBot)
            {
                humanClients++;
                if (ready)
                    humanReady++;
            }
        }

        if (humanClients <= 1)
            return humanReady >= humanClients;

        if (totalClients <= 0)
            return true;

        if ((readyMask & mask) == mask)
            return true;

        var readyRatio = (float)totalReady / totalClients;
        return readyRatio >= readyPercent;
    }

    public bool IsPlayerReady(
        int player,
        bool demoPlayback,
        CutsceneReadyType readyType,
        IReadOnlyList<CutsceneParticipant> participants)
    {
        if (demoPlayback || readyType != CutsceneReadyType.Vote)
            return false;

        var participant = FindParticipant(participants, player);
        if (participant is null || !participant.Value.IsParticipant)
            return false;

        if (RunnerActive && ScreenSkipType == CutsceneScreenSkipType.Unskippable)
            return false;

        return participant.Value.IsBot || IsReadyBitSet(player);
    }

    /// <summary>Returns true when the cutscene should advance this tic.</summary>
    public bool TryTickAdvance(
        CutsceneReadyType readyType,
        float readyPercent,
        IReadOnlyList<CutsceneParticipant> participants,
        int authoritySlot,
        out CutsceneAdvanceReason reason)
    {
        reason = CutsceneAdvanceReason.None;
        if (!RunnerActive)
            return false;

        if (ScreenSkipType == CutsceneScreenSkipType.Unskippable)
            return false;

        bool Ready(int slot) => IsPlayerReady(slot, false, readyType, participants);
        if (IsVoteSatisfied(readyType, readyPercent, participants, authoritySlot, Ready))
        {
            reason = CutsceneAdvanceReason.ReadyVote;
            return true;
        }

        if (CountdownTics > 0)
        {
            CountdownTics--;
            if (CountdownTics <= 0)
            {
                reason = CutsceneAdvanceReason.CountdownExpired;
                return true;
            }
        }

        return false;
    }

    public CutsceneAdvanceEffects AdvanceCutscene(Action? onInvasionWaveStart = null)
    {
        _readyCoordinator.ClearReady();
        CountdownTics = 0;
        onInvasionWaveStart?.Invoke();
        return new CutsceneAdvanceEffects(AuthorityShouldEndScreenJob: true);
    }

    public static int ResolveReadyHost(IReadOnlyList<CutsceneParticipant> participants, int authoritySlot)
    {
        if (IsParticipantSlot(participants, authoritySlot))
            return authoritySlot;

        foreach (var p in participants)
        {
            if (p.IsParticipant)
                return p.Slot;
        }

        return -1;
    }

    public static int CountHumanParticipants(IReadOnlyList<CutsceneParticipant> participants)
    {
        var count = 0;
        foreach (var p in participants)
        {
            if (p.IsParticipant && !p.IsBot)
                count++;
        }

        return count;
    }

    public static bool IsCutsceneReadyParticipant(
        int player,
        bool playingInGame,
        bool reservedSlot,
        bool lateJoinPending) =>
        player >= 0
        && playingInGame
        && !reservedSlot
        && !lateJoinPending;

    private bool IsReadyBitSet(int player) => _readyCoordinator.IsParticipantReady(player, true);

    private static CutsceneParticipant? FindParticipant(IReadOnlyList<CutsceneParticipant> participants, int slot)
    {
        foreach (var p in participants)
        {
            if (p.Slot == slot)
                return p;
        }

        return null;
    }

    private static bool IsParticipantSlot(IReadOnlyList<CutsceneParticipant> participants, int slot)
    {
        var p = FindParticipant(participants, slot);
        return p is not null && p.Value.IsParticipant;
    }
}

public readonly record struct CutsceneParticipant(
    int Slot,
    bool PlayingInGame,
    bool ReservedSlot,
    bool LateJoinPending,
    bool IsBot)
{
    public bool IsParticipant => CutsceneAdvanceSession.IsCutsceneReadyParticipant(
        Slot, PlayingInGame, ReservedSlot, LateJoinPending);
}

public readonly record struct CutsceneStartOptions(
    bool DemoPlayback,
    bool NetGame,
    bool InvasionMode,
    float CutsceneCountdownSeconds,
    float InvasionCountdownSeconds,
    int InvasionCountdownTicsOverride,
    Action<int>? OnInvasionCountdownStarted = null)
{
    public int ResolveCountdownTics(int ticRate)
    {
        var invasionCountdown = InvasionCountdownSeconds > 0f;
        var shouldUseCountdown = !DemoPlayback
            && (InvasionMode ? invasionCountdown : CutsceneCountdownSeconds > 0f)
            && (NetGame || InvasionMode);
        if (!shouldUseCountdown)
            return 0;

        if (InvasionMode)
            return InvasionCountdownTicsOverride > 0
                ? InvasionCountdownTicsOverride
                : (int)Math.Ceiling(InvasionCountdownSeconds * ticRate);

        return (int)Math.Ceiling(CutsceneCountdownSeconds * ticRate);
    }
}

public enum CutsceneAdvanceReason
{
    None = 0,
    ReadyVote = 1,
    CountdownExpired = 2,
}

public readonly record struct CutsceneAdvanceEffects(bool AuthorityShouldEndScreenJob);
