namespace HCDE.Net.Core;

/// <summary>CCMD mirrors (<c>d_net.cpp</c> 10289–10460).</summary>
public static class VmDNetCcmdConsole
{
    public static bool RejectWithoutNetgameNative(bool netgame) => !netgame;

    public static bool RejectWithoutMultiplayerNative(bool multiplayer) => !multiplayer;

    public static bool RejectWithoutAuthorityNative(bool authority) => !authority;

    public static bool KickArgcValidNative(int argc) => argc >= 2;

    public static int InvasionSpotSummaryFallbackFlagNative(bool usingFallback) => usingFallback ? 1 : 0;

    public static int InvasionSpotSpawnReadyPrintNative(int gametic, int nextSpawnGameTic) =>
        gametic >= nextSpawnGameTic ? 1 : 0;

    public static bool ShouldIterateInvasionSpotNative(int index, int spotCount) =>
        index < spotCount;

    public static bool ShouldListClientLatencyNative(bool authoritySlot) => !authoritySlot;

    public static bool ShouldSkipSingleClientPingListNative(int clientCount) => clientCount <= 1;

    public static bool KickClientNumberInvalidNative(int clientNumber, int maxPlayers) =>
        clientNumber < 0 || clientNumber >= maxPlayers;

    public static bool ShouldEnqueueKickTargetNative(int target, int consolePlayer, int existingListSize, int foundIndex) =>
        target != consolePlayer && foundIndex >= existingListSize;

    public static bool ShouldKickClientInGameNative(int target, int consolePlayer, bool inGame) =>
        target != consolePlayer && inGame;

    public static bool ShouldReportClientNotInGameNative(bool inGame) => !inGame;

    public static bool MuteArgcValidNative(int argc) => argc >= 2;

    public static bool MutePlayerNumberInvalidNative(int playerNumber, int maxPlayers) =>
        playerNumber < 0 || playerNumber >= maxPlayers;

    public static bool ShouldEnqueueMuteTargetNative(int player, int consolePlayer, int listSize, int findIndex) =>
        player != consolePlayer && findIndex >= listSize;

    public static ulong MuteClientMaskNative(int player) => 1ul << player;

    public static ulong ApplyMuteClientMaskNative(ulong mutedClients, int player) =>
        mutedClients | MuteClientMaskNative(player);

    public static bool IsClientMutedNative(ulong mutedClients, int player) =>
        (mutedClients & MuteClientMaskNative(player)) != 0;

    public static bool ShouldMuteTargetPlayerNative(int player, int consolePlayer, bool inGame) =>
        player != consolePlayer && inGame;

    public static bool ShouldMuteAllExceptConsoleNative(int player, int consolePlayer, bool inGame) =>
        inGame && player != consolePlayer;

    public static bool ShouldRecordMutedPlayerNative(ulong mutedClients, int player) =>
        (mutedClients & MuteClientMaskNative(player)) != 0;

    public static bool ListMutedShouldPrintEntryNative(ulong mutedClients, int player) =>
        ShouldRecordMutedPlayerNative(mutedClients, player);

    public static bool ShouldPrintMuteUsageNative(int argc) => !MuteArgcValidNative(argc);

    public static bool ShouldPrintKickUsageNative(int argc) => !KickArgcValidNative(argc);

    public static bool ShouldPrintNoMutedPlayersNative(bool found) => !found;

    public static bool ShouldRejectInvalidKickIntNative(bool validInt) => !validInt;

    public static bool ShouldRejectInvalidMuteIntNative(bool validInt) => !validInt;

    public static bool ShouldApplyMuteAllForPlayerNative(int player, int consolePlayer, bool inGame) =>
        ShouldMuteAllExceptConsoleNative(player, consolePlayer, inGame);

    public static bool ShouldSkipMuteWhenNotInGameNative(int player, int consolePlayer, bool inGame) =>
        !ShouldMuteTargetPlayerNative(player, consolePlayer, inGame);

    public static bool ShouldPushMuteWhenInGameNative(int player, int consolePlayer, bool inGame) =>
        ShouldMuteTargetPlayerNative(player, consolePlayer, inGame);

    public static bool ShouldEmitPingLineNative(bool authoritySlot, int clientCount) =>
        !authoritySlot && !ShouldSkipSingleClientPingListNative(clientCount);
}
