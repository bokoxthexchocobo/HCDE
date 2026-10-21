namespace HCDE.Net.Core;

/// <summary><c>Net_TickInvasionMirrorState</c>, <c>Net_ClearBuffers</c>, and gate helpers from <c>d_net.cpp</c> (4328–4490).</summary>
public static class VmDNetClearBuffers
{
    public static bool ShouldDecrementInvasionStateTics(bool localInvasionAuthority, bool invasionDisabled) =>
        !localInvasionAuthority && !invasionDisabled;

    public static int NextInvasionStateTics(int invasionStateTics) =>
        invasionStateTics > 0 ? invasionStateTics - 1 : invasionStateTics;

    /// <summary>Native <c>ClientStates[i]</c> reset at <c>d_net.cpp</c> 4337–4372.</summary>
    public static bool ClientSlotPlayingInGameAfterClearNative() => false;

    public static int ClientSlotAppliedSequenceAfterClearNative() => -1;

    public static uint ClientSlotAuthorityEventReplayNextIdAfterClearNative() => 0;

    /// <summary>Native globals cleared at <c>d_net.cpp</c> 4375–4410.</summary>
    public static bool GlobalNetGameAfterClearNative() => false;

    public static bool GlobalPendingLocalHealthRepairValidAfterClearNative() => false;

    public static int GlobalPredictionEndCapTicAfterClearNative() => int.MaxValue;

    public static int GlobalFullLatencyCycleAfterClearNative(int maxSendTics) => maxSendTics * 3;

    public static bool ShouldSetConsolePlayerInGameOnClear(bool serverReservedSlot0) =>
        !serverReservedSlot0;

    public static bool IsCutsceneReadyParticipant(
        int player,
        int maxPlayers,
        bool playingInGame,
        bool serverReservedSlot,
        bool lateJoinPending) =>
        player >= 0
        && player < maxPlayers
        && playingInGame
        && !serverReservedSlot
        && !lateJoinPending;

    public static bool IsTicGateClient(
        int player,
        int maxPlayers,
        bool networkClientInGame,
        bool serverReservedSlot,
        bool lateJoinPending) =>
        player >= 0
        && player < maxPlayers
        && networkClientInGame
        && !serverReservedSlot
        && !lateJoinPending;

    public static bool IsLateJoinSyncPending(int client, int maxPlayers, ulong lateJoinSyncPending) =>
        client >= 0
        && client < maxPlayers
        && (lateJoinSyncPending & (1ul << client)) != 0ul;
}
